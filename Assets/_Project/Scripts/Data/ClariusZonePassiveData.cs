using UnityEngine;

// Sinh lực đỏ rực (Claire). Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive - Clarius Zone
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive - Clarius Zone")]
public class ClariusZonePassiveData : PassiveSkillData
{
    [Header("Giảm sát thương nhận vào theo cấp 1..5 (0.05 = 5%)")]
    public float[] damageReduction = new float[MaxLevel];

    public float GetReduction(int level) => Pick(damageReduction, level);
    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new ClariusZonePassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Khi đứng trong vùng ảnh hưởng của Clarius, giảm {GetReduction(level) * 100f:0.#}% sát thương gánh chịu (không cộng dồn).";

    [ContextMenu("Fill Claire Defaults")]
    private void FillDefaults()
    {
        skillName = "Sinh lực đỏ rực";
        damageReduction = new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class ClariusZonePassiveEffect : PassiveSkillEffect
{
    private const float CheckInterval = 0.1f;                   // Kiểm tra vị trí 10 lần/giây là đủ
    private readonly ClariusZonePassiveData data;
    private float reduction, timer;

    public ClariusZonePassiveEffect(PlayerStats stats, ClariusZonePassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { reduction = data.GetReduction(level); }

    public override void Tick(float deltaTime)
    {
        timer += deltaTime;
        if (timer < CheckInterval) return;
        timer = 0f;
        // Chỉ một hệ số (không nhân theo số kiếm) nên đứng chồng nhiều vùng vẫn không cộng dồn
        stats.IncomingDamageMultiplier = IsInsideAnyZone() ? 1f - reduction : 1f;
    }

    private bool IsInsideAnyZone()
    {
        Vector2 p = stats.transform.position;
        foreach (ClariusSword sword in ClariusSword.Instances)  // HashSet: foreach không sinh rác
        {
            if (!sword.IsAuraActive) continue;                  // Kiếm đang rơi hoặc đang mờ thì chưa tính
            float r = sword.AuraRadius;
            if (((Vector2)sword.transform.position - p).sqrMagnitude <= r * r) return true;
        }
        return false;
    }
}