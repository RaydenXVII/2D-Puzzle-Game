using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    private static GameHUD s_instance;

    private GameObject m_root;
    private TextMeshProUGUI m_text;

    // Dibuat otomatis di setiap sesi Play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (s_instance != null) return;
        new GameObject("GameHUD").AddComponent<GameHUD>();
    }

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);

        BuildUI();

        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (s_instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Build index 0 = Main Menu: reset data dan sembunyikan HUD
        bool isMenu = scene.buildIndex == 0;

        if (isMenu)
            GameSession.Reset();
        else
            GameSession.StartIfNeeded();

        m_root.SetActive(!isMenu);
    }

    private void Update()
    {
        if (!m_root.activeSelf) return;

        if (GameSession.Running)
            GameSession.ElapsedTime += Time.deltaTime;

        int stageIndex = SceneManager.GetActiveScene().buildIndex;
        int totalStages = SceneManager.sceneCountInBuildSettings - 1;

        string stagePart = stageIndex > 0 ? $"Stage {stageIndex}/{totalStages}" : "Stage";

        m_text.text = $"{stagePart}    {GameSession.FormatTime(GameSession.ElapsedTime)}    Mati: {GameSession.Deaths}";
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("HUDCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;   // di bawah toast screenshot (999)

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        m_root = new GameObject("HUDPanel", typeof(RectTransform), typeof(Image));
        m_root.transform.SetParent(canvasGO.transform, false);

        var rt = m_root.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);   // pojok kiri atas
        rt.anchoredPosition = new Vector2(24f, -24f);
        rt.sizeDelta = new Vector2(660f, 60f);

        var bg = m_root.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        bg.raycastTarget = false;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(m_root.transform, false);

        var textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(20f, 0f);
        textRt.offsetMax = new Vector2(-20f, 0f);

        m_text = textGO.GetComponent<TextMeshProUGUI>();
        m_text.fontSize = 32f;
        m_text.alignment = TextAlignmentOptions.MidlineLeft;
        m_text.color = Color.white;
        m_text.textWrappingMode = TextWrappingModes.NoWrap;
        m_text.overflowMode = TextOverflowModes.Overflow;
        m_text.raycastTarget = false;

        m_root.SetActive(false);
    }
}