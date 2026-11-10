using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Nhóm nâng cấp skill trong panel trượt từ dưới: một hàng nâng cấp + danh sách hiệu ứng 5 cấp.
// PassiveGroup dùng chung cho mọi passive (đọc số thứ tự từ carousel), ActiveGroup cho skill chủ động.
public class SkillUpgradeSection : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;
    [SerializeField] private CharacterInfoCarousel tabs;      // Cần cho nhóm Passive
    [SerializeField] private SkillKind kind;
    [SerializeField] private UpgradeRowView row;              // Dùng lại prefab hàng nâng cấp có sẵn
    [SerializeField] private TMP_Text effectsText;            // Danh sách hiệu ứng theo cấp

    private readonly StringBuilder builder = new StringBuilder();

    private int CurrentIndex => (kind == SkillKind.Passive && tabs != null) ? tabs.Current.index : 0;

    private void Awake()
    {
        row.Init(OnRowClicked);
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        if (tabs != null) tabs.OnTabChanged += HandleTabChanged;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
        if (tabs != null) tabs.OnTabChanged -= HandleTabChanged;
    }

    private void HandleTabChanged(InfoTab tab) { Refresh(); }

    private void OnRowClicked()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;
        int index = CurrentIndex;                                  // Chốt tại lúc bấm
        SkillDataBase data = MetaProgress.GetSkillData(c, kind, index);
        if (data == null) return;

        int next = MetaProgress.GetSkillLevel(c, kind, index) + 1;
        ConfirmDialog.Ask($"Nâng cấp {data.skillName}", $"Nâng lên Lv {next}?",
                          MetaProgress.GetSkillUpgradeCost(c, kind, index),
                          () => MetaProgress.TryUpgradeSkill(c, kind, index));
    }

    private void Refresh()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;

        int index = CurrentIndex;
        SkillDataBase data = MetaProgress.GetSkillData(c, kind, index);
        SkillUpgradeStatus status = MetaProgress.GetSkillUpgradeStatus(c, kind, index);

        row.gameObject.SetActive(data != null);
        if (data == null)
        {
            effectsText.text = "Nhân vật này chưa có kỹ năng.";
        }
        else
        {
            int level = MetaProgress.GetSkillLevel(c, kind, index);
            string locked = status == SkillUpgradeStatus.NeedMoreStars ? $"Cần {level + 1} sao" : null;

            row.Set(data.skillName, data.description, $"Lv {level}/{MetaProgress.MaxSkillLevel}", "",
                    MetaProgress.GetSkillUpgradeCost(c, kind, index), MetaProgress.Gold, data.icon, locked);

            effectsText.text = BuildEffectsText(data, level);
        }

        // Nội dung đổi độ dài (nhất là khi chuyển giữa các passive): ép panel cha tính lại chiều cao
        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    // Cấp đã đạt: trắng. Cấp tiếp theo: vàng cam. Cấp chưa tới: xám
    private string BuildEffectsText(SkillDataBase data, int level)
    {
        builder.Clear();
        for (int lv = 1; lv <= MetaProgress.MaxSkillLevel; lv++)
        {
            string color = lv <= level ? "#FFFFFF" : (lv == level + 1 ? "#FFC060" : "#777777");
            builder.Append("<color=").Append(color).Append(">Lv ").Append(lv).Append(": ")
                   .Append(data.DescribeLevel(lv)).Append("</color>\n\n");
        }
        return builder.ToString();
    }
}