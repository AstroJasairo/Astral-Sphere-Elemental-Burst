using UnityEngine;

// Passive "Dòng chảy sinh lực" (Claire): tăng hiệu quả hồi phục.
// Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive > Healing Effectiveness
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Healing Effectiveness")]
public class HealingEffectivenessPassiveData : PassiveSkillData
{
    [Header("Hiệu quả hồi phục theo cấp 1..5 (0.03 = +3% lượng hồi)")]
    public float[] bonus = new float[MaxLevel];

    public float GetBonus(int level) => Pick(bonus, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new HealingEffectivenessPassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Tăng {GetBonus(level) * 100f:0.#}% hiệu quả hồi phục (mọi nguồn hồi máu).";

    [ContextMenu("Fill Claire Life Flow Defaults")]
    private void FillDefaults()
    {
        id = "claire_life_flow";
        skillName = "Dòng chảy sinh lực";
        bonus = new[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.15f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class HealingEffectivenessPassiveEffect : PassiveSkillEffect
{
    private readonly HealingEffectivenessPassiveData data;
    private float value;

    public HealingEffectivenessPassiveEffect(PlayerStats stats, HealingEffectivenessPassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { value = data.GetBonus(level); }

    // Mọi nguồn hồi máu đi qua PlayerStats.ReceiveHealing nên tự nhận hệ số này (nhân tích lũy với các nguồn khác)
    public override void Enable() { stats.HealingBonuses.Set(this, value); }
    public override void Disable() { stats.HealingBonuses.Remove(this); }
}