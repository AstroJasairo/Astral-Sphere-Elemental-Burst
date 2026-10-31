using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Một icon buff/debuff: viền (đổi màu theo loại) + hình bên trong + lớp tối quét theo thời gian
public class BuffIconView : MonoBehaviour
{
    [SerializeField] private Image borderImage;       // Khung ngoài, được tô xanh lá hoặc đỏ
    [SerializeField] private Image iconImage;         // Hình bên trong
    [SerializeField] private Image timerImage;        // Lớp tối quét dần (Image Type = Filled, Radial 360)
    [SerializeField] private TMP_Text labelText;      // Chữ thay thế khi chưa có hình (tùy chọn)

    public void Setup(BuffVisual visual, Color borderColor)
    {
        borderImage.color = borderColor;

        bool hasIcon = visual.icon != null;
        iconImage.sprite = visual.icon;
        iconImage.enabled = hasIcon;                   // Chưa có hình thì ẩn ô hình và hiện nhãn chữ
        ShowLabel(!hasIcon, visual.shortLabel);
    }

    // Dùng khi loại buff chưa được khai báo trong BuffCatalog
    public void SetupFallback(string label, Color borderColor)
    {
        borderImage.color = borderColor;
        iconImage.enabled = false;
        ShowLabel(true, label);
    }

    // remaining01 = 1 lúc mới nhận buff, về 0 khi hết. Lớp tối lớn dần theo thời gian đã trôi qua.
    public void SetRemaining(float remaining01)
    {
        timerImage.fillAmount = 1f - remaining01;
    }

    private void ShowLabel(bool show, string text)
    {
        if (labelText == null) return;
        labelText.gameObject.SetActive(show);
        labelText.text = text;
    }
}