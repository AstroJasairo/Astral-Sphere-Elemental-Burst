using System;
using UnityEngine;

// Hệ thống máu dùng chung cho Player và kẻ địch
public class HealthSystem : MonoBehaviour
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    public event Action<float, float> OnHealthChanged;   // (máu hiện tại, máu tối đa) -> dùng cho thanh máu UI
    public event Action OnDied;

    // Thiết lập (hoặc làm mới) máu. Quái lấy từ pool ra sẽ gọi lại hàm này để "hồi sinh"
    public void Init(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;                       // Đã chết thì không nhận thêm sát thương
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        if (IsDead) OnDied?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void IncreaseMax(float amount)
    {
        MaxHealth += amount;
        CurrentHealth += amount;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}