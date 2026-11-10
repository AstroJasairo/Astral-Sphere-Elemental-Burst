using UnityEngine;

// Lớp cha cho dữ liệu skill. Cấp 0 = chưa mở khóa, tối đa MaxLevel.
public abstract class SkillDataBase : ScriptableObject
{
    public const int MaxLevel = 5;

    [Tooltip("Khóa lưu tiến trình, DUY NHẤT trong một nhân vật, KHÔNG đổi sau khi phát hành. Quy ước: <nhân vật>_<kỹ năng>, vd claire_fire_resistance")]
    public string id;
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;                       // Mô tả chung (tab dùng)
    public int[] upgradeCost = new int[MaxLevel];               // [i] = vàng để lên từ cấp i lên cấp i+1 (SỐ TẠM)

    // Mô tả hiệu ứng cụ thể ở một cấp (1..5), lớp con tự ghép chuỗi từ số liệu
    public abstract string DescribeLevel(int level);

    public int GetUpgradeCost(int currentLevel)
        => (upgradeCost != null && currentLevel >= 0 && currentLevel < upgradeCost.Length) ? upgradeCost[currentLevel] : -1;

    // Lấy giá trị theo cấp (cấp 1 = phần tử 0), kẹp trong mảng
    protected static float Pick(float[] values, int level)
    {
        if (values == null || values.Length == 0) return 0f;
        return values[Mathf.Clamp(level, 1, values.Length) - 1];
    }
}

// Skill bị động: chạy liên tục, không bấm
public abstract class PassiveSkillData : SkillDataBase
{
    public abstract PassiveSkillEffect CreateEffect(PlayerStats stats);
}

// Skill chủ động: có nút kích hoạt, hiệu lực và hồi chiêu theo cấp
public abstract class ActiveSkillData : SkillDataBase
{
    [Tooltip("OnActivate: hồi chiêu tính ngay khi bấm. AfterEffectEnds: hết hiệu lực mới bắt đầu hồi")]
    public CooldownStart cooldownStart = CooldownStart.AfterEffectEnds;
    public float[] duration = new float[MaxLevel];              // Giây hiệu lực theo cấp
    public float[] cooldown = new float[MaxLevel];              // Giây hồi chiêu theo cấp

    public float GetDuration(int level) => Pick(duration, level);
    public float GetCooldown(int level) => Pick(cooldown, level);

    public abstract ActiveSkillEffect CreateEffect(PlayerStats stats);
}