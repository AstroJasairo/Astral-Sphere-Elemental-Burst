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
    private State state;
    private float clawTimer;
    private Color normalColor = Color.white;
    private Action<Telegraph> clawCallback;                               // Lưu sẵn để không sinh rác mỗi lần cào

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        clawCallback = OnClawLanded;
        if (bodyRenderer != null) normalColor = bodyRenderer.color;
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

    // ---------- Kỹ năng: cào AOE ----------

    private void BeginClaw()
    {
        state = State.ClawWindup;
        Enemy.OverrideMovement = true;                // Ngừng đuổi theo trong lúc gồng
        rb.linearVelocity = Vector2.zero;

        // Dạng 1 trong bản vẽ: vòng tròn cảnh báo quanh BOSS, người chơi phải đi ra ngoài vòng để né.
        TelegraphSpawner.Instance.SpawnCircle(transform.position, skills.clawRadius, skills.clawWarning,
                                              skills.clawShrink, skills.clawDamage, clawCallback);
    }

    // Telegraph gọi khi vòng cam chạm đích (đã tự gây sát thương cho người chơi nếu còn trong vòng)
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
}