using System;
using UnityEngine;

public class RunStats : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    public int Gold { get; private set; }
    public int Score { get; private set; }
    public int Kills { get; private set; }

    // UI lắng nghe sự kiện này thay vì kiểm tra mỗi frame (đỡ tốn CPU và không sinh chuỗi mới liên tục)
    public event Action OnChanged;

    private void OnEnable()
    {
        Pickup.Collected += HandleCollected;
        EnemyBase.EnemyKilled += HandleKilled;
    }

    private void OnDisable()
    {
        Pickup.Collected -= HandleCollected;
        EnemyBase.EnemyKilled -= HandleKilled;
    }

    private void HandleCollected(PickupType type, int amount)
    {
        if (type != PickupType.Gold) return;
        Gold += amount;     // Sau này nhân thêm hệ số nâng cấp chung "Vàng"
        OnChanged?.Invoke();
    }

    private void HandleKilled(EnemyBase enemy)
    {
        Kills++;
        Score += Mathf.RoundToInt(enemy.Data.scoreValue * stats.ScoreMultiplier);
        OnChanged?.Invoke();
    }
}