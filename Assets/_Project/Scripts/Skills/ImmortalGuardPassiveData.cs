using System;
using System.Text;
using UnityEngine;

[Serializable]
public class ImmortalStage
{
    [Range(0f, 1f)] public float hpThreshold = 0.5f;       // 0.5 = khi máu chạm 50%
    public float duration = 3f;                            // Giây miễn nhiễm
}

[Serializable]
public class ImmortalLevel
{
    public ImmortalStage[] stages = new ImmortalStage[0];
    public float cooldown = 60f;                           // Chỉ dùng khi chế độ Cooldown
}

// Bất tử hộ thân: khi máu chạm các mốc, nhận miễn nhiễm trong một thời gian.
// Dùng được cho nhân vật VÀ BOSS (chủ yếu để BOSS không bị giết quá nhanh trước khi thực hiện các cơ chế).
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Immortal Guard")]
public class ImmortalGuardPassiveData : PassiveSkillData
{
    [Tooltip("Loại miễn nhiễm nhận được: Damage (mặc định), Dot, CrowdControl, Indestructible (Bất hoại), Supreme (Tối thượng)")]
    public ImmunityFlags immunity = ImmunityFlags.Damage;

    [Tooltip("OneTime: mỗi mốc chỉ kích hoạt một lần mỗi trận. Cooldown: mốc nạp lại khi máu lên trên mốc, và có hồi chiêu")]
    public SkillTriggerMode mode = SkillTriggerMode.OneTime;

    [Tooltip("Bật: miễn nhiễm KHÔNG BAO GIỜ hết (bỏ qua thời gian của từng mốc)")]
    public bool unlimitedDuration = false;

    [Header("Theo cấp skill 1..5 (BOSS thường chỉ dùng cấp 1)")]
    public ImmortalLevel[] levels = new ImmortalLevel[MaxLevel];

    public ImmortalLevel GetLevel(int level)
    {
        if (levels == null || levels.Length == 0) return null;
        return levels[Mathf.Clamp(level, 1, levels.Length) - 1];
    }

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new ImmortalGuardPassiveEffect(stats, this);
    public override PassiveSkillEffect CreateEffect(ISkillOwner owner) => new ImmortalGuardPassiveEffect(owner, this);

    public override string DescribeLevel(int level)
    {
        ImmortalLevel l = GetLevel(level);
        if (l == null || l.stages == null || l.stages.Length == 0) return "Chưa cấu hình.";

        var sb = new StringBuilder("Khi máu chạm ");
        for (int i = 0; i < l.stages.Length; i++)
        {
            if (l.stages[i] == null) continue;
            if (i > 0) sb.Append(", ");
            sb.Append($"{l.stages[i].hpThreshold * 100f:0.#}%");
            if (!unlimitedDuration) sb.Append($" ({l.stages[i].duration:0.#}s)");
        }
        sb.Append($", nhận miễn nhiễm ({immunity})");
        sb.Append(unlimitedDuration ? " không giới hạn thời gian." : ".");
        if (mode == SkillTriggerMode.Cooldown) sb.Append($" Hồi chiêu {l.cooldown:0.#}s.");
        return sb.ToString();
    }

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_immortal_guard";
        skillName = "Bất tử hộ thân";
        immunity = ImmunityFlags.Damage;
        mode = SkillTriggerMode.OneTime;

        levels = new ImmortalLevel[MaxLevel];
        for (int i = 0; i < MaxLevel; i++)
        {
            levels[i] = new ImmortalLevel
            {
                cooldown = 90f - i * 10f,
                stages = new[]
                {
                    new ImmortalStage { hpThreshold = 0.5f, duration = 2f + i },
                    new ImmortalStage { hpThreshold = 0.25f, duration = 2f + i }
                }
            };
        }
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class ImmortalGuardPassiveEffect : PassiveSkillEffect
{
    private readonly ImmortalGuardPassiveData data;
    private ImmortalLevel level;
    private bool[] fired = new bool[0];                 // Mốc nào đã kích hoạt
    private bool running, applied, wasCooling;
    private float effectEnd, effectTotal, cooldownEnd, cooldownTotal;

    public ImmortalGuardPassiveEffect(ISkillOwner owner, ImmortalGuardPassiveData data) : base(owner) { this.data = data; }

    // ---- Dữ liệu cho HUD ----
    public override bool IsEffectShown => applied;
    public override bool EffectUsesTimer => !data.unlimitedDuration;
    public override float EffectRemaining01
        => (applied && !data.unlimitedDuration && effectTotal > 0f) ? Mathf.Clamp01((effectEnd - Time.time) / effectTotal) : 0f;

    public override float CooldownRemaining01
    {
        get
        {
            if (data.mode != SkillTriggerMode.Cooldown || cooldownTotal <= 0f) return 0f;
            float now = Time.time;
            return now >= cooldownEnd ? 0f : Mathf.Clamp01((cooldownEnd - now) / cooldownTotal);
        }
    }

    public override void SetLevel(int skillLevel)
    {
        level = data.GetLevel(skillLevel);
        int n = (level != null && level.stages != null) ? level.stages.Length : 0;
        if (fired.Length != n) System.Array.Resize(ref fired, n);      // Giữ trạng thái đã kích hoạt khi chỉ đổi cấp
        cooldownTotal = level != null ? level.cooldown : 0f;
    }

    // Gọi nhiều lần vẫn an toàn
    public override void Enable()
    {
        owner.Health.OnHealthChanged -= HandleHealthChanged;
        owner.Health.OnHealthChanged += HandleHealthChanged;
        running = true;
    }

    // Câm lặng: gỡ SẠCH miễn nhiễm đang có
    public override void Disable()
    {
        owner.Health.OnHealthChanged -= HandleHealthChanged;
        running = false;
        EndImmunity();
    }

    public override void Tick(float deltaTime)
    {
        float now = Time.time;
        if (applied && !data.unlimitedDuration && now >= effectEnd) EndImmunity();

        // Hết hồi chiêu: kiểm tra lại máu (có thể đang dưới mốc từ lúc còn hồi chiêu)
        if (wasCooling && now >= cooldownEnd)
        {
            wasCooling = false;
            Evaluate();
        }
    }

    private void HandleHealthChanged(float current, float max) { Evaluate(); }

    private void Evaluate()
    {
        if (!running || level == null || level.stages == null) return;

        HealthSystem health = owner.Health;
        if (health.IsDead || health.MaxHealth <= 0f) return;

        float ratio = health.CurrentHealth / health.MaxHealth;
        float now = Time.time;
        bool cooling = data.mode == SkillTriggerMode.Cooldown && now < cooldownEnd;
        ImmortalStage best = null;

        for (int i = 0; i < level.stages.Length; i++)
        {
            ImmortalStage stage = level.stages[i];
            if (stage == null) continue;

            if (ratio > stage.hpThreshold)                       // Máu trên mốc
            {
                if (data.mode == SkillTriggerMode.Cooldown) fired[i] = false;    // Nạp lại mốc
                continue;
            }

            if (fired[i] || cooling) continue;

            fired[i] = true;                                     // Vượt nhiều mốc một lúc: lấy mốc có thời gian dài nhất
            if (best == null || stage.duration > best.duration) best = stage;
        }

        if (best != null) Apply(best.duration, now);
    }

    private void Apply(float duration, float now)
    {
        applied = true;
        effectTotal = duration;
        effectEnd = data.unlimitedDuration ? float.MaxValue : now + duration;
        owner.SetImmunity(this, data.immunity);

        if (data.mode == SkillTriggerMode.Cooldown)
        {
            cooldownEnd = now + cooldownTotal;
            wasCooling = true;
        }
    }

    private void EndImmunity()
    {
        if (!applied) return;
        applied = false;
        owner.SetImmunity(this, ImmunityFlags.None);
    }
}