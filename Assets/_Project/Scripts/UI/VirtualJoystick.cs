using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Joystick ảo dạng "nổi": xuất hiện tại điểm chạm, kéo để lấy hướng di chuyển.
/// Gắn script này vào object JoystickArea.
/// </summary>
public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background; // Vòng tròn lớn
    [SerializeField] private RectTransform handle;     // Núm điều khiển
    [SerializeField] private float handleRange = 100f; // Núm di chuyển tối đa bao xa (pixel UI)

    /// <summary>Hướng di chuyển, độ dài từ 0 đến 1. Script khác chỉ cần đọc giá trị này.</summary>
    public Vector2 Direction { get; private set; }

    private RectTransform area;  // Vùng nhận chạm (chính là object này)

    private void Awake()
    {
        area = (RectTransform)transform;
        background.gameObject.SetActive(false); // Ẩn joystick lúc chưa chạm
    }

    // Khi ngón tay chạm xuống màn hình
    public void OnPointerDown(PointerEventData eventData)
    {
        // Đổi vị trí chạm (tọa độ màn hình) sang tọa độ cục bộ của vùng area
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            area, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        background.anchoredPosition = localPoint;   // Đặt joystick tại điểm chạm
        background.gameObject.SetActive(true);
        OnDrag(eventData);                          // Cập nhật ngay, tránh giật khung đầu
    }

    // Khi ngón tay kéo
    public void OnDrag(PointerEventData eventData)
    {
        // Tính vị trí ngón tay so với tâm của Background
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out Vector2 local);

        // Giới hạn núm không đi quá bán kính cho phép
        Vector2 offset = Vector2.ClampMagnitude(local, handleRange);
        handle.anchoredPosition = offset;

        // Chuẩn hóa về 0..1 để dùng làm hướng và cường độ
        Direction = offset / handleRange;
    }

    // Khi nhấc ngón tay
    public void OnPointerUp(PointerEventData eventData)
    {
        Direction = Vector2.zero;                  // Dừng di chuyển
        handle.anchoredPosition = Vector2.zero;    // Đưa núm về tâm
        background.gameObject.SetActive(false);    // Ẩn joystick
    }
}