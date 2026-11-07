using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearText;

    private void Start()
    {
        // Pastikan teks Game Clear tersembunyi saat game mulai
        if (clearText != null)
            clearText.SetActive(false);
    }

    public void GameOver()
    {
        // Muat ulang scene yang sedang aktif. Memakai build index, jadi tidak ada risiko salah ketik nama scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameClear()
    {
        if (clearText != null)
            clearText.SetActive(true);
    }
}