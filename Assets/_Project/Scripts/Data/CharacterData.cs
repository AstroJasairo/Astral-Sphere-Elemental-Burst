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

    [Header("Mana của All Out Assault (riêng từng nhân vật)")]
    public float maxMana = 100f;

    [Header("Phòng thủ nguyên tố (0.2 = kháng 20%, âm = yếu hệ)")]
    public ElementResistances elementResist = new ElementResistances();
    [Tooltip("Tick nguyên tố nào thì nhân vật miễn nhiễm hoàn toàn sát thương nguyên tố đó")]
    public ElementMask elementImmunity = ElementMask.None;

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

    [Header("Kỹ năng (để trống nếu nhân vật chưa có)")]
    [SerializeField, HideInInspector] private PassiveSkillData passiveSkill;   // CŨ (GĐ10): tự chuyển vào danh sách bên dưới
    public List<PassiveSkillData> passiveSkills = new List<PassiveSkillData>();
    public ActiveSkillData activeSkill;

    public int PassiveCount => passiveSkills != null ? passiveSkills.Count : 0;

    public PassiveSkillData GetPassive(int index)
        => (passiveSkills != null && index >= 0 && index < passiveSkills.Count) ? passiveSkills[index] : null;

    // Chạy trong Editor khi asset được nạp hoặc sửa
    private void OnValidate()
    {
        if (passiveSkills == null) passiveSkills = new List<PassiveSkillData>();

        if (passiveSkill != null)                                    // Chuyển dữ liệu cũ: passive đơn lẻ thành phần tử đầu danh sách
        {
            if (!passiveSkills.Contains(passiveSkill)) passiveSkills.Insert(0, passiveSkill);
            passiveSkill = null;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // Nhắc nhở id thiếu hoặc trùng (id là khóa lưu tiến trình nên không được trống/trùng trong cùng nhân vật)
        var seen = new HashSet<string>();
        foreach (PassiveSkillData p in passiveSkills)
        {
            if (p == null) continue;
            if (string.IsNullOrEmpty(p.id)) Debug.LogWarning($"{name}: passive '{p.name}' chưa có id.", this);
            else if (!seen.Add(p.id)) Debug.LogWarning($"{name}: id passive '{p.id}' bị trùng.", this);
        }
    }
}