using System.Collections.Generic;
using UnityEngine;

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
}

// Dữ liệu nhân vật. Tạo bằng: Assets > Create > Astral Sphere > Character Data
[CreateAssetMenu(menuName = "Astral Sphere/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite icon;

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
}