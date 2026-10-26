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
    public float Damage => damage;       // Sát thương đã nhân hệ số theo thời gian

    private Rigidbody2D rb;
    private HealthSystem health;
    private Transform target;
    private float damage;
    private float nextAttackTime;
    private float stopDistanceSqr;       // Quái tầm xa dừng lại khi tới khoảng cách này
    private const float AttackCooldown = 1f;   // Hồi chiêu sát thương khi chạm
    private StatusEffectHandler status;
    public bool OverrideMovement { get; set; }   // true: BOSS đang tự điều khiển (lao tới...), EnemyBase không đuổi theo nữa


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthSystem>();
        health.OnDied += HandleDied;
        status = GetComponent<StatusEffectHandler>();
        if (status == null) status = gameObject.AddComponent<StatusEffectHandler>();
    }

    private void OnEnable() { Active.Add(this); }
    private void OnDisable() { Active.Remove(this); }

    public void Init(EnemyData data, float hpMultiplier, float damageMultiplier)
    {
        Data = data;
        damage = data.baseDamage * damageMultiplier;
        health.Init(data.baseHp * hpMultiplier);
        target = GameManager.Instance.Player;
        rb.linearVelocity = Vector2.zero;
        nextAttackTime = 0f;
        OverrideMovement = false;
        status.ResetEffects();

        // Quái tầm xa dừng ở 90% tầm đánh. Cận chiến (attackRange = 0) thì luôn lao vào.
        float stop = data.attackRange * 0.9f;
        stopDistanceSqr = stop * stop;
    }

    private void FixedUpdate()
    {
        if (Data == null || target == null) return;
        if (OverrideMovement) return;
        if (status.TryGetForcedVelocity(out Vector2 forced))
        {
            rb.linearVelocity = forced;    // Bị trói hoặc đẩy lùi: bỏ qua việc đuổi theo
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;

        // Đã vào tầm bắn thì đứng yên (EnemyShooter lo việc bắn)
        if (stopDistanceSqr > 0f && toTarget.sqrMagnitude <= stopDistanceSqr)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = toTarget.normalized * Data.moveSpeed;
    }

    public void TeleportTo(Vector2 position)
    {
        rb.position = position;
        transform.position = position;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextAttackTime) return;
        if (!other.TryGetComponent(out PlayerStats player)) return;

        player.TakeDamage(damage);
        nextAttackTime = Time.time + AttackCooldown;
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