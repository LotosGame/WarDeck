using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 1.5f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 12f;
    [SerializeField] private float smoothness = 10f;

    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 1f;

    private Camera cam;
    private float targetZoom;
    private Vector3 targetPosition;
    private float initialZPosition;

    private Vector2 lastMousePosition;
    private bool isDragging;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            targetZoom = cam.orthographicSize;
            targetPosition = transform.position;
            initialZPosition = transform.position.z;
        }
    }

    private void Update()
    {
        if (Mouse.current == null || cam == null) return;

        HandleZoom();
        HandleDrag();

        // Плавная интерполяция размера и позиции
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * smoothness);

        Vector3 finalPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothness);
        finalPos.z = initialZPosition; // Фиксируем Z
        transform.position = finalPos;
    }

    private void HandleZoom()
    {
        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            float scrollDirection = Mathf.Sign(scrollValue);
            float newZoom = Mathf.Clamp(targetZoom - scrollDirection * zoomSpeed, minZoom, maxZoom);

            if (!Mathf.Approximately(newZoom, targetZoom))
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector3 worldPointBefore = cam.ScreenToWorldPoint(mouseScreenPos);

                targetZoom = newZoom;

                float oldSize = cam.orthographicSize;
                cam.orthographicSize = targetZoom;

                Vector3 worldPointAfter = cam.ScreenToWorldPoint(mouseScreenPos);
                cam.orthographicSize = oldSize;

                Vector3 positionOffset = worldPointBefore - worldPointAfter;
                targetPosition += positionOffset;
                targetPosition.z = initialZPosition;
            }
        }
    }

    private void HandleDrag()
    {
        // Проверяем зажатие колесика мыши (Middle Button)
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 deltaMouse = currentMousePos - lastMousePosition;

            // Конвертируем пиксельное смещение мыши в мировые координаты с учетом масштаба камеры
            Vector3 worldDelta = cam.ScreenToWorldPoint(new Vector3(deltaMouse.x, deltaMouse.y, cam.nearClipPlane))
                               - cam.ScreenToWorldPoint(Vector3.zero);

            targetPosition -= worldDelta * dragSpeed;
            targetPosition.z = initialZPosition;

            lastMousePosition = currentMousePos;
        }
    }
}