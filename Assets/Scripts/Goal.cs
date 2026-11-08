using System.Collections;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Lock")]
    [SerializeField] private bool startsLocked = false;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
    [SerializeField] private AudioClip unlockSfx;
    [SerializeField, Range(0f, 1f)] private float unlockVolume = 0.8f;

    private SpriteRenderer m_sprite;
    private Color m_unlockedColor;
    private Vector3 m_baseScale;
    private bool m_locked;
    private bool m_reached;

    private void Awake()
    {
        m_sprite = GetComponentInChildren<SpriteRenderer>();
        m_baseScale = transform.localScale;
        m_locked = startsLocked;

        if (m_sprite != null)
        {
            m_unlockedColor = m_sprite.color;
            if (m_locked)
                m_sprite.color = lockedColor;
        }
    }

    // Dipanggil oleh KeyPickup
    public void Unlock()
    {
        if (!m_locked) return;
        m_locked = false;

        if (m_sprite != null)
            m_sprite.color = m_unlockedColor;

        if (unlockSfx != null)
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(unlockSfx, pos, unlockVolume);
        }

        StartCoroutine(PulseRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryReach(other);
    }

    // Stay: kalau pemain sudah berada di Goal saat kunci diambil, Goal tetap terpicu
    private void OnTriggerStay2D(Collider2D other)
    {
        TryReach(other);
    }

    private void TryReach(Collider2D other)
    {
        if (m_locked || m_reached) return;
        if (!other.CompareTag("Player")) return;

        m_reached = true;

        PlayerManager player = other.GetComponentInParent<PlayerManager>();
        if (player != null)
            player.Freeze();

        gameManager.LevelComplete();
    }

    private IEnumerator PulseRoutine()
    {
        const float duration = 0.25f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(t / duration * Mathf.PI);   // naik lalu turun
            transform.localScale = m_baseScale * (1f + 0.4f * k);
            yield return null;
        }

        transform.localScale = m_baseScale;
    }
}