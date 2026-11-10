using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Một trang tab skill: icon, tên, cấp, mô tả. PassivePage dùng chung cho mọi passive (đọc số thứ tự từ carousel).
public class SkillInfoView : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;
    [SerializeField] private CharacterInfoCarousel tabs;     // Cần cho trang Passive: biết đang xem passive thứ mấy
    [SerializeField] private SkillKind kind;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text headerText;            // Tùy chọn: "BỊ ĐỘNG 2/3" hoặc "CHỦ ĐỘNG"
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text descriptionText;

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        if (tabs != null) tabs.OnTabChanged += HandleTabChanged;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
        if (tabs != null) tabs.OnTabChanged -= HandleTabChanged;      // kiểm tra null: object có thể đã bị hủy khi đóng scene
    }

    private void HandleTabChanged(InfoTab tab) { Refresh(); }

    private void Refresh()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;

        int index = (kind == SkillKind.Passive && tabs != null) ? tabs.Current.index : 0;

        if (headerText != null)
            headerText.text = kind == SkillKind.Passive ? $"BỊ ĐỘNG {index + 1}/{Mathf.Max(1, c.PassiveCount)}" : "CHỦ ĐỘNG";

        SkillDataBase data = MetaProgress.GetSkillData(c, kind, index);
        if (data == null)                                           // Nhân vật chưa có skill (vd Acronomia)
        {
            nameText.text = "Chưa có kỹ năng";
            levelText.text = "";
            descriptionText.text = "";
            icon.enabled = false;
            return;
        }

        int level = MetaProgress.GetSkillLevel(c, kind, index);
        nameText.text = data.skillName;
        icon.sprite = data.icon;
        icon.enabled = data.icon != null;
        levelText.text = level > 0 ? $"Lv {level}/{MetaProgress.MaxSkillLevel}" : "Chưa mở khóa";

        // Chưa mở khóa thì xem trước hiệu ứng cấp 1
        string effect = level > 0 ? data.DescribeLevel(level) : "Cấp 1: " + data.DescribeLevel(1);
        descriptionText.text = string.IsNullOrEmpty(data.description) ? effect : data.description + "\n\n" + effect;
    }
}