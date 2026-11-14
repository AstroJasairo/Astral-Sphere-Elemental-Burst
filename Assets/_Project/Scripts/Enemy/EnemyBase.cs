using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem), typeof(StatusEffectHandler))]
public class EnemyBase : MonoBehaviour
{
    public static event Action<EnemyBase> EnemyKilled;

    // Tập quái đang hoạt động. Số lượng lấy trực tiếp từ Active.Count nên không thể bị lệch.
    public static readonly HashSet<EnemyBase> Active = new HashSet<EnemyBase>();
    public static int ActiveCount => Active.Count;
    public static void ResetStatics() => Active.Clear();   // GameManager gọi khi vào trận mới

    public EnemyData Data { get; private set; }
    public Transform Target => target;   // EnemyShooter cần để ngắm
    public float MaxHealth => health.MaxHealth;     // Máu tối đa thực tế (đã nhân hệ số độ khó). Vẫn đọc được lúc phát EnemyKilled
    public bool OverrideMovement { get; set; }   // true: BOSS đang tự điều khiển (lao tới...), EnemyBase không đuổi theo nữa

    // Debuff của quái. Thuộc tính lấy lười: tự thêm component nếu prefab chưa có
    private EnemyDebuffSystem debuffs;
    public EnemyDebuffSystem Debuffs
    {
        get
        {
            if (debuffs == null && !TryGetComponent(out debuffs))
                debuffs = gameObject.AddComponent<EnemyDebuffSystem>();
            return debuffs;
        }
    }

    // Các hệ số do debuff (BOSS và EnemyShooter đọc)
    public float Damage => damage * Debuffs.DamageDealtMultiplier;     // Sát thương đã nhân hệ số theo thời gian và Exhaust
    public float DamageDealtMultiplier => Debuffs.DamageDealtMultiplier;
    public float SpeedMultiplier => Debuffs.SpeedMultiplier;           // Slow
    public float AttackSpeedMultiplier => Debuffs.AttackSpeedMultiplier;   // Tê cóng
    public bool IsStunned => Debuffs.IsStunned;

    private Rigidbody2D rb;
    private HealthSystem health;
    private Transform target;
    private float damage;
    private float defense;               // DEF thực tế = baseDef x hệ số theo thời gian
    private float nextAttackTime;
    private float stopDistanceSqr;       // Quái tầm xa dừng lại khi tới khoảng cách này
    private const float AttackCooldown = 1f;   // Hồi chiêu sát thương khi chạm (chia cho tốc độ đánh)
    private StatusEffectHandler status;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthSystem>();
        health.OnDied += HandleDied;
        health.IncomingFilter = FilterIncoming;                  // DEF, kháng, miễn nhiễm, Vulnerable (gán một lần)
        status = GetComponent<StatusEffectHandler>();
        if (status == null) status = gameObject.AddComponent<StatusEffectHandler>();
    }

    private void OnEnable() { Active.Add(this); }
    private void OnDisable() { Active.Remove(this); }

    // defMultiplier là tham số TÙY CHỌN ở cuối: chỗ gọi cũ (BossSpawner) vẫn biên dịch, BOSS không tăng DEF theo thời gian
    public void Init(EnemyData data, float hpMultiplier, float damageMultiplier, float defMultiplier = 1f)
    {
        Data = data;
        damage = data.baseDamage * damageMultiplier;
        defense = data.baseDef * defMultiplier;
        health.Init(data.baseHp * hpMultiplier);
        target = GameManager.Instance.Player;
        rb.linearVelocity = Vector2.zero;
        nextAttackTime = 0f;
        OverrideMovement = false;
        status.ResetEffects();
        Debuffs.ResetAll();                                      // Quái lấy lại từ pool: xóa debuff và DOT cũ

        // Quái tầm xa dừng ở 90% tầm đánh. Cận chiến (attackRange = 0) thì luôn lao vào.
        float stop = data.attackRange * 0.9f;
        stopDistanceSqr = stop * stop;
    }

    // Sát thương nhận vào theo nguyên tố: miễn nhiễm, kháng, DEF, rồi Vulnerable. Pure bỏ qua miễn nhiễm, kháng, DEF.
    private float FilterIncoming(float amount, ElementType element)
    {
        if (Data == null) return amount;

        if (element != ElementType.Pure)
        {
            if (ElementMaskUtil.Contains(Data.elementImmunity, element)) return 0f;

            if (Data.elementResist != null) amount *= 1f - Data.elementResist.GetClamped(element);
            amount *= 100f / (100f + Mathf.Max(0f, defense));
        }

        return amount * Debuffs.DamageTakenMultiplier;          // Vulnerable áp dụng cả Pure
    }

    // BOSS gọi khi bắt đầu một đòn lao: đòn lao luôn gây sát thương nếu trúng, dù đang trong hồi chiêu chạm
    public void ResetContactCooldown() { nextAttackTime = 0f; }

    private void FixedUpdate()
    {
        if (Data == null || target == null) return;
        if (OverrideMovement) return;
        if (status.TryGetForcedVelocity(out Vector2 forced))
        {
            rb.linearVelocity = forced;    // Bị trói hoặc đẩy lùi: bỏ qua việc đuổi theo
            return;
        }

        if (Debuffs.IsStunned)             // Choáng: đứng yên (đẩy lùi ở trên vẫn tác dụng)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;

        // Đã vào tầm bắn thì đứng yên (EnemyShooter lo việc bắn)
        if (stopDistanceSqr > 0f && toTarget.sqrMagnitude <= stopDistanceSqr)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = toTarget.normalized * Data.moveSpeed * Debuffs.SpeedMultiplier;   // Slow
    }

    public void TeleportTo(Vector2 position)
    {
        rb.position = position;
        transform.position = position;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextAttackTime) return;
        if (Debuffs.IsStunned) return;                           // Quái bị Choáng không gây sát thương khi chạm
        if (!other.TryGetComponent(out PlayerStats player)) return;

        player.TakeDamage(Damage, Data.damageElement);           // Damage đã nhân hệ số Exhaust
        HitEffect.ApplyAll(Data.hitEffects, player);             // Áp dụng cả khi khiên hấp thụ hết sát thương
        nextAttackTime = Time.time + AttackCooldown / Debuffs.AttackSpeedMultiplier;
    }

    private void HandleDied()
    {
        try
        {
            EnemyKilled?.Invoke(this);
        }
        finally
        {
            PoolManager.Instance.Release(gameObject);   // Luôn trả về pool, kể cả khi có hàm lắng nghe bị lỗi
        }
    }
}