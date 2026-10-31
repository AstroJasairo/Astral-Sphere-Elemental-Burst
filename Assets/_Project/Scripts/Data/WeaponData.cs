using UnityEngine;

// Dữ liệu cho từng cấp của vũ khí (Lv1 đến Lv5)
[System.Serializable]
public class WeaponLevel
{
    public int projectileCount = 1;   // Kim loại: tăng số dao mỗi lần phóng
    public float radius = 1.5f;       // Lửa: bán kính Aura
    public float explosionRadius = 0f;  // MỚI: bán kính nổ của đạn (Bóng tối). 0 = không nổ
    public int chainTargets = 0;        // MỚI: số kẻ địch mỗi lần giật (Lôi)
    public float coneAngle = 60f;        // MỚI. Phong: góc mở hình nón (độ)
    public float knockbackDistance = 0f; // MỚI. Phong: quãng đường đẩy lùi
    public float beamWidth = 0.6f;       // MỚI. Ánh sáng, Mộc: độ rộng tia
    public float rootDuration = 0f;      // MỚI. Mộc: thời gian trói (giây)
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

    [Header("Chuỗi sét (chỉ dùng cho vũ khí dạng Chain)")]
    public float chainRange = 3f;           // Khoảng cách tối đa để tia sét nhảy sang kẻ địch kế tiếp

    [Header("Bắn loạt (Kim loại). Lớn hơn 0: bắn lần lượt từng viên; bằng 0: bắn cùng lúc, tỏa quạt")]
    public float burstInterval = 0f;         // Khoảng cách giữa hai viên trong một loạt (giây)

    [Header("Quả cầu xoay (Linh hồn)")]
    public float orbitSpeed = 150f;          // Độ mỗi giây
    public float orbitRadius = 2f;           // Khoảng cách quả cầu tới người chơi
    public float hitInterval = 0.5f;         // Mỗi kẻ địch chỉ chịu sát thương từ quả cầu tối đa một lần mỗi khoảng này

    [Header("Tia (Ánh sáng, Mộc)")]
    public float beamLength = 10f;           // Chiều dài tia

    [Header("Thông số theo cấp: phần tử 0 = Lv1 ... phần tử 4 = Lv5")]
    public WeaponLevel[] levels = new WeaponLevel[5];

    // Thêm vào class WeaponData (giữ nguyên các trường cũ)
    [Header("Hiển thị và prefab")]
    public Sprite icon;
    [TextArea] public string description;          // Mô tả khi nhận vũ khí mới
    [TextArea] public string upgradeDescription;   // Mô tả khi nâng cấp
    public GameObject weaponPrefab;                // Prefab chứa script vũ khí (ProjectileWeapon, AuraWeapon...)
}