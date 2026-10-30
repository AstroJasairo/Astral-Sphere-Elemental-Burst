using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapCardView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button cardButton;
    [SerializeField] private Button rankingButton;             // Nút xếp hạng (màu vàng)
    [SerializeField] private GameObject lockOverlay;           // Lớp phủ "Sắp ra mắt"
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Color placeholderColor = new Color(0.35f, 0.38f, 0.45f);

    public int Offset { get; set; }                            // Độ lệch so với thẻ giữa (-2, -1, 0, +1, +2), do bánh xe gán
    public MapData Map { get; private set; }

    public event Action<MapCardView> Clicked;
    public event Action<MapData> RankingClicked;

    private RectTransform rect;
    private int boundIndex = -1;

    private void Awake()
    {
        rect = (RectTransform)transform;
        cardButton.onClick.AddListener(() => Clicked?.Invoke(this));
        rankingButton.onClick.AddListener(() => RankingClicked?.Invoke(Map));
    }

    // Gán nội dung map cho thẻ. Chỉ làm việc khi map thay đổi nên gọi mỗi frame cũng không tốn.
    public void Bind(MapData map, int mapIndex)
    {
        if (boundIndex == mapIndex && Map == map) return;
        boundIndex = mapIndex;
        Map = map;

        nameText.text = map.displayName;
        background.sprite = map.background;
        background.color = map.background != null ? Color.white : placeholderColor;
        lockOverlay.SetActive(!map.isAvailable);
    }

    // Đặt vị trí, kích thước, độ mờ theo khoảng cách tới tâm bánh xe
    public void SetPose(Vector2 position, float scale, float alpha)
    {
        rect.anchoredPosition = position;
        rect.localScale = new Vector3(scale, scale, 1f);
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = alpha > 0.15f;            // Thẻ gần như trong suốt thì không nhận chạm
    }
}