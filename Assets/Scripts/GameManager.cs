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
        if (clearText != null)
            clearText.SetActive(false);
    }

    public void GameOver()
    {
        if (m_levelEnded) return;

        GameSession.Deaths++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Dipanggil oleh Goal
    public void LevelComplete()
    {
        if (m_levelEnded) return;
        m_levelEnded = true;

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(LoadSceneAfterDelay(nextIndex));
        else
            GameClear();
    }

    public void GameClear()
    {
        GameSession.Finish();   // hentikan timer di stage terakhir

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