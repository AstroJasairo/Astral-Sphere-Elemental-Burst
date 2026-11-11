using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Một icon buff/debuff: viền (đổi màu theo loại) + hình bên trong + lớp tối quét theo thời gian + ô số stack (DOT)
public class BuffIconView : MonoBehaviour
{
    [SerializeField] private Image borderImage;       // Khung ngoài, được tô xanh lá hoặc đỏ
    [SerializeField] private Image iconImage;         // Hình bên trong
    [SerializeField] private Image timerImage;        // Lớp tối quét dần (Image Type = Filled, Radial 360)
    [SerializeField] private TMP_Text labelText;      // Chữ thay thế khi chưa có hình (tùy chọn)
    [SerializeField] private GameObject stackBox;     // Ô vuông nhỏ góc dưới phải (tùy chọn)
    [SerializeField] private TMP_Text stackText;      // Số stack bên trong ô

    private int shownStacks = -1;

    public void Setup(BuffVisual visual, Color borderColor, Sprite iconOverride = null)
    {
        borderImage.color = borderColor;

        Sprite sprite = iconOverride != null ? iconOverride : visual.icon;   // ưu tiên icon truyền vào
        bool hasIcon = sprite != null;
        iconImage.sprite = sprite;
        iconImage.enabled = hasIcon;                   // Chưa có hình thì ẩn ô hình và hiện nhãn chữ (vd "ACT")
        ShowLabel(!hasIcon, visual.shortLabel);
        ResetStacks();
    }

    // Dùng khi loại buff chưa có trong BuffCatalog, và cho DOT
    public void SetupFallback(string label, Color borderColor, Sprite iconOverride = null)
    {
        borderImage.color = borderColor;
        iconImage.sprite = iconOverride;
        iconImage.enabled = iconOverride != null;
        ShowLabel(iconOverride == null, label);
        ResetStacks();
    }

    // remaining01 = 1 lúc mới nhận buff, về 0 khi hết. Lớp tối lớn dần theo thời gian đã trôi qua.
    public void SetRemaining(float remaining01)
    {
        timerImage.fillAmount = 1f - remaining01;
    }

    // Chỉ hiện ô khi từ 2 stack trở lên. Chỉ đổi chữ khi số đổi (không sinh chuỗi mỗi frame).
    public void SetStacks(int stacks)
    {
        if (stacks == shownStacks) return;
        shownStacks = stacks;
        bool show = stacks >= 2;
        if (stackBox != null) stackBox.SetActive(show);
        if (show && stackText != null) stackText.text = stacks.ToString();
    }

    // Icon không có đồng hồ (vd Miễn nhiễm của BOSS): lớp tối luôn trống
    public void SetNoTimer()
    {
        timerImage.fillAmount = 0f;
    }

    private void ResetStacks()
    {
        shownStacks = -1;
        SetStacks(0);                                  // Icon lấy lại từ kho luôn bắt đầu không có ô stack
    }

    private void ShowLabel(bool show, string text)
    {
        if (labelText == null) return;
        labelText.gameObject.SetActive(show);
        labelText.text = text;
    }
}