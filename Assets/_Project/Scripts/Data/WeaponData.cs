using UnityEngine;

// Dữ liệu cho từng cấp của vũ khí (Lv1 đến Lv5)
[System.Serializable]
public class WeaponLevel
{
    public float baseDamage = 0f;        // MỚI. Sát thương cơ bản ở cấp này. 0 = dùng WeaponData.baseDamage
    public int projectileCount = 1;      // Kim loại, Nước: số viên. Linh hồn: số quả cầu
    public float radius = 1.5f;          // Lửa, Đất, Phong, Clarius: bán kính
    public float explosionRadius = 0f;   // Bóng tối, Nước: bán kính nổ
    public int chainTargets = 0;         // Lôi: số kẻ địch mỗi lần giật
    public float coneAngle = 60f;        // Phong: góc mở hình nón (độ)
    public float knockbackDistance = 0f; // Phong: quãng đường đẩy lùi
    public float beamWidth = 0.6f;       // Ánh sáng, Plant: độ rộng tia
    public float rootDuration = 0f;      // Plant: thời gian trói (giây)
    public float duration = 0f;          // MỚI. Clarius: thời gian tồn tại của thanh kiếm (giây)
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

    [Header("Đặc trưng và Clarius")]
    [Tooltip("Vũ khí đặc trưng: KHÔNG bao giờ xuất hiện như 'vũ khí mới' trong màn chọn nâng cấp")]
    public bool isUnique = false;
    public float tickInterval = 0.5f;            // Clarius: khoảng cách giữa hai lần gây sát thương (n giây)

    [Header("Hỗ trợ cân bằng")]
    [Tooltip("Dùng cho menu chuột phải 'Fill Damage Per Level': mỗi cấp tăng bao nhiêu so với Base Damage (0.2 = +20%)")]
    public float damageGrowthPerLevel = 0.2f;

    // Chuột phải vào tiêu đề asset: điền sẵn sát thương của 5 cấp (Lv1 = Base Damage, mỗi cấp +damageGrowthPerLevel)
    [ContextMenu("Fill Damage Per Level")]
    private void FillDamagePerLevel()
    {
        for (int i = 0; i < levels.Length; i++)
            levels[i].baseDamage = Mathf.Round(baseDamage * (1f + damageGrowthPerLevel * i) * 100f) / 100f;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Thêm vào class WeaponData (giữ nguyên các trường cũ)
    [Header("Hiển thị và prefab")]
    public Sprite icon;
    [TextArea] public string description;          // Mô tả khi nhận vũ khí mới
    [TextArea] public string upgradeDescription;   // Mô tả khi nâng cấp
    public GameObject weaponPrefab;                // Prefab chứa script vũ khí (ProjectileWeapon, AuraWeapon...)

    [Header("Thông số theo cấp: phần tử 0 = Lv1 ... phần tử 4 = Lv5")]
    public WeaponLevel[] levels = new WeaponLevel[5];
}