using UnityEngine;

// Nữ hoàng chinh phạt (active): dựng aura quanh nhân vật, kẻ địch bên trong bị làm yếu.
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Active/Queens Conquest")]
public class QueensConquestSkillData : ActiveSkillData
{
    [Header("Theo cấp 1..5 (phần trăm: 0.3 = 30%)")]
    public float[] radius = new float[MaxLevel];
    public float[] moveSlow = new float[MaxLevel];                // Giảm tốc độ di chuyển
    public float[] attackSpeedSlow = new float[MaxLevel];         // Giảm tốc độ tấn công
    public float[] damageDealtReduction = new float[MaxLevel];    // Giảm sát thương kẻ địch gây ra
    public float[] damageTakenIncrease = new float[MaxLevel];     // Tăng sát thương kẻ địch nhận vào (NHÂN RIÊNG)

    [Header("Hình aura (tùy chọn): prefab có SpriteRenderer hình tròn đường kính 1 đơn vị")]
    public GameObject auraPrefab;
    [Range(0f, 1f)] public float auraAlpha = 0.25f;

    public float GetRadius(int level) => Pick(radius, level);
    public float GetMoveSlow(int level) => Pick(moveSlow, level);
    public float GetAttackSlow(int level) => Pick(attackSpeedSlow, level);
    public float GetDealtReduction(int level) => Pick(damageDealtReduction, level);
    public float GetTakenIncrease(int level) => Pick(damageTakenIncrease, level);

    public override ActiveSkillEffect CreateEffect(PlayerStats stats) => new QueensConquestEffect(stats, this);

    public override string DescribeLevel(int level)
    {
        return $"Dựng aura bán kính {GetRadius(level):0.#} quanh bản thân. Kẻ địch bên trong: giảm {GetMoveSlow(level) * 100f:0.#}% tốc độ di chuyển, " +
               $"{GetAttackSlow(level) * 100f:0.#}% tốc độ tấn công, {GetDealtReduction(level) * 100f:0.#}% sát thương gây ra, " +
               $"tăng {GetTakenIncrease(level) * 100f:0.#}% sát thương nhận vào.\n" +
               $"Hiệu lực {GetDuration(level):0.#}s, hồi chiêu {GetCooldown(level):0.#}s.";
    }

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_queens_conquest";
        skillName = "Nữ hoàng chinh phạt";
        cooldownStart = CooldownStart.AfterEffectEnds;
        radius = new[] { 3f, 4f, 5f, 6f, 7f };
        moveSlow = new[] { 0.30f, 0.35f, 0.40f, 0.45f, 0.50f };
        attackSpeedSlow = new[] { 0.50f, 0.55f, 0.60f, 0.65f, 0.70f };
        damageDealtReduction = new[] { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f };
        damageTakenIncrease = new[] { 0.20f, 0.25f, 0.30f, 0.35f, 0.40f };
        duration = new[] { 15f, 16f, 17f, 18f, 19f };
        cooldown = new[] { 200f, 190f, 180f, 170f, 160f };
        upgradeCost = new[] { 1500, 4000, 10000, 25000, 60000 };         // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class QueensConquestEffect : ActiveSkillEffect
{
    private const float ScanInterval = 0.15f;
    private const float Linger = 0.35f;                   // Debuff sống lâu hơn một nhịp quét: ra khỏi aura là tự hết sau ~0.35s

    private readonly QueensConquestSkillData data;
    private float radius, moveSlow, attackSlow, dealtReduction, takenIncrease;
    private float scanTimer;
    private GameObject aura;

    public QueensConquestEffect(PlayerStats stats, QueensConquestSkillData data) : base(stats) { this.data = data; }

    public override void Begin(int level, float duration)
    {
        radius = data.GetRadius(level);
        moveSlow = data.GetMoveSlow(level);
        attackSlow = data.GetAttackSlow(level);
        dealtReduction = data.GetDealtReduction(level);
        takenIncrease = data.GetTakenIncrease(level);
        scanTimer = 0f;                                   // Quét ngay ở Tick đầu tiên

        SpawnAura();
    }

    public override void Tick(float deltaTime)
    {
        if (aura != null) aura.transform.position = stats.transform.position;     // Aura đi theo nhân vật

        scanTimer -= deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = ScanInterval;

        Vector2 center = stats.transform.position;
        float sqr = radius * radius;

        foreach (EnemyBase enemy in EnemyBase.Active)     // HashSet: không sinh rác
        {
            if (((Vector2)enemy.transform.position - center).sqrMagnitude > sqr) continue;
            enemy.Debuffs.ApplyAura(moveSlow, attackSlow, dealtReduction, takenIncrease, Linger);
        }
    }

    // Hết hiệu lực hoặc bị Câm lặng: gỡ hình aura. Debuff trên quái tự hết sau Linger giây
    public override void End()
    {
        if (aura != null && PoolManager.Instance != null) PoolManager.Instance.Release(aura);
        aura = null;
    }

    private void SpawnAura()
    {
        if (data.auraPrefab == null) return;

        aura = PoolManager.Instance.Get(data.auraPrefab, stats.transform.position, Quaternion.identity);
        aura.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);   // Sprite tròn đường kính 1

        if (aura.TryGetComponent(out SpriteRenderer sr))
        {
            Color c = stats.PrimaryColor;                 // Màu chính của nhân vật
            c.a = data.auraAlpha;
            sr.color = c;
        }
    }
}