using System.Collections.Generic;
using UnityEngine;

// Một nâng cấp chung (menu "Nâng cấp chung")
[System.Serializable]
public class SharedUpgradeEntry
{
    public SharedUpgradeType type;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public float[] bonusByLevel = new float[10];   // Bonus ở từng cấp 1 đến 10 (cấp 1 = 0)
    public int[] costByLevel = new int[10];        // Vàng để lên tới cấp đó
}

// Cấu hình meta toàn game. Tạo bằng: Assets > Create > Astral Sphere > Meta Config
// VÀ phải đặt trong thư mục tên "Resources", tên file "MetaConfig".
[CreateAssetMenu(menuName = "Astral Sphere/Meta Config")]
public class MetaConfig : ScriptableObject
{
    public List<CharacterData> characters = new List<CharacterData>();
    public AllOutAssaultData allOutAssault;
    public List<SharedUpgradeEntry> sharedUpgrades = new List<SharedUpgradeEntry>();

    public CharacterData GetCharacter(string id)
    {
        foreach (CharacterData c in characters)
            if (c != null && c.id == id) return c;
        return null;
    }

    public SharedUpgradeEntry GetShared(SharedUpgradeType type)
    {
        foreach (SharedUpgradeEntry e in sharedUpgrades)
            if (e.type == type) return e;
        return null;
    }

    [ContextMenu("Fill Default Shared Upgrades")]
    private void FillDefaultSharedUpgrades()
    {
        sharedUpgrades = new List<SharedUpgradeEntry>
        {
            Make(SharedUpgradeType.Gold, "Vàng", "Tăng vàng nhận được mỗi lần nhặt", 0.05f),
            Make(SharedUpgradeType.Crystal, "Pha lê", "Tăng pha lê nhận được mỗi lần nhặt", 0.05f),
            Make(SharedUpgradeType.HealthPotion, "Bình máu", "Tăng lượng máu hồi khi nhặt bình máu", 0.02f),
            Make(SharedUpgradeType.SpeedPotion, "Bình tăng tốc", "Tăng thêm tốc độ di chuyển khi nhặt bình tốc độ", 0.03f),
            Make(SharedUpgradeType.RagePotion, "Bình cuồng nộ", "Tăng thêm ATK khi nhặt bình cuồng nộ", 0.03f),
            Make(SharedUpgradeType.Shield, "Khiên", "Tăng lá chắn tạo ra khi nhặt khiên", 0.06f),
            Make(SharedUpgradeType.Magnet, "Nam châm", "Tăng thêm bán kính hút khi nhặt nam châm", 0.1f),
            Make(SharedUpgradeType.ManaPotion, "Bình mana", "Tăng tỉ lệ rơi bình mana", 0.15f),
            Make(SharedUpgradeType.ScoreBoost, "Tăng điểm", "Tăng thêm điểm khi nhặt vật phẩm tăng điểm", 0.05f),
        };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static SharedUpgradeEntry Make(SharedUpgradeType type, string name, string description, float perLevel)
    {
        var e = new SharedUpgradeEntry
        {
            type = type,
            displayName = name,
            description = description,
            bonusByLevel = new float[10],
            costByLevel = new int[10]
        };
        for (int i = 0; i < 10; i++)
        {
            e.bonusByLevel[i] = perLevel * i;               // Cấp 1 không có bonus
            e.costByLevel[i] = i == 0 ? 0 : 75 * i * i;     // Lên cấp 2 tốn 75, cấp 10 tốn 6075
        }
        return e;
    }
}