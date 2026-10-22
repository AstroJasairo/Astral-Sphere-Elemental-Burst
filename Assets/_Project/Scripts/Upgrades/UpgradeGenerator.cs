using System.Collections.Generic;
using UnityEngine;

public class UpgradeGenerator : MonoBehaviour
{
    [SerializeField] private WeaponInventory inventory;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private WeaponData[] allWeapons;          // Toàn bộ vũ khí có thể xuất hiện
    [SerializeField] private StatUpgradeData[] statUpgrades;   // 6 lựa chọn tăng chỉ số

    // Tạo danh sách lựa chọn ngẫu nhiên, không trùng nhau
    public List<UpgradeOption> Generate(int count)
    {
        var candidates = new List<UpgradeOption>();

        // 1) Nâng cấp vũ khí đang có (chưa tối đa Lv5)
        foreach (WeaponBase w in inventory.Weapons)
        {
            if (w.IsMaxLevel) continue;
            WeaponBase weapon = w;   // Sao chép ra biến cục bộ để lambda bên dưới nhớ đúng vũ khí
            candidates.Add(new UpgradeOption
            {
                Title = $"{w.Data.weaponName} Lv{w.Level + 1}",
                Description = w.Data.upgradeDescription,
                Icon = w.Data.icon,
                Apply = () => weapon.LevelUp()
            });
        }

        // 2) Vũ khí mới (chỉ khi còn ô trống và chưa sở hữu)
        if (inventory.HasFreeSlot)
        {
            foreach (WeaponData d in allWeapons)
            {
                if (inventory.Has(d)) continue;
                WeaponData data = d;
                candidates.Add(new UpgradeOption
                {
                    Title = data.weaponName,
                    Description = data.description,
                    Icon = data.icon,
                    Apply = () => inventory.AddWeapon(data)
                });
            }
        }

        // 3) Tăng chỉ số
        foreach (StatUpgradeData s in statUpgrades)
        {
            StatUpgradeData stat = s;
            candidates.Add(new UpgradeOption
            {
                Title = stat.displayName,
                Description = stat.description,
                Icon = stat.icon,
                Apply = () => stats.ApplyStat(stat.stat, stat.amount)
            });
        }

        // Xáo trộn (Fisher-Yates) rồi lấy count phần tử đầu
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }
        return candidates.GetRange(0, Mathf.Min(count, candidates.Count));
    }
}