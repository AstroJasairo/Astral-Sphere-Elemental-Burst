using UnityEngine;

// Dòng chảy năng lượng: tăng hiệu quả hồi mana.
// Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive > Energy Flow
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Energy Flow")]
public class EnergyFlowPassiveData : PassiveSkillData
{
    [Header("Hiệu quả hồi mana theo cấp 1..5 (0.05 = +5%)")]
    public float[] manaGainBonus = new float[MaxLevel];

    public float GetBonus(int level) => Pick(manaGainBonus, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new EnergyFlowPassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Tăng {GetBonus(level) * 100f:0.#}% hiệu quả hồi mana.";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_energy_flow";
        skillName = "Dòng chảy năng lượng";
        manaGainBonus = new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class EnergyFlowPassiveEffect : PassiveSkillEffect
{
    private readonly EnergyFlowPassiveData data;
    private float value;

    public EnergyFlowPassiveEffect(PlayerStats stats, EnergyFlowPassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { value = data.GetBonus(level); }

    // Cộng thẳng hay nhân tích lũy do CombatRules.manaGainStacking quyết định
    public override void Enable() { stats.ManaGainBonuses.Set(this, value); }
    public override void Disable() { stats.ManaGainBonuses.Remove(this); }
}