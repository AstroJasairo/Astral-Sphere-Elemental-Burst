using System;
using UnityEngine;

// Miễn nhiễm khi ở xa: người chơi đứng xa hơn một khoảng thì chủ sở hữu miễn nhiễm (mặc định: sát thương Normal).
// Có vùng đệm để không chớp bật tắt: bật khi xa hơn distance, chỉ tắt khi lại gần hơn (distance - margin).
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Immune When Far")]
public class ImmuneWhenFarPassiveData : PassiveSkillData
{
    [Serializable]
    public class FarLevel
    {
        public float distance = 8f;
        public float margin = 0.8f;
    }

    public ImmunityFlags immunity = ImmunityFlags.Damage;

    [Header("Theo cấp (1 cấp mặc định)")]
    public FarLevel[] levels = { new FarLevel() };

    public FarLevel GetLevel(int level)
    {
        if (levels == null || levels.Length == 0) return null;
        return levels[Mathf.Clamp(level, 1, levels.Length) - 1];
    }

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new ImmuneWhenFarPassiveEffect(stats, this);
    public override PassiveSkillEffect CreateEffect(ISkillOwner owner) => new ImmuneWhenFarPassiveEffect(owner, this);

    public override string DescribeLevel(int level)
    {
        FarLevel l = GetLevel(level);
        return l == null ? "" : $"Miễn nhiễm ({immunity}) khi kẻ địch đứng xa hơn {l.distance:0.#}.";
    }

    [ContextMenu("Fill Lord Of The Wolf Defaults")]
    private void FillDefaults()
    {
        skillName = "Miễn nhiễm khi ở xa";
        immunity = ImmunityFlags.Damage;
        levels = new[] { new FarLevel { distance = 8f, margin = 0.8f } };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class ImmuneWhenFarPassiveEffect : PassiveSkillEffect
{
    private readonly ImmuneWhenFarPassiveData data;
    private ImmuneWhenFarPassiveData.FarLevel level;
    private bool running, immune;

    public ImmuneWhenFarPassiveEffect(ISkillOwner owner, ImmuneWhenFarPassiveData data) : base(owner) { this.data = data; }

    public override void SetLevel(int skillLevel) { level = data.GetLevel(skillLevel); }

    public override void Enable() { running = true; }

    public override void Disable()
    {
        running = false;
        if (immune) { immune = false; owner.SetImmunity(this, ImmunityFlags.None); }       // Gỡ SẠCH
    }

    public override void Tick(float deltaTime)
    {
        if (!running || level == null) return;

        Transform player = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (player == null) return;

        float sqr = ((Vector2)player.position - (Vector2)owner.Transform.position).sqrMagnitude;
        float limit = immune ? level.distance - level.margin : level.distance;
        bool now = sqr > limit * limit;

        if (now == immune) return;
        immune = now;
        owner.SetImmunity(this, immune ? data.immunity : ImmunityFlags.None);
    }
}