using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem))]
public class EnemyBase : MonoBehaviour
{
    // Sự kiện toàn cục "có quái bị giết". Sau này: hồi mana, cộng điểm, rơi đồ chỉ cần lắng nghe sự kiện này.
    public static event Action<EnemyBase> EnemyKilled;

    public EnemyData Data { get; private set; }

    private Rigidbody2D rb;
    private HealthSystem health;
    private Transform target;
    private float damage;
    private float nextAttackTime;
    private const float AttackCooldown = 1f;   // Chạm người chơi: 1 giây mới gây sát thương lại
    public static int ActiveCount { get; private set; }                       // Số quái đang hoạt động
    public static readonly HashSet<EnemyBase> Active = new HashSet<EnemyBase>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthSystem>();
        health.OnDied += HandleDied;   // Đăng ký 1 lần, vẫn còn hiệu lực khi object được tái sử dụng
    }

    // Spawner gọi ngay sau khi lấy quái từ pool. Hàm này "làm mới" toàn bộ trạng thái.
    public void Init(EnemyData data, float hpMultiplier, float damageMultiplier)
    {
        Data = data;
        damage = data.baseDamage * damageMultiplier;
        health.Init(data.baseHp * hpMultiplier);
        target = GameManager.Instance.Player;
        rb.linearVelocity = Vector2.zero;
        nextAttackTime = 0f;
    }

    private void FixedUpdate()
    {
        if (Data == null || target == null) return;
        // Hướng từ quái đến người chơi, rồi đặt vận tốc theo hướng đó
        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = dir * Data.moveSpeed;
    }

    // Collider của quái là Trigger, nên dùng OnTriggerStay2D. Hàm chạy liên tục khi đang chạm.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextAttackTime) return;   // Chưa hết hồi chiêu: thoát sớm cho nhẹ
        if (!other.TryGetComponent(out PlayerStats player)) return;   // Không phải người chơi thì bỏ qua

        player.TakeDamage(damage);
        nextAttackTime = Time.time + AttackCooldown;
    }

    private void HandleDied()
    {
        // Fix: Kẻ địch không còn tồn tại khi hết máu mà không được release về pool
        try
        {
            EnemyKilled?.Invoke(this);   // Có thể có hàm lắng nghe ném lỗi
        }
        finally
        {
            // "finally" LUÔN chạy, kể cả khi phía trên có lỗi.
            // Lỗi vẫn hiện trong Console để bạn biết mà sửa, nhưng quái không còn thành "quái ma".
            PoolManager.Instance.Release(gameObject);   // Trả về pool thay vì Destroy
        }
    }

    // OnEnable/OnDisable gọi mỗi khi quái được bật/tắt (lấy ra khỏi pool / trả về pool),
    // nên bao quát mọi đường trả về, không phụ thuộc vào event.
    private void OnEnable()
    {
        ActiveCount++;
        Active.Add(this);
    }

    private void OnDisable()
    {
        ActiveCount--;
        Active.Remove(this);
    }

    // Dịch chuyển quái tới vị trí mới
    public void TeleportTo(Vector2 position)
    {
        rb.position = position;
        transform.position = position;
    }
}