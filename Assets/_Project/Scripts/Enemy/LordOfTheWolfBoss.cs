using System;
using UnityEngine;

public class LordOfTheWolfBoss : BossBase
{
    private enum State
    {
        Chasing,        // Lao theo người chơi với tốc độ cao
        ClawWindup      // Đứng lại, vòng cảnh báo đang chạy
    }

    [SerializeField] private SpriteRenderer bodyRenderer;                 // Hình thân (đổi màu khi miễn nhiễm)
    [SerializeField] private Color immuneColor = new Color(0.55f, 0.65f, 0.9f);
    [SerializeField] private GameObject clawEffectPrefab;                 // Hiệu ứng khi cào xong (prefab có TimedEffect). Tùy chọn

    private LordOfTheWolfData skills;
    private Rigidbody2D rb;
    private BossDamageReduction reduction;
    private State state;
    private float clawTimer;
    private int nextThreshold;                                            // Mốc máu kế tiếp chưa kích hoạt
    private Color normalColor = Color.white;
    private Action<Telegraph> clawCallback;                               // Lưu sẵn để không sinh rác mỗi lần cào

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        clawCallback = OnClawLanded;
        if (bodyRenderer != null) normalColor = bodyRenderer.color;
        if (!TryGetComponent(out reduction)) reduction = gameObject.AddComponent<BossDamageReduction>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (Health != null) Health.OnHealthChanged -= HandleHealthChanged;   // Health đã lưu sẵn ở BossBase, an toàn khi hủy
    }

    protected override void OnBegin()
    {
        skills = Enemy.Data as LordOfTheWolfData;
        if (skills == null)
        {
            Debug.LogError("LordOfTheWolfBoss: EnemyData phải là LordOfTheWolfData.", this);
            return;
        }

        state = State.Chasing;
        clawTimer = skills.firstClawDelay;
        if (bodyRenderer != null) bodyRenderer.color = normalColor;

        nextThreshold = 0;
        reduction.Clear();
        Health.OnHealthChanged -= HandleHealthChanged;                    // Chống đăng ký trùng khi BOSS lấy lại từ pool
        Health.OnHealthChanged += HandleHealthChanged;
    }

    private void Update()
    {
        if (skills == null) return;

        UpdateImmunity();

        if (state != State.Chasing) return;

        clawTimer -= Time.deltaTime;
        if (clawTimer <= 0f) BeginClaw();
    }

    // ---------- Nội tại: miễn nhiễm khi người chơi ở xa ----------

    private void UpdateImmunity()
    {
        float sqr = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;

        // Có vùng đệm: bật miễn nhiễm khi xa hơn immuneDistance, nhưng chỉ tắt khi lại gần hơn (immuneDistance - margin)
        float limit = IsImmune ? skills.immuneDistance - skills.immuneMargin : skills.immuneDistance;
        bool immune = sqr > limit * limit;

        bool changed = immune != IsImmune;
        SetImmune(immune);

        if (changed && bodyRenderer != null)
            bodyRenderer.color = immune ? immuneColor : normalColor;
    }

    // ---------- Nội tại: giảm sát thương khi máu xuống thấp ----------

    // Chạy mỗi khi máu đổi. Máu tụt qua một mốc thì thêm một lớp giảm sát thương (mỗi mốc đúng một lần)
    private void HandleHealthChanged(float current, float max)
    {
        if (skills == null || max <= 0f || current <= 0f) return;

        float ratio = current / max;
        float[] thresholds = skills.reductionHpThresholds;
        while (nextThreshold < thresholds.Length && ratio <= thresholds[nextThreshold])
        {
            reduction.Add(skills.thresholdReduction, skills.thresholdDuration);
            nextThreshold++;
        }
    }

    // ---------- Kỹ năng: cào AOE ----------

    private void BeginClaw()
    {
        state = State.ClawWindup;
        Enemy.OverrideMovement = true;                // Ngừng đuổi theo trong lúc gồng
        rb.linearVelocity = Vector2.zero;

        // Dạng 1 trong bản vẽ: vòng tròn cảnh báo quanh BOSS, người chơi phải đi ra ngoài vòng để né.
        TelegraphSpawner.Instance.SpawnCircle(transform.position, skills.clawRadius, skills.clawWarning,
                                              skills.clawShrink, skills.clawDamage, clawCallback,
                                              Enemy.Data.damageElement, skills.clawEffects);
    }

    // Telegraph gọi khi vòng cam chạm đích (đã tự gây sát thương và hiệu ứng cho người chơi nếu còn trong vòng)
    private void OnClawLanded(Telegraph t)
    {
        if (state != State.ClawWindup) return;

        if (clawEffectPrefab != null)
        {
            var sector = t as SectorTelegraph;
            if (sector != null)
            {
                GameObject fx = PoolManager.Instance.Get(clawEffectPrefab, t.transform.position, Quaternion.identity);
                fx.GetComponent<TimedEffect>().Play(sector.Radius * 2f);
            }
        }

        state = State.Chasing;
        Enemy.OverrideMovement = false;               // Trả quyền di chuyển lại cho EnemyBase
        clawTimer = skills.clawInterval;
    }

    // ---------- Debug: kiểm tra mốc máu mà không cần đánh thật ----------
    [ContextMenu("Debug: Hit 10% Max HP")]
    private void DebugHit() { if (Debug.isDebugBuild) Health.TakeDamage(Health.MaxHealth * 0.1f); }

    [ContextMenu("Debug: Log Reduction")]
    private void DebugLogReduction()
    {
        Debug.Log($"[LotW] hệ số sát thương nhận x{Health.DamageTakenMultiplier:0.###}, số lớp giảm {reduction.Stacks}, máu {Health.CurrentHealth:0}/{Health.MaxHealth:0}");
    }
}