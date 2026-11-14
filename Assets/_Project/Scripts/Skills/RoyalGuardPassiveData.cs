using UnityEngine;

// Hộ giá: mỗi cấp của mỗi vũ khí đang mang giảm sát thương nhận vào.
// Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive > Royal Guard
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Royal Guard")]
public class RoyalGuardPassiveData : PassiveSkillData
{
    [Header("Giảm sát thương cho MỖI cấp vũ khí, theo cấp skill 1..5 (0.005 = 0.5%)")]
    public float[] reductionPerWeaponLevel = new float[MaxLevel];

    public float GetPerLevel(int level) => Pick(reductionPerWeaponLevel, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new RoyalGuardPassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Mỗi cấp của mỗi vũ khí đang mang giảm {GetPerLevel(level) * 100f:0.##}% sát thương nhận vào.";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_royal_guard";          // Đổi tiền tố theo nơi dùng (nhân vật, hoặc tên vũ khí khi gắn lên vũ khí)
        skillName = "Hộ giá";
        reductionPerWeaponLevel = new[] { 0.005f, 0.01f, 0.015f, 0.02f, 0.025f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class RoyalGuardPassiveEffect : WeaponLevelScaledPassiveEffect
{
    private readonly RoyalGuardPassiveData data;

    public RoyalGuardPassiveEffect(PlayerStats stats, RoyalGuardPassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { perLevel = data.GetPerLevel(level); }

    // Nguồn giảm MỌI sát thương (trừ Pure), cộng với giảm theo nguyên tố, chặn theo CombatRules
    protected override void ApplyValue(float value) { stats.Reductions.Set(this, DamageReductionSet.AllScope, value); }
    protected override void RemoveValue() { stats.Reductions.Remove(this); }
}