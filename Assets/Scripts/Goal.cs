using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Cari PlayerManager di objek ini atau induknya (kalau collider ada di child)
        PlayerManager player = other.GetComponentInParent<PlayerManager>();
        if (player != null)
            player.Freeze();

        gameManager.LevelComplete();
    }
}