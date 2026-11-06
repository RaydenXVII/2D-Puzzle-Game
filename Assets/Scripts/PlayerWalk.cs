using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalk : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float baseGravity = 3f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.05f;

    private InputAction m_moveAction;
    private InputAction m_flipAction;
    private Rigidbody2D m_rb;
    private Collider2D m_collider;
    private SpriteRenderer m_sprite;
    private ContactFilter2D m_groundFilter;
    private readonly RaycastHit2D[] m_hits = new RaycastHit2D[4];

    private float m_moveX;
    private bool m_flipPressed;
    private bool m_gravityFlipped;

    private Vector2 GravityDirection => m_gravityFlipped ? Vector2.up : Vector2.down;

    private void Awake()
    {
        m_moveAction = inputActions.FindAction("Player/Move", true);
        m_flipAction = inputActions.FindAction("Player/Flip", true);

        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponentInChildren<Collider2D>();
        m_sprite = GetComponentInChildren<SpriteRenderer>();

        if (m_collider == null || m_sprite == null)
        {
            enabled = false;
            return;
        }

        m_rb.gravityScale = baseGravity;

        m_groundFilter = new ContactFilter2D();
        m_groundFilter.SetLayerMask(groundLayer);
        m_groundFilter.useLayerMask = true;
    }

    private void OnEnable()
    {
        m_moveAction.Enable();
        m_flipAction.Enable();
    }

    private void OnDisable()
    {
        m_moveAction.Disable();
        m_flipAction.Disable();
    }

    private void Update()
    {
        m_moveX = m_moveAction.ReadValue<Vector2>().x;

        if (m_moveX > 0.01f)
            m_sprite.flipX = false;
        else if (m_moveX < -0.01f)
            m_sprite.flipX = true;

        if (m_flipAction.WasPressedThisFrame())
            m_flipPressed = true;
    }

    private void FixedUpdate()
    {
        m_rb.linearVelocity = new Vector2(m_moveX * walkSpeed, m_rb.linearVelocity.y);

        if (m_flipPressed && IsGrounded())
            FlipGravity();

        m_flipPressed = false;
    }

    private bool IsGrounded()
    {
        Vector2 gravityDir = GravityDirection;
        int count = m_collider.Cast(gravityDir, m_groundFilter, m_hits, groundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            if (Vector2.Dot(m_hits[i].normal, -gravityDir) > 0.5f)
                return true;
        }
        return false;
    }

    private void FlipGravity()
    {
        m_gravityFlipped = !m_gravityFlipped;
        m_rb.gravityScale = m_gravityFlipped ? -baseGravity : baseGravity;
        m_sprite.flipY = m_gravityFlipped;
    }
}