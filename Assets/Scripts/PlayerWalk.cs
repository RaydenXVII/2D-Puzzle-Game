using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalk : MonoBehaviour
{
    public InputActionAsset inputActions;
    public float walkSpeed = 6f;

    private InputAction m_moveAction;
    private Rigidbody2D m_rb;
    private float m_moveX;

    private void Awake()
    {
        m_moveAction = inputActions.FindAction("Player/Move", true);
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        m_moveAction.Enable();
    }

    private void OnDisable()
    {
        m_moveAction.Disable();
    }

    private void Update()
    {
        m_moveX = m_moveAction.ReadValue<Vector2>().x;
    }

    private void FixedUpdate()
    {
        m_rb.linearVelocity = new Vector2(m_moveX * walkSpeed, m_rb.linearVelocity.y);
    }
}