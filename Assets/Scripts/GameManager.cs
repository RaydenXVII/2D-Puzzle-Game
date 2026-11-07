using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject clearText;
    [SerializeField] private float nextLevelDelay = 0.5f;

    private bool m_levelEnded;

    private void Start()
    {
        // Pastikan teks Game Clear tersembunyi saat stage dimulai
        if (clearText != null)
            clearText.SetActive(false);
    }

    public void GameOver()
    {
        // Kalau stage sudah selesai, abaikan (misalnya pemain menyentuh duri saat jeda pindah stage)
        if (m_levelEnded) return;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Dipanggil oleh Goal
    public void LevelComplete()
    {
        if (m_levelEnded) return;   // cegah terpanggil dua kali
        m_levelEnded = true;

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(LoadSceneAfterDelay(nextIndex));
        else
            GameClear();   // tidak ada scene berikutnya: ini stage terakhir
    }

    public void GameClear()
    {
        if (clearText != null)
            clearText.SetActive(true);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    private IEnumerator LoadSceneAfterDelay(int buildIndex)
    {
        yield return new WaitForSeconds(nextLevelDelay);
        SceneManager.LoadScene(buildIndex);
    }
}