using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenus : MonoBehaviour
{
    private static GameMenus s_instance;

    private static readonly Color Dim        = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color BoxColor   = new Color(0.165f, 0.086f, 0.031f, 0.96f);
    private static readonly Color BoxBorder  = new Color(0.784f, 0.518f, 0.247f);
    private static readonly Color BtnNormal  = new Color(0.784f, 0.518f, 0.247f);
    private static readonly Color BtnHover   = new Color(0.878f, 0.631f, 0.353f);
    private static readonly Color BtnPressed = new Color(0.604f, 0.373f, 0.165f);
    private static readonly Color BtnBorder  = new Color(0.290f, 0.141f, 0.063f);
    private static readonly Color LabelColor = new Color(1f, 0.945f, 0.839f);
    private static readonly Color TitleRed   = new Color(0.878f, 0.271f, 0.227f);
    private static readonly Color TitleCyan  = new Color(0.361f, 0.902f, 1f);

    private Transform m_canvasRoot;
    private GameObject m_pausePanel;
    private GameObject m_clearPanel;
    private TextMeshProUGUI m_clearStats;
    private GameObject m_ownEventSystem;
    private bool m_paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (s_instance != null) return;
        new GameObject("GameMenus").AddComponent<GameMenus>();
    }

    // Dipanggil oleh GameManager saat stage terakhir selesai
    public static void ShowClear()
    {
        if (s_instance != null)
            s_instance.OpenClear();
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
        Time.timeScale = 1f;
        m_paused = false;
        m_pausePanel.SetActive(false);
        m_clearPanel.SetActive(false);

        EnsureEventSystem();
    }

    private void Update()
    {
        // Menu pause hanya untuk stage, bukan Main Menu (build index 0)
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

        if (m_clearPanel.activeSelf) return;
        SetPaused(!m_paused);
    }

    private void SetPaused(bool paused)
    {
        m_paused = paused;
        Time.timeScale = paused ? 0f : 1f;
        m_pausePanel.SetActive(paused);
    }

    private void OpenClear()
    {
        if (m_clearPanel.activeSelf) return;

        SetPaused(false);
        m_clearStats.text =
            "Waktu   " + GameSession.FormatTime(GameSession.ElapsedTime) +
            "\nMati   " + GameSession.Deaths;
        m_clearPanel.SetActive(true);
    }

    private void RestartStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        GameSession.Reset();
        SceneManager.LoadScene(0);
    }

    private void PlayAgain()
    {
        Time.timeScale = 1f;
        GameSession.Reset();
        SceneManager.LoadScene(1);
    }

    // Pastikan tepat satu EventSystem agar tombol bisa diklik
    private void EnsureEventSystem()
    {
        EventSystem[] systems = FindObjectsByType<EventSystem>();

        bool hasOther = false;
        foreach (EventSystem s in systems)
        {
            if (m_ownEventSystem == null || s.gameObject != m_ownEventSystem)
                hasOther = true;
        }

        if (hasOther)
        {
            if (m_ownEventSystem != null)
            {
                Destroy(m_ownEventSystem);
                m_ownEventSystem = null;
            }
            return;
        }

        if (m_ownEventSystem == null)
        {
            m_ownEventSystem = new GameObject("EventSystem (GameMenus)",
                typeof(EventSystem), typeof(InputSystemUIInputModule));
            m_ownEventSystem.transform.SetParent(transform, false);
        }
    }

    // ---------- Membangun UI ----------

    private void BuildUI()
    {
        var canvasGO = new GameObject("MenusCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;   // di atas HUD (500), di bawah toast screenshot (999)

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        m_canvasRoot = canvasGO.transform;

        // Menu pause
        RectTransform pauseBox = CreatePanel("PausePanel", new Vector2(760, 640), "PAUSE", TitleRed, 230f);
        m_pausePanel = pauseBox.parent.gameObject;
        CreateButton(pauseBox, "Lanjut",       new Vector2(0, 60),   () => SetPaused(false));
        CreateButton(pauseBox, "Ulangi stage", new Vector2(0, -60),  RestartStage);
        CreateButton(pauseBox, "Menu utama",   new Vector2(0, -180), GoToMenu);

        // Layar Game Clear
        RectTransform clearBox = CreatePanel("ClearPanel", new Vector2(760, 700), "GAME CLEAR", TitleCyan, 270f);
        m_clearPanel = clearBox.parent.gameObject;
        m_clearStats = CreateText(clearBox, "Stats", "", 52f, LabelColor, new Vector2(0, 100), new Vector2(700, 220));
        CreateButton(clearBox, "Main lagi",  new Vector2(0, -100), PlayAgain);
        CreateButton(clearBox, "Menu utama", new Vector2(0, -220), GoToMenu);

        m_pausePanel.SetActive(false);
        m_clearPanel.SetActive(false);
    }

    private RectTransform CreatePanel(string name, Vector2 boxSize, string title, Color titleColor, float titleY)
    {
        RectTransform overlay = NewUI(name, m_canvasRoot);
        Stretch(overlay);
        var dim = overlay.gameObject.AddComponent<Image>();
        dim.color = Dim;   // raycastTarget aktif: klik tidak tembus ke game

        RectTransform box = NewUI("Box", overlay);
        SetCenter(box, Vector2.zero, boxSize);
        var boxImg = box.gameObject.AddComponent<Image>();
        boxImg.color = BoxColor;
        boxImg.raycastTarget = false;
        var outline = box.gameObject.AddComponent<Outline>();
        outline.effectColor = BoxBorder;
        outline.effectDistance = new Vector2(6, -6);

        CreateText(box, "Title", title, 90f, titleColor, new Vector2(0, titleY), new Vector2(700, 110));
        return box;
    }

    private void CreateButton(RectTransform parent, string label, Vector2 pos, UnityAction onClick)
    {
        RectTransform rt = NewUI("Button_" + label, parent);
        SetCenter(rt, pos, new Vector2(500, 90));

        var img = rt.gameObject.AddComponent<Image>();
        img.color = Color.white;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock colors = btn.colors;
        colors.normalColor      = BtnNormal;
        colors.highlightedColor = BtnHover;
        colors.selectedColor    = BtnHover;
        colors.pressedColor     = BtnPressed;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = BtnBorder;
        outline.effectDistance = new Vector2(5, -5);

        TextMeshProUGUI text = CreateText(rt, "Label", label, 44f, LabelColor, Vector2.zero, new Vector2(500, 90));
        Stretch(text.rectTransform);
    }

    private TextMeshProUGUI CreateText(RectTransform parent, string name, string content,
        float size, Color color, Vector2 pos, Vector2 boxSize)
    {
        RectTransform rt = NewUI(name, parent);
        SetCenter(rt, pos, boxSize);

        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void SetCenter(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}