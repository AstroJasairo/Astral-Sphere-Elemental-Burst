using UnityEngine;

// Lưỡi gươm của nữ hoàng: giống Hộ giá nhưng TĂNG SÁT THƯƠNG GÂY RA (cả sát thương Pure).
// Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive > Queens Blade
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Queens Blade")]
public class QueensBladePassiveData : PassiveSkillData
{
    [Header("Tăng sát thương cho MỖI cấp vũ khí, theo cấp skill 1..5 (0.005 = 0.5%)")]
    public float[] bonusPerWeaponLevel = new float[MaxLevel];

    public float GetPerLevel(int level) => Pick(bonusPerWeaponLevel, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new QueensBladePassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Mỗi cấp của mỗi vũ khí đang mang tăng {GetPerLevel(level) * 100f:0.##}% sát thương gây ra.";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_queens_blade";
        skillName = "Lưỡi gươm của nữ hoàng";
        bonusPerWeaponLevel = new[] { 0.005f, 0.01f, 0.015f, 0.02f, 0.025f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class QueensBladePassiveEffect : WeaponLevelScaledPassiveEffect
{
    private readonly QueensBladePassiveData data;

    public QueensBladePassiveEffect(PlayerStats stats, QueensBladePassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { perLevel = data.GetPerLevel(level); }

    // Nhóm "tất cả": áp dụng cho mọi nguyên tố kể cả Pure. Cộng với bonus nguyên tố (khác nhóm)
    protected override void ApplyValue(float value) { stats.DamageBonuses.Set(this, DamageBonusSet.AllScope, value); }
    protected override void RemoveValue() { stats.DamageBonuses.Remove(this); }
}