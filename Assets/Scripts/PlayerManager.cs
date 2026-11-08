using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float baseGravity = 3f;

    [Header("Ground Check")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float groundCheckDistance = 0.05f;

    [Header("Bounce Wall")]
    [SerializeField] private string bounceTag = "Bounce";
    [SerializeField] private float bounceCooldown = 0.1f;

    [Header("Game Over")]
    [SerializeField] private string trapTag = "Trap";
    [SerializeField] private int blinkCount = 6;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float delayBeforeRestart = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip flipSfx;
    [SerializeField] private AudioClip deathSfx;
    [SerializeField, Range(0f, 1f)] private float flipVolume = 0.8f;
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.05f;

    private InputAction m_moveAction;
    private InputAction m_flipAction;
    private Rigidbody2D m_rb;
    private Collider2D m_collider;
    private SpriteRenderer m_sprite;
    private AudioSource m_audio;
    private ContactFilter2D m_castFilter;
    private readonly RaycastHit2D[] m_hits = new RaycastHit2D[8];

    private Color m_originalColor;
    private float m_moveX;
    private float m_lastBounceTime = -999f;
    private bool m_flipPressed;
    private bool m_gravityFlipped;
    private bool m_isDead;
    private bool m_isFrozen;

    private Vector2 GravityDirection => m_gravityFlipped ? Vector2.up : Vector2.down;
    private bool IsInactive => m_isDead || m_isFrozen;

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

        if (gameManager == null)
            Debug.LogWarning("PlayerManager: GameManager belum diisi di Inspector.", this);

        // Pakai AudioSource yang sudah ada, atau buat otomatis
        m_audio = GetComponent<AudioSource>();
        if (m_audio == null)
            m_audio = gameObject.AddComponent<AudioSource>();
        m_audio.playOnAwake = false;
        m_audio.spatialBlend = 0f;   // suara 2D, volume tidak tergantung jarak ke kamera

        m_originalColor = m_sprite.color;
        m_rb.gravityScale = baseGravity;

        m_castFilter = new ContactFilter2D();
        m_castFilter.useLayerMask = false;
        m_castFilter.useTriggers = false;
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
        if (IsInactive || Time.timeScale == 0f) return;

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
        if (IsInactive) return;

        m_rb.linearVelocity = new Vector2(m_moveX * walkSpeed, m_rb.linearVelocity.y);

        if (m_flipPressed && IsGrounded())
            FlipGravity();

        m_flipPressed = false;
    }

    // Objek solid (bukan trigger): trap yang berupa collider biasa dan dinding Bounce
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(trapTag))
            Die();
        else if (collision.collider.CompareTag(bounceTag))
            BounceOffWall();
    }

    // Trap yang berupa trigger, dan bounce bar
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(trapTag))
            Die();
        else if (other.CompareTag(bounceTag))
            BounceOffWall();
    }

    private bool IsGrounded()
    {
        Vector2 gravityDir = GravityDirection;
        int count = m_collider.Cast(gravityDir, m_castFilter, m_hits, groundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            if (m_hits[i].collider.CompareTag(groundTag) &&
                Vector2.Dot(m_hits[i].normal, -gravityDir) > 0.5f)
                return true;
        }
        return false;
    }

    private void FlipGravity()
    {
        m_gravityFlipped = !m_gravityFlipped;
        m_rb.gravityScale = m_gravityFlipped ? -baseGravity : baseGravity;
        m_sprite.flipY = m_gravityFlipped;

        PlayFlipSfx();
    }

    private void PlayFlipSfx()
    {
        if (flipSfx == null) return;

        m_audio.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        m_audio.PlayOneShot(flipSfx, flipVolume);
    }

    private void BounceOffWall()
    {
        if (IsInactive) return;

        if (Time.time - m_lastBounceTime < bounceCooldown) return;
        m_lastBounceTime = Time.time;

        FlipGravity();
    }

    public void Freeze()
    {
        if (m_isDead || m_isFrozen) return;
        m_isFrozen = true;

        m_moveX = 0f;
        m_flipPressed = false;
        m_rb.linearVelocity = Vector2.zero;
        m_rb.simulated = false;
    }

    private void Die()
    {
        if (IsInactive) return;
        m_isDead = true;

        if (deathSfx != null)
            m_audio.PlayOneShot(deathSfx, flipVolume);

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        m_moveX = 0f;
        m_rb.linearVelocity = Vector2.zero;
        m_rb.simulated = false;

        for (int i = 0; i < blinkCount; i++)
        {
            m_sprite.color = (i % 2 == 0) ? blinkColor : m_originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }

        m_sprite.color = blinkColor;
        yield return new WaitForSeconds(delayBeforeRestart);

        if (gameManager != null)
            gameManager.GameOver();
    }
}