using System;
using System.Collections.Generic;
using UnityEngine;

// Sét đánh: mỗi lần thi triển tạo nhiều vòng cảnh báo. Vòng đầu nhắm thẳng người chơi, các vòng còn lại rải ngẫu nhiên quanh đó.
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Boss/Lightning Strikes")]
public class LightningStrikesSkillData : BossSkillData
{
    [Serializable]
    public class StrikeLevel : BossSkillLevel
    {
        public int strikesPerWave = 3;
        public float radius = 1.8f;
        public float warning = 1.5f;       // Thời gian vòng cam lớn dần (thời gian để né)
        public float spread = 5f;          // Các vòng còn lại rải trong bán kính này quanh người chơi
        public float damage = 20f;
    }

    [Header("Theo cấp (1 cấp mặc định; cấp 2, 3 dành cho độ khó cao)")]
    public StrikeLevel[] levels = { new StrikeLevel { firstDelay = 2f, cooldown = 2.5f } };

    public List<HitEffect> effects = new List<HitEffect>();       // Hiệu ứng khi vòng sét trúng người chơi (vd Choáng 3s)
    public GameObject strikeEffectPrefab;                          // Chớp sáng khi sét đánh (prefab có TimedEffect). Tùy chọn

    public override int LevelCount => levels != null ? levels.Length : 0;

    public override BossSkillRuntime CreateRuntime(EnemyBase enemy, int level)
    {
        StrikeLevel l = Pick(levels, level);
        return l == null ? null : new LightningStrikesRuntime(enemy, this, l);
    }

    private void Reset() { exclusive = false; pausedWhileOthersExecute = true; }

    [ContextMenu("Fill Thunder Bird Defaults")]
    private void FillDefaults()
    {
        skillName = "Sét đánh";
        mode = BossSkillMode.Cooldown;
        exclusive = false;
        pausedWhileOthersExecute = true;
        levels = new[] { new StrikeLevel { firstDelay = 2f, cooldown = 2.5f, strikesPerWave = 3, radius = 1.8f, warning = 1.5f, spread = 5f, damage = 20f } };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class LightningStrikesRuntime : BossSkillRuntime
{
    private readonly LightningStrikesSkillData data;
    private readonly LightningStrikesSkillData.StrikeLevel level;
    private readonly Action<Telegraph> landed;                // Lưu sẵn: không sinh rác mỗi lần thi triển

    public LightningStrikesRuntime(EnemyBase enemy, LightningStrikesSkillData data,
                                   LightningStrikesSkillData.StrikeLevel level) : base(enemy, data)
    {
        this.data = data;
        this.level = level;
        landed = OnLanded;
    }

    public override void Begin()
    {
        Phase = BossCastPhase.Idle;
        timer = level.firstDelay;
    }

    public override void Tick(float scaledDt, float rawDt, bool paused, bool canStart)
    {
        if (!CountdownReady(scaledDt, paused, canStart)) return;

        CastWave();
        FinishCast(level.cooldown);
    }

    private void CastWave()
    {
        Transform target = enemy.Target;
        if (target == null) return;

        Vector2 playerPos = target.position;
        float warning = level.warning / enemy.AttackSpeedMultiplier;        // Tê cóng: cảnh báo chạy chậm hơn
        float damage = level.damage * enemy.DamageDealtMultiplier;          // Exhaust: giảm sát thương gây ra

        for (int i = 0; i < level.strikesPerWave; i++)
        {
            // Vòng đầu nhắm đúng chỗ người chơi đang đứng (buộc phải di chuyển), các vòng còn lại rải ngẫu nhiên quanh đó
            Vector2 center = i == 0 ? playerPos : playerPos + UnityEngine.Random.insideUnitCircle * level.spread;

            TelegraphSpawner.Instance.SpawnCircle(center, level.radius, warning, false, damage, landed,
                                                  enemy.Data.damageElement, data.effects);
        }
    }

    // Telegraph gọi khi vòng cam chạm viền: tạo hiệu ứng chớp sáng
    private void OnLanded(Telegraph t)
    {
        if (data.strikeEffectPrefab == null) return;
        var sector = t as SectorTelegraph;
        if (sector == null) return;

        GameObject fx = PoolManager.Instance.Get(data.strikeEffectPrefab, t.transform.position, Quaternion.identity);
        fx.GetComponent<TimedEffect>().Play(sector.Radius * 2f);
    }
}