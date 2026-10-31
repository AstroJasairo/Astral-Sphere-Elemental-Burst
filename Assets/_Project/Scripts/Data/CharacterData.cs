using System.Collections.Generic;
using UnityEngine;

// Tiến trình một chỉ số của một nhân vật: bonus và chi phí theo từng cấp 1 đến 10 (chỉ số mảng = cấp - 1)
[System.Serializable]
public class StatProgression
{
    public StatType stat;
    public float[] bonusByLevel = new float[10];   // Bonus ở cấp đó (cấp 1 thường bằng 0)
    public int[] costByLevel = new int[10];        // Vàng cần để LÊN TỚI cấp đó (cấp 1 = 0)
}

// Một đặc quyền mở khóa ở một mốc sao
[System.Serializable]
public class StarPerk
{
    public string id;               // Tên định danh duy nhất, ví dụ "lightning_start"
    public string replacesId;       // id của perk bị thay thế (để trống nếu không thay ai)
    public int requiredStar;        // Cần bao nhiêu sao để mở khóa
    public PerkType type;
    public ElementType element;     // Nguyên tố bị ảnh hưởng (dùng cho 2 loại bonus)
    public WeaponData weapon;       // Dùng cho StartWeapon
    public int weaponLevel = 1;     // Dùng cho StartWeapon: cấp khởi đầu
    public float bonus;             // Dùng cho 2 loại bonus: 0.5 = +50%
    public string description;      // Mô tả hiệu quả để hiện trong menu, ví dụ "+50% sát thương Lôi"
}

// Dữ liệu nhân vật. Tạo bằng: Assets > Create > Astral Sphere > Character Data
[CreateAssetMenu(menuName = "Astral Sphere/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Meta: định danh và nâng cấp vĩnh viễn")]
    public string id;                                   // Duy nhất, dùng làm khóa lưu. Ví dụ "acronomia". KHÔNG đổi sau khi phát hành
    public int[] starCosts = new int[5];                // Vàng để lên sao 1, 2, 3, 4, 5
    public List<StatProgression> statProgressions = new List<StatProgression>();

    public StatProgression GetProgression(StatType stat)
    {
        foreach (StatProgression p in statProgressions)
            if (p.stat == stat) return p;
        return null;
    }

    public string characterName;
    public Sprite icon;

    [Header("Màu sắc nhân vật")]
    public Color primaryColor = new Color(1f, 0.75f, 0.1f);      // Màu chính: thanh mana khi All Out Assault (sau này: giao diện, hiệu ứng...)
    public Color secondaryColor = Color.white;                   // Màu phụ: dành cho tương lai

    [Header("Chỉ số cơ bản riêng của nhân vật (lv0, trước khi nâng ở menu)")]
    public float baseMaxHp = 100f;
    public float baseAtk = 1f;
    public float baseDef = 0f;
    public float baseMoveSpeed = 5f;
    public float baseMagnetRadius = 1.5f;

    [Header("Đặc quyền theo sao (0 đến 5)")]
    public List<StarPerk> perks = new List<StarPerk>();

    // Trả về các perk THỰC SỰ đang hoạt động ở số sao hiện tại
    public List<StarPerk> GetActivePerks(int star)
    {
        // Bước 1: lấy tất cả perk đã đủ điều kiện sao
        var unlocked = new List<StarPerk>();
        foreach (StarPerk p in perks)
            if (p.requiredStar <= star) unlocked.Add(p);

        // Bước 2: gom id của các perk đã bị perk khác (đang mở khóa) thay thế
        var replaced = new HashSet<string>();
        foreach (StarPerk p in unlocked)
            if (!string.IsNullOrEmpty(p.replacesId)) replaced.Add(p.replacesId);

        // Bước 3: giữ lại những perk không bị thay thế
        var active = new List<StarPerk>();
        foreach (StarPerk p in unlocked)
            if (!replaced.Contains(p.id)) active.Add(p);
        return active;
    }

    // Chuột phải vào tiêu đề asset > chọn mục này để điền sẵn số mặc định, khỏi nhập tay 6 x 20 con số
    [ContextMenu("Fill Default Meta Progression")]
    private void FillDefaultMetaProgression()
    {
        starCosts = new[] { 1000, 3000, 8000, 20000, 50000 };

        // Mức tăng mỗi cấp, theo thứ tự StatType: Hp, Mp, Atk, Def, Spd, Mag
        // Hp và Def cộng thẳng; còn lại là phần trăm (0.04 = 4%)
        float[] perLevel = { 10f, 0.02f, 0.04f, 2f, 0.02f, 0.06f };

        statProgressions = new List<StatProgression>();
        for (int s = 0; s < perLevel.Length; s++)
        {
            var p = new StatProgression { stat = (StatType)s, bonusByLevel = new float[10], costByLevel = new int[10] };
            for (int i = 0; i < 10; i++)
            {
                p.bonusByLevel[i] = perLevel[s] * i;           // Cấp 1 (i = 0) không có bonus
                p.costByLevel[i] = i == 0 ? 0 : 50 * i * i;    // Lên cấp 2 tốn 50, cấp 10 tốn 4050
            }
            statProgressions.Add(p);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}