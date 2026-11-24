using UnityEngine;

// Thiên thần hộ mệnh: khi nhận sát thương khiến bản thân bị kết liễu, sống sót với 1 máu và hồi máu đều trong một thời gian.
// Không cứu khỏi hiệu ứng gây chết (instant kill). Có hồi chiêu. Dùng được cho nhân vật và BOSS.
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Guardian Angel")]
public class GuardianAnglePassiveData : PassiveSkillData
{
    [Tooltip("Fixed: hồi một lượng máu cố định. Percent: hồi theo phần trăm máu tối đa (0.2 = 20%)")]
    public HealAmountMode healMode = HealAmountMode.Percent;

    [Header("Theo cấp skill 1..5")]
    public float[] healAmount = new float[MaxLevel];            // Tổng máu hồi (chia đều cho thời gian)
    public float[] healDuration = new float[MaxLevel];          // Hồi trong bao nhiêu giây
    [UnityEngine.Serialization.FormerlySerializedAs("invulnerableDuration")]
    public float[] supremeDuration = new float[MaxLevel];
    public float[] cooldown = new float[MaxLevel];

    [Tooltip("OnActivate: hồi chiêu tính từ lúc kích hoạt. AfterEffectEnds: tính từ lúc hồi máu xong")]
    public CooldownStart cooldownStart = CooldownStart.OnActivate;

    public float GetAmount(int level) => Pick(healAmount, level);
    public float GetDuration(int level) => Pick(healDuration, level);
    public float GetSupreme(int level) => Pick(supremeDuration, level);
    public float GetCooldown(int level) => Pick(cooldown, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new GuardianAnglePassiveEffect(stats, this);
    public override PassiveSkillEffect CreateEffect(ISkillOwner owner) => new GuardianAnglePassiveEffect(owner, this);

    public override string DescribeLevel(int level)
    {
        string amount = healMode == HealAmountMode.Percent
            ? $"{GetAmount(level) * 100f:0.#}% máu tối đa"
            : $"{GetAmount(level):0.#} máu";

        string supreme = GetSupreme(level) > 0f ? $" Tối thượng {GetSupreme(level):0.#}s đầu (miễn nhiễm, đứng yên, ngừng tấn công)." : "";
        return $"Khi nhận sát thương chí mạng, sống sót với 1 máu và hồi {amount} trong {GetDuration(level):0.#}s.{supreme} " +
               $"Hồi chiêu {GetCooldown(level):0.#}s.";
    }

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_guardian_angel";
        skillName = "Thiên thần hộ mệnh";
        healMode = HealAmountMode.Percent;
        healAmount = new[] { 0.10f, 0.15f, 0.20f, 0.25f, 0.30f };
        healDuration = new[] { 5f, 5f, 5f, 5f, 5f };
        supremeDuration = new[] { 1f, 1.5f, 2f, 2.5f, 3f };
        cooldown = new[] { 180f, 165f, 150f, 135f, 120f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class GuardianAnglePassiveEffect : PassiveSkillEffect
{
    private const float HealTick = 0.2f;                   // Hồi theo cụm 0.2s, không mỗi frame (đỡ phát sự kiện máu)

    private readonly GuardianAnglePassiveData data;
    private float amount, healDuration, supremeDuration, cooldownTotal;
    private bool running;

    private bool healing, supreme;
    private float healLeft, healPerSecond, healEnd, healTimer, supremeEnd;
    private float cooldownStartTime, cooldownEnd;

    public GuardianAnglePassiveEffect(ISkillOwner owner, GuardianAnglePassiveData data) : base(owner) { this.data = data; }

    // ---- Dữ liệu cho HUD ----
    public override bool IsEffectShown => healing || supreme;
    public override float EffectRemaining01
    {
        get
        {
            if (!healing || healDuration <= 0f) return supreme ? 1f : 0f;
            return Mathf.Clamp01((healEnd - Time.time) / healDuration);
        }
    }

    public override float CooldownRemaining01
    {
        get
        {
            float now = Time.time;
            if (cooldownTotal <= 0f || now < cooldownStartTime || now >= cooldownEnd) return 0f;
            return Mathf.Clamp01((cooldownEnd - now) / cooldownTotal);
        }
    }

    public override void SetLevel(int level)
    {
        amount = data.GetAmount(level);
        healDuration = data.GetDuration(level);
        supremeDuration = data.GetSupreme(level);
        cooldownTotal = data.GetCooldown(level);
    }

    public override void Enable()
    {
        owner.Health.LethalGuards -= HandleLethal;         // Gỡ trước rồi đăng ký: gọi nhiều lần không trùng
        owner.Health.LethalGuards += HandleLethal;
        running = true;
    }

    // Câm lặng: gỡ người bảo vệ, dừng hồi và Tối thượng. Hồi chiêu vẫn chạy tiếp (theo Time.time)
    public override void Disable()
    {
        owner.Health.LethalGuards -= HandleLethal;
        running = false;
        healing = false;
        EndSupreme();
    }

    // HealthSystem gọi khi một đòn sắp giết chủ sở hữu. Trả về true nếu đã cứu (và đã Revive)
    private bool HandleLethal()
    {
        if (!running) return false;

        float now = Time.time;
        if (now < cooldownEnd) return false;               // Còn hồi chiêu

        HealthSystem health = owner.Health;
        health.Revive(1f);                                 // Sống sót với 1 máu

        float total = data.healMode == HealAmountMode.Percent ? health.MaxHealth * amount : amount;
        if (total > 0f)
        {
            if (healDuration > 0f)
            {
                healing = true;
                healLeft = total;
                healPerSecond = total / healDuration;
                healEnd = now + healDuration;
                healTimer = 0f;
            }
            else
            {
                owner.ReceiveHealing(total);               // Không có thời gian: hồi ngay
            }
        }

        if (supremeDuration > 0f)
        {
            supreme = true;
            supremeEnd = now + supremeDuration;
            owner.SetImmunity(this, ImmunityFlags.Supreme);    // Bất hoại + miễn nhiễm gây chết. Sát thương Mechanic vẫn nhận
            health.SetDormant(this, true);                     // Đứng yên, ngừng tấn công, vũ khí tắt. Skill vẫn chạy
        }

        float effectEnd = Mathf.Max(healing ? healEnd : now, supreme ? supremeEnd : now);
        cooldownStartTime = data.cooldownStart == CooldownStart.OnActivate ? now : effectEnd;
        cooldownEnd = cooldownStartTime + cooldownTotal;
        return true;
    }

    public override void Tick(float deltaTime)
    {
        float now = Time.time;

        if (supreme && now >= supremeEnd) EndSupreme();

        if (!healing) return;

        healTimer += deltaTime;
        bool finished = now >= healEnd;
        if (healTimer < HealTick && !finished) return;

        // Hết giờ thì hồi nốt phần còn lại, không để sót do làm tròn
        float chunk = finished ? healLeft : Mathf.Min(healLeft, healPerSecond * healTimer);
        healTimer = 0f;

        if (chunk > 0f)
        {
            owner.ReceiveHealing(chunk);
            healLeft -= chunk;
        }

        if (finished || healLeft <= 0.001f) healing = false;
    }

    private void EndSupreme()
    {
        if (!supreme) return;
        supreme = false;
        owner.SetImmunity(this, ImmunityFlags.None);
        owner.Health.SetDormant(this, false);
    }
}