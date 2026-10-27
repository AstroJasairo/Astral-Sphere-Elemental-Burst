using System;
using UnityEngine;

public class RunStats : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    public int Gold { get; private set; }
    public int Score { get; private set; }
    public int Kills { get; private set; }

    private readonly RemainderAccumulator goldAcc = new RemainderAccumulator();
    private float goldMultiplier = 1f;

    // UI lắng nghe sự kiện này thay vì kiểm tra mỗi frame (đỡ tốn CPU và không sinh chuỗi mới liên tục)
    public event Action OnChanged;

    private void Awake()
    {
        goldMultiplier = 1f + MetaProgress.GetSharedBonus(SharedUpgradeType.Gold);
    }

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
        Gold += goldAcc.Add(amount * goldMultiplier);   // Giữ phần lẻ để bonus nhỏ vẫn có tác dụng
        OnChanged?.Invoke();
    }

    private void HandleKilled(EnemyBase enemy)
    {
        Kills++;
        Score += Mathf.RoundToInt(enemy.Data.scoreValue * stats.ScoreMultiplier);
        OnChanged?.Invoke();
    }
}