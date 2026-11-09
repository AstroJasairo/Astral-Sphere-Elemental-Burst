using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Một trang tab skill: icon, tên, cấp, mô tả. Gắn lên PassivePage và ActivePage (chọn Kind khác nhau).
public class SkillInfoView : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;
    [SerializeField] private SkillKind kind;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text descriptionText;

    // Trang chỉ bật khi tab đang chọn nên OnEnable chạy mỗi lần chuyển tới => luôn có dữ liệu mới
    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;

        SkillDataBase data = MetaProgress.GetSkillData(c, kind);
        if (data == null)                                           // Nhân vật chưa có skill (vd Acronomia)
        {
            nameText.text = "Chưa có kỹ năng";
            levelText.text = "";
            descriptionText.text = "";
            icon.enabled = false;
            return;
        }

        int level = MetaProgress.GetSkillLevel(c.id, kind);
        nameText.text = data.skillName;
        icon.sprite = data.icon;
        icon.enabled = data.icon != null;
        levelText.text = level > 0 ? $"Lv {level}/{MetaProgress.MaxSkillLevel}" : "Chưa mở khóa";

        // Chưa mở khóa thì xem trước hiệu ứng cấp 1
        string effect = level > 0 ? data.DescribeLevel(level) : "Cấp 1: " + data.DescribeLevel(1);
        descriptionText.text = string.IsNullOrEmpty(data.description) ? effect : data.description + "\n\n" + effect;
    }
}