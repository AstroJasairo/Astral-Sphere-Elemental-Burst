using System.Collections.Generic;
using UnityEngine;

// Bảng nâng cấp chung (trượt từ cạnh phải): một hàng cho mỗi nâng cấp trong MetaConfig
public class SharedUpgradePanel : MonoBehaviour
{
    [SerializeField] private UpgradeRowView rowPrefab;
    [SerializeField] private Transform rowsParent;

    private readonly List<UpgradeRowView> rows = new List<UpgradeRowView>();
    private readonly List<SharedUpgradeEntry> entries = new List<SharedUpgradeEntry>();

    private void Awake()
    {
        MetaConfig cfg = MetaProgress.Config;
        if (cfg == null) return;

        foreach (SharedUpgradeEntry entry in cfg.sharedUpgrades)
        {
            SharedUpgradeType type = entry.type;                   // Sao chép để hàm bên dưới nhớ đúng loại
            UpgradeRowView row = Instantiate(rowPrefab, rowsParent);
            row.Init(() => MetaProgress.TryUpgradeShared(type));
            rows.Add(row);
            entries.Add(entry);
        }
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

    private void Refresh()
    {
        int wallet = MetaProgress.Gold;

        for (int i = 0; i < rows.Count; i++)
        {
            SharedUpgradeEntry e = entries[i];
            int cost = MetaProgress.GetSharedUpgradeCost(e.type);

            string value = $"+{MetaProgress.GetSharedBonus(e.type) * 100f:0.#}%";
            if (cost >= 0)
                value += $"  >  +{MetaProgress.GetNextSharedBonus(e.type) * 100f:0.#}%";

            rows[i].Set(e.displayName, e.description,
                        $"Lv {MetaProgress.GetSharedLevel(e.type)}/{MetaProgress.MaxLevel}", value, cost, wallet, e.icon);
        }
    }
}