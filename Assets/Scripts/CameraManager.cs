using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float leftCameraX = 0f;
    [SerializeField] private float rightCameraX = 18f;
    [SerializeField] private string playerTag = "Player";

    private Collider2D m_trigger;

    private void Awake()
    {
        m_trigger = GetComponent<Collider2D>();

        // Kalau slot kamera kosong, pakai kamera ber-tag MainCamera
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    // OnTriggerExit2D: kamera pindah hanya setelah pemain benar-benar melewati batas,
    // bukan saat baru menyentuhnya (menghindari bug "touch and jump" di tutorial)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Karena ini saat keluar, pemain yang berada di kanan pusat trigger berarti keluar ke kanan
        bool exitedRight = other.transform.position.x > m_trigger.bounds.center.x;

        MoveCameraToX(exitedRight ? rightCameraX : leftCameraX);
    }

    private void MoveCameraToX(float x)
    {
        Vector3 pos = targetCamera.transform.position;
        pos.x = x;   // Y dan Z tetap, supaya kamera 2D tidak berubah kedalamannya
        targetCamera.transform.position = pos;
    }
}