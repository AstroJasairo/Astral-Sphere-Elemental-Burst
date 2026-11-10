using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bảng nâng cấp nhân vật (trượt từ đáy lên): Nâng sao + 6 hàng Nâng chỉ số + hiệu quả theo sao
public class CharacterUpgradePanel : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;
    [SerializeField] private UpgradeRowView starRow;               // Hàng "Nâng sao"
    [SerializeField] private TMP_Text starEffectsText;             // Danh sách hiệu quả theo mốc sao
    [SerializeField] private UpgradeRowView statRowPrefab;         // Prefab một hàng
    [SerializeField] private Transform statRowsParent;             // Nơi chứa 6 hàng
    [SerializeField] private RectTransform contentRoot;

    [Header("Tab (theo CharacterInfoCarousel)")]
    [SerializeField] private CharacterInfoCarousel tabs;
    [SerializeField] private GameObject statsGroup;            // Chứa Promotion, StarRow, StarEffectsText, Text, StatRows
    [SerializeField] private GameObject passiveGroup;
    [SerializeField] private GameObject activeGroup;
    [SerializeField] private ScrollRect scroll;                // Cuộn về đầu khi đổi tab

    private static readonly StatType[] Stats = (StatType[])Enum.GetValues(typeof(StatType));

    private readonly List<UpgradeRowView> statRows = new List<UpgradeRowView>();
    private readonly StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        starRow.Init(() =>
        {
            CharacterData c = carousel.Current;
            if (c == null) return;
            ConfirmDialog.Ask("Nâng sao", $"Nâng {c.characterName} lên {MetaProgress.GetStars(c.id) + 1} sao?",
                              MetaProgress.GetStarUpgradeCost(c), () => MetaProgress.TryUpgradeStar(c));
        });

        foreach (StatType stat in Stats)
        {
            StatType captured = stat;
            UpgradeRowView row = Instantiate(statRowPrefab, statRowsParent);
            row.Init(() =>
            {
                CharacterData c = carousel.Current;
                if (c == null) return;
                ConfirmDialog.Ask($"Nâng {StatDisplay.Label(captured)}",
                                  $"Nâng lên Lv {MetaProgress.GetStatLevel(c.id, captured) + 1}?",
                                  MetaProgress.GetStatUpgradeCost(c, captured),
                                  () => MetaProgress.TryUpgradeStat(c, captured));
            });
            statRows.Add(row);
        }
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;                         // Mua gì, đổi nhân vật... đều làm mới
        if (tabs != null) tabs.OnTabChanged += ApplyTab;
        ApplyTab(tabs != null ? tabs.Current : InfoTab.Stats);
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
        if (tabs != null) tabs.OnTabChanged -= ApplyTab;         // kiểm tra null: object có thể đã bị hủy khi đóng scene
    }

    private void Refresh()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;

        int wallet = MetaProgress.Gold;
        int stars = MetaProgress.GetStars(c.id);

        // Hàng nâng sao
        starRow.Set("Nâng sao", $"Sao hiện tại: {stars}/{MetaProgress.MaxStars}",
                    $"{stars}/{MetaProgress.MaxStars}", "", MetaProgress.GetStarUpgradeCost(c), wallet, StatIcons.Star);
        starEffectsText.text = BuildStarEffectsText(c, stars);

        // 6 hàng chỉ số
        for (int i = 0; i < Stats.Length; i++)
        {
            StatType stat = Stats[i];
            int cost = MetaProgress.GetStatUpgradeCost(c, stat);

            string value = StatDisplay.FormatBonus(stat, MetaProgress.GetStatBonus(c, stat));
            if (cost >= 0)                                          // Chưa tối đa thì hiện thêm mức kế tiếp
                value += "  >  " + StatDisplay.FormatBonus(stat, MetaProgress.GetNextStatBonus(c, stat));

            statRows[i].Set(StatDisplay.Label(stat), StatDisplay.Describe(stat),
                            $"Lv {MetaProgress.GetStatLevel(c.id, stat)}/{MetaProgress.MaxLevel}", value, cost, wallet, StatIcons.Get(stat));
        }
        RebuildLayout();
    }

    // Danh sách hiệu quả theo từng mốc sao, tô 3 màu: đang hiệu lực / bị mốc cao thay thế / chưa mở khóa
    private string BuildStarEffectsText(CharacterData c, int stars)
    {
        List<StarPerk> active = c.GetActivePerks(stars);           // Chỉ tạo mới lúc làm mới, không phải mỗi frame
        builder.Clear();

        for (int tier = 0; tier <= MetaProgress.MaxStars; tier++)
        {
            foreach (StarPerk perk in c.perks)
            {
                if (perk.requiredStar != tier) continue;

                string color, suffix = "";
                if (perk.requiredStar > stars) color = "#777777";                 // Chưa mở khóa
                else if (!active.Contains(perk)) { color = "#A07040"; suffix = " (đã được thay thế)"; }
                else color = "#FFFFFF";                                            // Đang hiệu lực

                builder.Append("<color=").Append(color).Append(">Sao ").Append(tier).Append(": ")
                       .Append(perk.description).Append(suffix).Append("</color>\n");
            }
        }
        return builder.ToString();
    }

    // Bật đúng nhóm theo LOẠI tab đang chọn (mọi tab Passive dùng chung PassiveGroup)
    private void ApplyTab(InfoTab tab)
    {
        statsGroup.SetActive(tab.kind == InfoTabKind.Stats);
        passiveGroup.SetActive(tab.kind == InfoTabKind.Passive);
        activeGroup.SetActive(tab.kind == InfoTabKind.Active);
        RebuildLayout();                                         // Ép tính lại chiều cao trước khi đưa cuộn về đầu
        if (scroll != null) scroll.verticalNormalizedPosition = 1f;
    }

    private void RebuildLayout()
    {
        // Ép tính lại chiều cao ngay lập tức sau khi đổi chữ/số hàng,
        // để ScrollView biết đúng chiều cao nội dung ngay từ lần mở đầu tiên
        if (contentRoot != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
    }
}