using UnityEngine;

// Dữ liệu cho từng cấp của vũ khí (Lv1 đến Lv5)
[System.Serializable]
public class WeaponLevel
{
    public int projectileCount = 1;   // Kim loại: tăng số dao mỗi lần phóng
    public float radius = 1.5f;       // Lửa: bán kính Aura
    // Các vũ khí khác sẽ thêm trường riêng sau (bán kính, góc nón, thời gian trói...)
}

// Dữ liệu một vũ khí. Tạo bằng: Assets > Create > Astral Sphere > Weapon Data
[CreateAssetMenu(menuName = "Astral Sphere/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public ElementType element;

    [Header("Chiến đấu")]
    public float baseDamage = 10f;
    public float cooldown = 1f;        // Giây giữa hai lần tấn công
    public float targetRange = 8f;     // Tầm tìm mục tiêu

    [Header("Đạn (chỉ dùng cho vũ khí dạng Projectile)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 2f;
    public int pierce = 0;             // Số kẻ địch xuyên qua thêm (0 = trúng 1 là biến mất)

    [Header("Thông số theo cấp: phần tử 0 = Lv1 ... phần tử 4 = Lv5")]
    public WeaponLevel[] levels = new WeaponLevel[5];

    // Thêm vào class WeaponData (giữ nguyên các trường cũ)
    [Header("Hiển thị và prefab")]
    public Sprite icon;
    [TextArea] public string description;          // Mô tả khi nhận vũ khí mới
    [TextArea] public string upgradeDescription;   // Mô tả khi nâng cấp
    public GameObject weaponPrefab;                // Prefab chứa script vũ khí (ProjectileWeapon, AuraWeapon...)
}