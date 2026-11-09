using System.Text;
using TMPro;
using UnityEngine;

// Nhóm nâng cấp skill trong panel trượt từ dưới: một hàng nâng cấp + danh sách hiệu ứng 5 cấp.
// Gắn lên PassiveGroup và ActiveGroup (chọn Kind khác nhau).
public class SkillUpgradeSection : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;
    [SerializeField] private SkillKind kind;
    [SerializeField] private UpgradeRowView row;              // Dùng lại prefab hàng nâng cấp có sẵn
    [SerializeField] private TMP_Text effectsText;            // Danh sách hiệu ứng theo cấp

    private readonly StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        row.Init(OnRowClicked);
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
    }

    private void OnRowClicked()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;
        SkillDataBase data = MetaProgress.GetSkillData(c, kind);
        if (data == null) return;

        int next = MetaProgress.GetSkillLevel(c.id, kind) + 1;
        ConfirmDialog.Ask($"Nâng cấp {data.skillName}", $"Nâng lên Lv {next}?",
                          MetaProgress.GetSkillUpgradeCost(c, kind),
                          () => MetaProgress.TryUpgradeSkill(c, kind));
    }

    private void Refresh()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;

        SkillDataBase data = MetaProgress.GetSkillData(c, kind);
        SkillUpgradeStatus status = MetaProgress.GetSkillUpgradeStatus(c, kind);

        row.gameObject.SetActive(data != null);
        if (data == null)
        {
            effectsText.text = "Nhân vật này chưa có kỹ năng.";
            return;
        }

        int level = MetaProgress.GetSkillLevel(c.id, kind);
        string locked = status == SkillUpgradeStatus.NeedMoreStars ? $"Cần {level + 1} sao" : null;

        row.Set(data.skillName, data.description, $"Lv {level}/{MetaProgress.MaxSkillLevel}", "",
                MetaProgress.GetSkillUpgradeCost(c, kind), MetaProgress.Gold, data.icon, locked);

        effectsText.text = BuildEffectsText(data, level);
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