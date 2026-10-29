using UnityEngine;
using UnityEngine.UI;

public enum SlideDirection { FromBottom, FromRight }

public class SlidePanel : MonoBehaviour
{
    [SerializeField] private SlideDirection direction = SlideDirection.FromBottom;
    [SerializeField] private float speed = 14f;
    [SerializeField] private GameObject dimOverlay;   // Lớp tối nằm phía sau, bấm vào để đóng (tùy chọn)

    public bool IsOpen { get; private set; }

    private RectTransform rect;
    private Vector2 openPosition;
    private Vector2 closedPosition;
    private Vector2 target;
    private bool initialized;

    private void Awake()
    {
        Init();
    }

    // Khởi tạo lười: Open/Close có thể bị gọi trước Awake khi object cha đang tắt
    private void Init()
    {
        if (initialized) return;
        initialized = true;

        rect = (RectTransform)transform;
        openPosition = rect.anchoredPosition;                 // Vị trí thiết kế trong Editor = vị trí mở

        // Vị trí đóng: đẩy ra ngoài màn hình đúng bằng kích thước bảng
        closedPosition = direction == SlideDirection.FromBottom
            ? openPosition + new Vector2(0f, -rect.sizeDelta.y)
            : openPosition + new Vector2(rect.sizeDelta.x, 0f);

        rect.anchoredPosition = closedPosition;
        target = closedPosition;

        if (dimOverlay != null)
        {
            dimOverlay.SetActive(false);
            Button dimButton = dimOverlay.GetComponent<Button>();
            if (dimButton != null) dimButton.onClick.AddListener(Close);   // Bấm vào vùng tối là đóng
        }
    }

    public void Open()
    {
        Init();
        IsOpen = true;
        target = openPosition;
        if (dimOverlay != null) dimOverlay.SetActive(true);
    }

    public void Close()
    {
        Init();
        IsOpen = false;
        target = closedPosition;
        if (dimOverlay != null) dimOverlay.SetActive(false);
    }

    // Đóng ngay không hiệu ứng (dùng khi rời màn hình)
    public void CloseImmediate()
    {
        Close();
        rect.anchoredPosition = closedPosition;
    }

    private void Update()
    {
        if ((rect.anchoredPosition - target).sqrMagnitude < 0.01f) return;

        // Làm mượt kiểu "chậm dần khi tới đích"; dùng thời gian không bị ảnh hưởng bởi timeScale
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, target, t);
    }
}