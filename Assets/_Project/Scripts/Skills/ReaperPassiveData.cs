using System.Text;
using UnityEngine;

// Đoạt mệnh: khi tấn công một mục tiêu (không nhất thiết gây sát thương) mà mục tiêu còn dưới ngưỡng % máu tối đa,
// kết liễu ngay. Ngưỡng nhân với hiệu lực của "giao thức" theo bậc mục tiêu.
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Reaper")]
public class ReaperPassiveData : PassiveSkillData
{
    [Header("Ngưỡng máu theo cấp skill 1..5 (0.05 = 5%)")]
    public float[] hpThreshold = new float[MaxLevel];

    [Tooltip("Standard: Normal 100%, Elite 50%, MiniBoss 20%, Boss 10%. Weak: Normal 100%, Elite 50%, MiniBoss 25%, Boss 0%. (Bảng sửa trong CombatRules)")]
    public ProtocolKind protocol = ProtocolKind.Standard;

    public float GetThreshold(int level) => Pick(hpThreshold, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new ReaperPassiveEffect(stats, this);

    public override string DescribeLevel(int level)
    {
        float t = GetThreshold(level);
        var sb = new StringBuilder("Tấn công kẻ địch khiến máu còn dưới ngưỡng thì kết liễu: ");

        AppendTier(sb, "Normal", EnemyTier.Normal, t);
        AppendTier(sb, ", Elite", EnemyTier.Elite, t);
        AppendTier(sb, ", Mini Boss", EnemyTier.MiniBoss, t);
        AppendTier(sb, ", Boss", EnemyTier.Boss, t);
        sb.Append(". Standard và Cataclysm miễn nhiễm (nếu giao thức có hiệu lực 0).");
        return sb.ToString();
    }

    private void AppendTier(StringBuilder sb, string label, EnemyTier tier, float threshold)
    {
        float effect = CombatRules.GetProtocolEffectiveness(protocol, tier);
        if (effect <= 0f) return;
        sb.Append(label).Append(" dưới ").Append((threshold * effect * 100f).ToString("0.##")).Append('%');
    }

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_reaper";
        skillName = "Đoạt mệnh";
        hpThreshold = new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };
        protocol = ProtocolKind.Standard;
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class ReaperPassiveEffect : PassiveSkillEffect
{
    private readonly ReaperPassiveData data;
    private float threshold;

    public ReaperPassiveEffect(PlayerStats stats, ReaperPassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { threshold = data.GetThreshold(level); }

    // Gọi nhiều lần vẫn an toàn
    public override void Enable()
    {
        stats.OnEnemyHit -= HandleEnemyHit;
        stats.OnEnemyHit += HandleEnemyHit;
    }

    public override void Disable() { stats.OnEnemyHit -= HandleEnemyHit; }

    // CombatHit báo sau MỖI đòn trúng quái (kể cả đòn không gây sát thương). Quái đã chết trong đòn đó thì không báo.
    private void HandleEnemyHit(EnemyBase enemy)
    {
        float effect = CombatRules.GetProtocolEffectiveness(data.protocol, enemy.Tier);
        if (effect <= 0f) return;                                     // Bậc này miễn nhiễm theo giao thức

        HealthSystem health = enemy.Health;
        if (health == null || health.IsDead || health.MaxHealth <= 0f) return;

        if (health.CurrentHealth / health.MaxHealth < threshold * effect)
            health.Kill();                                            // Bị chặn nếu mục tiêu miễn nhiễm hiệu ứng gây chết
    }
}