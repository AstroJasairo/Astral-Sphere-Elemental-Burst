using UnityEngine;

// Brave: mỗi kẻ địch trong phạm vi tăng ATK và DEF (có giới hạn số kẻ địch được tính).
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Brave")]
public class BravePassiveData : PassiveSkillData
{
    [Header("Theo cấp skill 1..5")]
    public float[] radius = new float[MaxLevel];              // Phạm vi quanh nhân vật
    public float[] atkPerEnemy = new float[MaxLevel];         // 0.02 = +2% ATK mỗi kẻ địch
    public float[] defPerEnemy = new float[MaxLevel];         // 0.03 = +3% DEF gốc mỗi kẻ địch

    [Tooltip("Tối đa bao nhiêu kẻ địch được tính (tránh tăng vô hạn khi quái đông)")]
    public int maxEnemies = 10;

    public float GetRadius(int level) => Pick(radius, level);
    public float GetAtk(int level) => Pick(atkPerEnemy, level);
    public float GetDef(int level) => Pick(defPerEnemy, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new BravePassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Mỗi kẻ địch trong phạm vi {GetRadius(level):0.#} tăng {GetAtk(level) * 100f:0.#}% ATK và " +
           $"{GetDef(level) * 100f:0.#}% DEF (tối đa {maxEnemies} kẻ địch).";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_brave";
        skillName = "Brave";
        radius = new[] { 3f, 4f, 5f, 6f, 7f };
        atkPerEnemy = new[] { 0.02f, 0.04f, 0.06f, 0.08f, 0.10f };
        defPerEnemy = new[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.15f };
        maxEnemies = 10;
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class BravePassiveEffect : PassiveSkillEffect
{
    private const float ScanInterval = 0.1f;              // Quét 10 lần/giây là đủ
    private const float BuffLinger = 0.35f;               // Icon sống lâu hơn một nhịp quét để không chớp

    private readonly BravePassiveData data;
    private float radius, atkPer, defPer;
    private float timer;
    private int shown = -1;                               // Số stack đã báo cho PlayerStats (-1 = chưa báo)

    public BravePassiveEffect(PlayerStats stats, BravePassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level)
    {
        radius = data.GetRadius(level);
        atkPer = data.GetAtk(level);
        defPer = data.GetDef(level);
        shown = -1;                                       // Cấp đổi: ép cập nhật lại ở lần quét tới
        timer = 0f;
    }

    public override void Enable() { timer = 0f; shown = -1; }

    // Gỡ SẠCH: trả ATK/DEF về như chưa có Brave và xóa icon
    public override void Disable()
    {
        stats.SetBrave(0, 0f, 0f);
        stats.Buffs.Clear(BuffType.Brave);
        shown = 0;
    }

    public override void Tick(float deltaTime)
    {
        timer -= deltaTime;
        if (timer > 0f) return;
        timer = ScanInterval;

        // Đếm kẻ địch trong phạm vi. foreach trên HashSet không sinh rác
        Vector2 center = stats.transform.position;
        float sqr = radius * radius;
        int count = 0;

        foreach (EnemyBase enemy in EnemyBase.Active)
        {
            if (((Vector2)enemy.transform.position - center).sqrMagnitude > sqr) continue;
            if (++count >= data.maxEnemies) break;
        }

        if (count != shown)
        {
            shown = count;
            stats.SetBrave(count, count * atkPer, count * defPer);
        }

        // Icon buff: giá trị = số stack, không đồng hồ (BuffHudView xử lý riêng)
        // Phải Clear trước: Buffs.Apply giữ giá trị CAO HƠN khi buff còn hiệu lực nên số stack sẽ không giảm được
        stats.Buffs.Clear(BuffType.Brave);
        if (count > 0) stats.Buffs.Apply(BuffType.Brave, count, BuffLinger);
    }
}