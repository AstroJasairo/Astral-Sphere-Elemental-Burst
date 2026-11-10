using UnityEngine;

// Passive "Kháng nguyên tố": giảm sát thương của MỘT nguyên tố nhận vào. Dùng cho Kháng hỏa của Claire và các skill tương tự sau này.
// Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive - Element Resistance
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive - Element Resistance")]
public class ElementResistancePassiveData : PassiveSkillData
{
    public ElementType element = ElementType.Fire;

    [Header("Giảm sát thương của nguyên tố trên theo cấp 1..5 (0.05 = 5%)")]
    public float[] reduction = new float[MaxLevel];

    public float GetReduction(int level) => Pick(reduction, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new ElementResistancePassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Giảm {GetReduction(level) * 100f:0.#}% sát thương {ElementDisplay.Name(element)} nhận vào.";

    [ContextMenu("Fill Claire Fire Resistance Defaults")]
    private void FillDefaults()
    {
        id = "claire_fire_resistance";
        skillName = "Kháng hỏa";
        element = ElementType.Fire;
        reduction = new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM, cân bằng sau
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class ElementResistancePassiveEffect : PassiveSkillEffect
{
    private readonly ElementResistancePassiveData data;
    private float value;

    public ElementResistancePassiveEffect(PlayerStats stats, ElementResistancePassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { value = data.GetReduction(level); }

    // Đăng ký nguồn giảm theo nguyên tố vào hệ thống giảm sát thương (11A). Khác nhóm "chung" nên CỘNG với nhau.
    public override void Enable() { stats.Reductions.Set(this, (int)data.element, value); }
    public override void Disable() { stats.Reductions.Remove(this); }
}