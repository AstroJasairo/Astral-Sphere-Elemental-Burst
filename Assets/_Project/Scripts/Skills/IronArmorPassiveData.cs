using UnityEngine;

// Giáp sắt: khi nhận sát thương khiến máu còn dưới một ngưỡng %, nhận giảm sát thương một thời gian.
// Có hiệu lực (icon xanh dương) và hồi chiêu (icon tím).
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Iron Armor")]
public class IronArmorPassiveData : PassiveSkillData
{
    [Header("Theo cấp skill 1..5")]
    public float[] hpThreshold = new float[MaxLevel];          // 0.25 = kích hoạt khi máu còn dưới 25%
    public float[] damageReduction = new float[MaxLevel];      // 0.1 = giảm 10% sát thương
    public float[] duration = new float[MaxLevel];             // Giây hiệu lực
    public float[] cooldown = new float[MaxLevel];             // Giây hồi chiêu

    [Tooltip("OnActivate: hồi chiêu tính từ lúc kích hoạt. AfterEffectEnds: tính từ lúc hết hiệu lực")]
    public CooldownStart cooldownStart = CooldownStart.OnActivate;

    public float GetThreshold(int level) => Pick(hpThreshold, level);
    public float GetReduction(int level) => Pick(damageReduction, level);
    public float GetDuration(int level) => Pick(duration, level);
    public float GetCooldown(int level) => Pick(cooldown, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new IronArmorPassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Khi nhận sát thương khiến máu còn dưới {GetThreshold(level) * 100f:0.#}%, giảm {GetReduction(level) * 100f:0.#}% " +
           $"sát thương nhận vào trong {GetDuration(level):0.#} giây. Hồi chiêu {GetCooldown(level):0.#} giây.";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_iron_armor";
        skillName = "Giáp sắt";
        hpThreshold = new[] { 0.25f, 0.30f, 0.35f, 0.40f, 0.45f };
        damageReduction = new[] { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f };
        duration = new[] { 6f, 7f, 8f, 9f, 10f };
        cooldown = new[] { 60f, 55f, 50f, 45f, 40f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class IronArmorPassiveEffect : PassiveSkillEffect
{
    private readonly IronArmorPassiveData data;
    private float threshold, reduction, effectTotal, cooldownTotal;
    private bool subscribed;
    private bool applied;                                    // Đang có hiệu lực giảm sát thương
    private float effectEnd, cooldownStart, cooldownEnd;     // Ba mốc thời gian, giống hồi chiêu của skill chủ động

    public IronArmorPassiveEffect(PlayerStats stats, IronArmorPassiveData data) : base(stats) { this.data = data; }

    // ---- Dữ liệu cho HUD ----
    public override bool IsEffectShown => applied;
    public override float EffectRemaining01
        => (applied && effectTotal > 0f) ? Mathf.Clamp01((effectEnd - Time.time) / effectTotal) : 0f;

    public override float CooldownRemaining01
    {
        get
        {
            float now = Time.time;
            // Kiểu AfterEffectEnds: hồi chiêu chưa bắt đầu khi còn đang hiệu lực
            if (cooldownTotal <= 0f || now < cooldownStart || now >= cooldownEnd) return 0f;
            return Mathf.Clamp01((cooldownEnd - now) / cooldownTotal);
        }
    }

    public override void SetLevel(int level)
    {
        threshold = data.GetThreshold(level);
        reduction = data.GetReduction(level);
        effectTotal = data.GetDuration(level);
        cooldownTotal = data.GetCooldown(level);
    }

    // Gọi nhiều lần vẫn an toàn: gỡ đăng ký trước rồi mới đăng ký lại
    public override void Enable()
    {
        stats.OnDamaged -= HandleDamaged;
        stats.OnDamaged += HandleDamaged;
        subscribed = true;
    }

    // Câm lặng: gỡ SẠCH hiệu ứng đang chạy. Hồi chiêu vẫn tiếp tục (dựa vào Time.time)
    public override void Disable()
    {
        if (subscribed) stats.OnDamaged -= HandleDamaged;
        subscribed = false;
        EndEffect();
    }

    public override void Tick(float deltaTime)
    {
        if (applied && Time.time >= effectEnd) EndEffect();
    }

    // Phát SAU khi đòn đã trừ máu, nên đòn kích hoạt không được hưởng giảm sát thương
    private void HandleDamaged(float amount)
    {
        float now = Time.time;
        if (now < cooldownEnd) return;                              // Còn hồi chiêu

        HealthSystem health = stats.Health;
        if (health.IsDead || health.MaxHealth <= 0f) return;
        if (health.CurrentHealth / health.MaxHealth >= threshold) return;   // Máu chưa xuống dưới ngưỡng

        applied = true;
        effectEnd = now + effectTotal;
        stats.Reductions.Set(this, DamageReductionSet.AllScope, reduction);   // Giảm chung (trừ Pure)

        cooldownStart = data.cooldownStart == CooldownStart.OnActivate ? now : effectEnd;
        cooldownEnd = cooldownStart + cooldownTotal;
    }

    private void EndEffect()
    {
        applied = false;
        stats.Reductions.Remove(this);
    }
}