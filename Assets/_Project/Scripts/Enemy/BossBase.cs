using System;
using UnityEngine;

// Lớp cha cho mọi BOSS. Đi kèm với EnemyBase và HealthSystem trên cùng object.
[RequireComponent(typeof(EnemyBase), typeof(HealthSystem))]
public abstract class BossBase : MonoBehaviour
{
    // HUD lắng nghe để hiện thanh máu BOSS
    public static event Action<BossBase> BossSpawned;

    public EnemyBase Enemy { get; private set; }
    public HealthSystem Health { get; private set; }
    public bool IsImmune { get; private set; }
    public event Action<bool> ImmunityChanged;      // HUD lắng nghe để hiện nhãn "MIỄN NHIỄM"
    public string BossName => Enemy.Data != null ? Enemy.Data.enemyName : "BOSS";

    protected Transform player;

    protected virtual void Awake()
    {
        Enemy = GetComponent<EnemyBase>();
        Health = GetComponent<HealthSystem>();
    }

    protected virtual void OnEnable() { EnemyBase.EnemyKilled += HandleEnemyKilled; }
    protected virtual void OnDisable() { EnemyBase.EnemyKilled -= HandleEnemyKilled; }

    // BossSpawner gọi SAU KHI đã gọi EnemyBase.Init
    public void Begin()
    {
        player = GameManager.Instance.Player;
        IsImmune = false;
        Health.Invulnerable = false;
        OnBegin();
        BossSpawned?.Invoke(this);
    }

    // Lớp con khởi tạo kỹ năng, đặt bộ đếm giờ...
    protected abstract void OnBegin();

    private void HandleEnemyKilled(EnemyBase killed)
    {
        if (killed != Enemy) return;            // Sự kiện này phát cho MỌI quái chết, chỉ quan tâm chính mình

        Telegraph.CancelAll();                  // Xóa mọi vùng cảnh báo còn đang chạy
        GameManager.Instance.TriggerVictory();  // Đánh bại BOSS = thắng màn
    }

    // Lớp con gọi để bật/tắt miễn nhiễm
    protected void SetImmune(bool immune)
    {
        if (IsImmune == immune) return;
        IsImmune = immune;
        Health.Invulnerable = immune;
        ImmunityChanged?.Invoke(immune);
    }
}