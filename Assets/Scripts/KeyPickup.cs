using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private Goal goal;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 3f;

    private Vector3 m_startPos;
    private bool m_collected;

    private void Awake()
    {   
        m_startPos = transform.position;

        if (goal == null)
            Debug.LogWarning("KeyPickup: slot Goal belum diisi di Inspector.", this);
    }

    private void Update()
    {
        if (m_collected) return;

        // Kunci naik-turun pelan supaya terlihat bisa diambil
        transform.position = m_startPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_collected || !other.CompareTag("Player")) return;
        m_collected = true;

        if (goal != null)
            goal.Unlock();

        if (pickupSfx != null)
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSfx, pos, volume);
        }

        Destroy(gameObject);
    }
}