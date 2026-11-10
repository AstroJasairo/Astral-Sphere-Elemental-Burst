using System.Text;
using UnityEngine;

// Huyết nộ (Claire). Tạo bằng: Assets > Create > Astral Sphere > Skills > Active - Blood Rage
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Active - Blood Rage")]
public class BloodRageSkillData : ActiveSkillData
{
    [Header("Theo cấp 1..5 (phần trăm: 0.05 = 5%)")]
    public float[] hpDrainPerSecond = new float[MaxLevel];      // % máu TỐI ĐA của người chơi mất mỗi giây
    public float[] atkBonus = new float[MaxLevel];              // % ATK cộng thêm
    public float[] healOnKill = new float[MaxLevel];            // % máu TỐI ĐA CỦA KẺ ĐỊCH BỊ HẠ được hồi

    public float GetDrain(int level) => Pick(hpDrainPerSecond, level);
    public float GetAtkBonus(int level) => Pick(atkBonus, level);
    public float GetHealOnKill(int level) => Pick(healOnKill, level);

    public override ActiveSkillEffect CreateEffect(PlayerStats stats) => new BloodRageEffect(stats, this);

    public override string DescribeLevel(int level)
    {
        return $"Mất {GetDrain(level) * 100f:0.#}% máu tối đa mỗi giây, tăng {GetAtkBonus(level) * 100f:0.#}% ATK. " +
               $"Mỗi kẻ địch bị hạ hồi {GetHealOnKill(level) * 100f:0.#}% máu tối đa của chúng.\n" +
               $"Hiệu lực {GetDuration(level):0.#}s, hồi chiêu {GetCooldown(level):0.#}s.";
    }

    // Chuột phải tiêu đề asset để điền số của Claire
    [ContextMenu("Fill Claire Defaults")]
    private void FillDefaults()
    {
        skillName = "Huyết nộ";
        cooldownStart = CooldownStart.AfterEffectEnds;
        hpDrainPerSecond = new[] { 0.05f, 0.05f, 0.04f, 0.04f, 0.03f };
        atkBonus = new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };
        healOnKill = new[] { 0.02f, 0.04f, 0.06f, 0.08f, 0.10f };
        duration = new[] { 11f, 12f, 13f, 14f, 15f };
        cooldown = new[] { 100f, 90f, 80f, 70f, 60f };
        upgradeCost = new[] { 1500, 4000, 10000, 25000, 60000 };     // TẠM, cân bằng sau
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

// Hành vi Huyết nộ: kênh ATK nằm trong BuffController (tự có icon trên thanh buff), mất máu và hồi máu xử lý ở đây
public class BloodRageEffect : ActiveSkillEffect
{
    private const float TickInterval = 0.2f;                    // Trừ máu theo cụm 0.2s, không mỗi frame (đỡ phát sự kiện máu)
    private readonly BloodRageSkillData data;
    private float drainPct, healPct, tickTimer;

    public BloodRageEffect(PlayerStats stats, BloodRageSkillData data) : base(stats) { this.data = data; }

    public override void Begin(int level, float duration)
    {
        drainPct = data.GetDrain(level);
        healPct = data.GetHealOnKill(level);
        tickTimer = 0f;
        stats.Buffs.Apply(BuffType.ActiveSkill, data.GetAtkBonus(level), duration);   // PlayerStats.Atk đọc kênh này
    }

    public override void Tick(float deltaTime)
    {
        tickTimer += deltaTime;
        if (tickTimer < TickInterval) return;
        float elapsed = tickTimer;
        tickTimer = 0f;

        HealthSystem health = stats.Health;
        float allowed = health.CurrentHealth - 1f;              // Luôn chừa 1 HP, skill không tự giết người chơi
        if (allowed <= 0f) return;
        float drain = stats.MaxHp * drainPct * elapsed;         // Đi thẳng vào máu, không qua DEF/khiên
        health.TakeDamage(Mathf.Min(drain, allowed));
    }

    public override void OnEnemyKilled(EnemyBase enemy)
    {
        stats.ReceiveHealing(enemy.MaxHealth * healPct);            // Theo máu tối đa của quái bị hạ
    }

    public override void End() { }                              // Buff ATK tự hết hạn theo thời gian, không cần gỡ
}