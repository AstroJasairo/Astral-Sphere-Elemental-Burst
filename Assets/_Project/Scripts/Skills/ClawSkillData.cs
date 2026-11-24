using System;
using System.Collections.Generic;
using UnityEngine;

// Cào AOE: đứng lại, vòng tròn cảnh báo quanh quái, người chơi phải đi ra ngoài vòng để né.
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Boss/Claw")]
public class ClawSkillData : BossSkillData
{
    [Serializable]
    public class ClawLevel : BossSkillLevel
    {
        public float radius = 3.5f;
        public float warning = 1.2f;
        public float damage = 30f;
        public bool shrink = false;        // false: vòng cam lớn dần từ tâm. true: nhỏ dần từ viền vào tâm
    }

    [Header("Theo cấp (1 cấp mặc định)")]
    public ClawLevel[] levels = { new ClawLevel { firstDelay = 4f, cooldown = 6f } };

    public List<HitEffect> effects = new List<HitEffect>();       // Hiệu ứng khi đòn cào trúng (vd Câm lặng 5s)
    public GameObject clawEffectPrefab;                            // Hiệu ứng khi cào xong (prefab có TimedEffect). Tùy chọn

    public override int LevelCount => levels != null ? levels.Length : 0;

    public override BossSkillRuntime CreateRuntime(EnemyBase enemy, int level)
    {
        ClawLevel l = Pick(levels, level);
        return l == null ? null : new ClawRuntime(enemy, this, l);
    }

    private void Reset() { exclusive = true; pausedWhileOthersExecute = false; }

    [ContextMenu("Fill Lord Of The Wolf Defaults")]
    private void FillDefaults()
    {
        skillName = "Cào";
        mode = BossSkillMode.Cooldown;
        exclusive = true;
        pausedWhileOthersExecute = false;
        levels = new[] { new ClawLevel { firstDelay = 4f, cooldown = 6f, radius = 3.5f, warning = 1.2f, damage = 30f, shrink = false } };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class ClawRuntime : BossSkillRuntime
{
    private readonly ClawSkillData data;
    private readonly ClawSkillData.ClawLevel level;
    private readonly Action<Telegraph> landed;

    public ClawRuntime(EnemyBase enemy, ClawSkillData data, ClawSkillData.ClawLevel level) : base(enemy, data)
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
        if (Phase != BossCastPhase.Idle) return;                      // Đang gồng: chờ telegraph
        if (!CountdownReady(scaledDt, paused, canStart)) return;

        Phase = BossCastPhase.Warning;
        enemy.OverrideMovement = true;                // Ngừng đuổi theo trong lúc gồng
        enemy.Body.linearVelocity = Vector2.zero;

        TelegraphSpawner.Instance.SpawnCircle(enemy.transform.position, level.radius,
                                              level.warning / enemy.AttackSpeedMultiplier,    // Tê cóng: telegraph chạy chậm hơn
                                              level.shrink, level.damage * enemy.DamageDealtMultiplier, landed,
                                              enemy.Data.damageElement, data.effects);
    }

    // Telegraph gọi khi vòng cam chạm đích (đã tự gây sát thương và hiệu ứng nếu người chơi còn trong vòng)
    private void OnLanded(Telegraph t)
    {
        if (Phase != BossCastPhase.Warning) return;

        if (data.clawEffectPrefab != null)
        {
            var sector = t as SectorTelegraph;
            if (sector != null)
            {
                GameObject fx = PoolManager.Instance.Get(data.clawEffectPrefab, t.transform.position, Quaternion.identity);
                fx.GetComponent<TimedEffect>().Play(sector.Radius * 2f);
            }
        }

        enemy.OverrideMovement = false;               // Trả quyền di chuyển lại cho EnemyBase
        FinishCast(level.cooldown);
    }

    public override void Cancel()
    {
        if (Phase == BossCastPhase.Idle) return;
        Phase = BossCastPhase.Idle;
        enemy.OverrideMovement = false;
    }
}