using System;
using UnityEngine;

// Giảm sát thương theo mốc máu: máu tụt qua mỗi mốc thì nhận một lớp giảm sát thương tạm thời (mỗi mốc đúng một lần).
// Dùng BossDamageReduction (nhiều lớp nhân tích lũy, có icon trên thanh buff BOSS).
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/HP Threshold Reduction")]
public class HpThresholdReductionPassiveData : PassiveSkillData
{
    [Serializable]
    public class ThresholdLevel
    {
        [Tooltip("Mốc máu còn lại (0.75 = 75%). Tự sắp xếp giảm dần")]
        public float[] thresholds = { 0.75f, 0.5f, 0.25f };
        [Range(0f, 1f)] public float reduction = 0.5f;        // Giảm 50% sát thương nhận vào
        public float duration = 20f;
    }

    [Header("Theo cấp (1 cấp mặc định)")]
    public ThresholdLevel[] levels = { new ThresholdLevel() };

    public ThresholdLevel GetLevel(int level)
    {
        if (levels == null || levels.Length == 0) return null;
        return levels[Mathf.Clamp(level, 1, levels.Length) - 1];
    }

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new HpThresholdReductionPassiveEffect(stats, this);
    public override PassiveSkillEffect CreateEffect(ISkillOwner owner) => new HpThresholdReductionPassiveEffect(owner, this);

    public override string DescribeLevel(int level)
    {
        ThresholdLevel l = GetLevel(level);
        return l == null ? "" : $"Khi máu chạm các mốc, giảm {l.reduction * 100f:0.#}% sát thương nhận vào trong {l.duration:0.#}s (mỗi mốc một lần).";
    }

    private void OnValidate()
    {
        if (levels == null) return;
        foreach (ThresholdLevel l in levels)
            if (l != null && l.thresholds != null) Array.Sort(l.thresholds, (a, b) => b.CompareTo(a));   // Giảm dần
    }

    [ContextMenu("Fill Lord Of The Wolf Defaults")]
    private void FillDefaults()
    {
        skillName = "Giảm sát thương theo mốc máu";
        levels = new[] { new ThresholdLevel { thresholds = new[] { 0.75f, 0.5f, 0.25f }, reduction = 0.5f, duration = 20f } };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class HpThresholdReductionPassiveEffect : PassiveSkillEffect
{
    private readonly HpThresholdReductionPassiveData data;
    private HpThresholdReductionPassiveData.ThresholdLevel level;
    private BossDamageReduction reduction;
    private int nextThreshold;                                  // Mốc kế tiếp chưa kích hoạt
    private bool running;

    public HpThresholdReductionPassiveEffect(ISkillOwner owner, HpThresholdReductionPassiveData data) : base(owner) { this.data = data; }

    public override void SetLevel(int skillLevel) { level = data.GetLevel(skillLevel); }

    public override void Enable()
    {
        if (running) return;                                    // Gọi nhiều lần vẫn an toàn (không đặt lại mốc)
        running = true;

        GameObject go = owner.Transform.gameObject;
        if (!go.TryGetComponent(out reduction)) reduction = go.AddComponent<BossDamageReduction>();
        reduction.Clear();

        nextThreshold = 0;
        owner.Health.OnHealthChanged -= HandleHealthChanged;
        owner.Health.OnHealthChanged += HandleHealthChanged;
    }

    public override void Disable()
    {
        running = false;
        owner.Health.OnHealthChanged -= HandleHealthChanged;
        if (reduction != null) reduction.Clear();
    }

    // Chạy mỗi khi máu đổi. Máu tụt qua một mốc thì thêm một lớp giảm sát thương
    private void HandleHealthChanged(float current, float max)
    {
        if (!running || level == null || reduction == null || max <= 0f || current <= 0f) return;

        float ratio = current / max;
        float[] thresholds = level.thresholds;
        if (thresholds == null) return;

        while (nextThreshold < thresholds.Length && ratio <= thresholds[nextThreshold])
        {
            reduction.Add(level.reduction, level.duration);
            nextThreshold++;
        }
    }
}