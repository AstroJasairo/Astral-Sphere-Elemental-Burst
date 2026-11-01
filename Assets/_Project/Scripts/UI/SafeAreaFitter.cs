using UnityEngine;

// Co một RectTransform (kéo giãn toàn màn hình) cho vừa với vùng an toàn của màn hình.
// Đặt các thành phần cần tránh tai thỏ (tiêu đề, nút lùi, HUD) làm con của object này.
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rect;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        rect = (RectTransform)transform;
        Apply();
    }

    // Vùng an toàn có thể đổi khi xoay màn hình hoặc đổi độ phân giải, nên kiểm tra mỗi frame (rất nhẹ)
    private void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
            Apply();
    }

    private void Apply()
    {
        Rect safe = Screen.safeArea;
        lastSafeArea = safe;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        if (Screen.width <= 0 || Screen.height <= 0) return;

        // Đổi pixel sang tỉ lệ 0..1 để dùng làm điểm neo
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}