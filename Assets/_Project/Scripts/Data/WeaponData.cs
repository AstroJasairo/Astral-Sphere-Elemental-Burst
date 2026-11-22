using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Dữ liệu cho từng cấp của vũ khí (Lv1 đến Lv5)
// Quy ước các chỉ số "theo cấp": 0 = dùng giá trị chung trong WeaponData (hoặc trong prefab với 3 trường đánh dấu)
[System.Serializable]
public class WeaponLevel
{
    [Header("General")]
    public float baseDamage = 0f;            // Sát thương cơ bản ở cấp này. 0 = dùng WeaponData.baseDamage
    public float cooldown = 0f;              // Hồi chiêu ở cấp này (giây). 0 = dùng WeaponData.cooldown
    [Tooltip("Tầm tìm mục tiêu. 0 = dùng WeaponData.targetRange")]
    public float targetRange = 0f;

    [Header("Scope")]
    public float explosionRadius = 0f;       // Bóng tối, Nước: bán kính nổ
    public float innerRadius = 0f;           // Nhẫn, Nón nhẫn, Sweep nhẫn: bán kính lỗ giữa (0 = không có lỗ)
    public float radius = 0f;                // Lửa, Đất, Phong, Clarius: bán kính
    public float coneAngle = 0f;             // Phong: góc mở hình nón (độ)

    [Header("Specials")]
    public int projectileCount = 0;          // Kim loại, Nước: số viên. Linh hồn: số quả cầu
    public int chainTargets = 0;             // Lôi: số kẻ địch mỗi lần giật
    public float knockbackDistance = 0f;     // Phong: quãng đường đẩy lùi
    public float beamWidth = 0f;             // Ánh sáng, Plant: độ rộng tia
    public float rootDuration = 0f;          // Plant: thời gian trói (giây)

    [Header("Projectile (Kim loại, Nước, Bóng tối)")]
    [Tooltip("Bật: dùng 'Pierce' của cấp này. Tắt: dùng WeaponData.pierce (vì 0 xuyên là giá trị có nghĩa)")]
    public bool overridePierce = false;
    [Tooltip("Số kẻ địch xuyên qua thêm (chỉ có tác dụng khi bật Override Pierce)")]
    public int pierce = 0;
    [Tooltip("0 = dùng WeaponData.projectileSpeed")]
    public float projectileSpeed = 0f;
    [Tooltip("0 = dùng WeaponData.projectileLifetime")]
    public float projectileLifetime = 0f;
    [Tooltip("Khoảng cách giữa hai viên trong loạt. 0 = dùng WeaponData.burstInterval. CẨN THẬN: giá trị > 0 làm cấp này bắn LOẠT")]
    public float burstInterval = 0f;
    [Tooltip("Chế độ bắn tỏa: góc giữa các viên. 0 = dùng giá trị trong prefab")]
    public float spreadAngle = 0f;

    [Header("Chain (Lôi)")]
    [Tooltip("0 = dùng WeaponData.chainRange")]
    public float chainRange = 0f;

    [Header("Orbit (Linh hồn)")]
    [Tooltip("Độ mỗi giây. 0 = dùng WeaponData.orbitSpeed")]
    public float orbitSpeed = 0f;
    [Tooltip("0 = dùng WeaponData.orbitRadius")]
    public float orbitRadius = 0f;
    [Tooltip("Bán kính va chạm của mỗi quả cầu. 0 = dùng giá trị trong prefab")]
    public float orbHitRadius = 0f;
    [Tooltip("0 = dùng WeaponData.hitInterval")]
    public float hitInterval = 0f;

    [Header("Beam (Ánh sáng, Plant)")]
    [Tooltip("0 = dùng WeaponData.beamLength")]
    public float beamLength = 0f;

    [Header("Telegraph (Phong, Plant, các dạng Area)")]
    [Tooltip("Thời gian vùng cam chạy hết. 0 = dùng giá trị trong prefab")]
    public float telegraphDuration = 0f;

    [Header("Unique")]
    public float existDuration = 0f;         // Clarius: thời gian tồn tại của thanh kiếm (giây)
    [Tooltip("Clarius: khoảng cách giữa hai lần gây sát thương. 0 = dùng WeaponData.tickInterval")]
    public float tickInterval = 0f;

    [Header("Effects")]
    public List<WeaponHitEffect> hitEffects = new List<WeaponHitEffect>();      // Hiệu ứng gây lên quái ở cấp này.
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

    [Header("Passive đính kèm (cấp passive = cấp vũ khí; nhân vật mang vũ khí được hưởng)")]
    public List<PassiveSkillData> passiveSkills = new List<PassiveSkillData>();

    [Header("Hỗ trợ cân bằng")]
    [Tooltip("Dùng cho menu chuột phải 'Fill Damage Per Level': mỗi cấp tăng bao nhiêu so với Base Damage (0.2 = +20%)")]
    public float damageGrowthPerLevel = 0.2f;

    // Dòng mô tả passive ở một cấp, nối vào thẻ chọn nâng cấp
    public string DescribePassives(int level)
    {
        if (passiveSkills == null || passiveSkills.Count == 0) return "";

        var sb = new StringBuilder();
        foreach (PassiveSkillData p in passiveSkills)
        {
            if (p == null) continue;
            sb.Append("\n+ ").Append(p.skillName).Append(": ").Append(p.DescribeLevel(level));
        }
        return sb.ToString();
    }

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

    // Chuột phải vào tiêu đề asset: chép cooldown chung vào cooldown của cả 5 cấp, rồi bạn chỉnh từng cấp
    [ContextMenu("Fill Cooldown Per Level")]
    private void FillCooldownPerLevel()
    {
        for (int i = 0; i < levels.Length; i++) levels[i].cooldown = cooldown;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Chuột phải vào tiêu đề asset: chép các chỉ số chung vào cả 5 cấp, rồi bạn chỉnh từng cấp
    [ContextMenu("Fill Level Stats From Weapon")]
    private void FillLevelStats()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            WeaponLevel l = levels[i];
            l.targetRange = targetRange;
            l.overridePierce = true;
            l.pierce = pierce;
            l.projectileSpeed = projectileSpeed;
            l.projectileLifetime = projectileLifetime;
            l.burstInterval = burstInterval;
            l.chainRange = chainRange;
            l.orbitSpeed = orbitSpeed;
            l.orbitRadius = orbitRadius;
            l.hitInterval = hitInterval;
            l.beamLength = beamLength;
            l.tickInterval = tickInterval;
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Xóa mọi giá trị riêng của cấp: tất cả cấp dùng lại giá trị chung
    [ContextMenu("Clear Level Stat Overrides")]
    private void ClearLevelStats()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            WeaponLevel l = levels[i];
            l.targetRange = 0f; l.overridePierce = false; l.pierce = 0;
            l.projectileSpeed = 0f; l.projectileLifetime = 0f; l.burstInterval = 0f; l.spreadAngle = 0f;
            l.chainRange = 0f; l.orbitSpeed = 0f; l.orbitRadius = 0f; l.orbHitRadius = 0f; l.hitInterval = 0f;
            l.beamLength = 0f; l.telegraphDuration = 0f; l.tickInterval = 0f;
        }
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

    [Header("Âm thanh")]
    public AudioCue fireCue = new AudioCue();         // Tiếng khi vũ khí tấn công

    [Header("Thông số theo cấp: phần tử 0 = Lv1 ... phần tử 4 = Lv5")]
    public WeaponLevel[] levels = new WeaponLevel[5];
}