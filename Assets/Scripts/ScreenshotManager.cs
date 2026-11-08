using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScreenshotManager : MonoBehaviour
{
    private const string FolderName = "Screenshots";
    private const string FilePrefix = "OniFlip";
    private const float ToastDuration = 2.5f;

    private static ScreenshotManager s_instance;

    private GameObject m_toastRoot;
    private TextMeshProUGUI m_toastText;
    private Coroutine m_toastRoutine;
    private bool m_isCapturing;

    // Membuat manager otomatis di setiap sesi Play, di scene mana pun
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (s_instance != null) return;
        new GameObject("ScreenshotManager").AddComponent<ScreenshotManager>();
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
        BuildToast();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f12Key.wasPressedThisFrame)
            StartCapture();
    }

    // Bisa dipanggil dari script lain: ScreenshotManager.Capture();
    public static void Capture()
    {
        if (s_instance != null)
            s_instance.StartCapture();
    }

    private void StartCapture()
    {
        if (m_isCapturing) return;
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        m_isCapturing = true;

        // Sembunyikan pesan lama supaya tidak ikut terfoto
        m_toastRoot.SetActive(false);

        // Tunggu frame selesai dirender (termasuk UI) sebelum mengambil gambar
        yield return new WaitForEndOfFrame();

        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

        try
        {
            string folder = Path.Combine(Application.persistentDataPath, FolderName);
            Directory.CreateDirectory(folder);

            string fileName = $"{FilePrefix}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
            string path = Path.Combine(folder, fileName);

            File.WriteAllBytes(path, tex.EncodeToPNG());

            Debug.Log("Screenshot disimpan: " + path);
            ShowToast("Screenshot disimpan: " + fileName);
        }
        catch (Exception e)
        {
            Debug.LogError("Gagal menyimpan screenshot: " + e.Message);
            ShowToast("Gagal menyimpan screenshot");
        }
        finally
        {
            if (tex != null) Destroy(tex);
            m_isCapturing = false;
        }
    }

    private void ShowToast(string message)
    {
        m_toastText.text = message;
        m_toastRoot.SetActive(true);

        if (m_toastRoutine != null) StopCoroutine(m_toastRoutine);
        m_toastRoutine = StartCoroutine(HideToastAfterDelay());
    }

    private IEnumerator HideToastAfterDelay()
    {
        // Realtime: tetap jalan walau Time.timeScale = 0
        yield return new WaitForSecondsRealtime(ToastDuration);
        m_toastRoot.SetActive(false);
    }

    private void BuildToast()
    {
        var canvasGO = new GameObject("ScreenshotToastCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;   // selalu di atas UI lain

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        m_toastRoot = new GameObject("Toast", typeof(RectTransform), typeof(Image));
        m_toastRoot.transform.SetParent(canvasGO.transform, false);

        var rt = m_toastRoot.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 60f);
        rt.sizeDelta = new Vector2(1000f, 64f);

        var bg = m_toastRoot.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        bg.raycastTarget = false;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(m_toastRoot.transform, false);

        var textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(16f, 0f);
        textRt.offsetMax = new Vector2(-16f, 0f);

        m_toastText = textGO.GetComponent<TextMeshProUGUI>();
        m_toastText.fontSize = 30f;
        m_toastText.alignment = TextAlignmentOptions.Center;
        m_toastText.color = Color.white;
        m_toastText.textWrappingMode = TextWrappingModes.NoWrap;
        m_toastText.overflowMode = TextOverflowModes.Overflow;
        m_toastText.raycastTarget = false;

        m_toastRoot.SetActive(false);
    }
}