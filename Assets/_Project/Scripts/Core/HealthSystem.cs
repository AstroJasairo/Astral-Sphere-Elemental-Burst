using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float BaseMaxHealth { get; private set; }               // Máu tối đa gốc (chưa bị Yếu đuối)
    public float MaxHealth { get; private set; }                   // Máu tối đa ĐANG HIỆU LỰC (đã trừ Yếu đuối)
    public float CurrentHealth { get; private set; }
    public float MaxReduction { get; private set; }                // 0..1: phần máu tối đa đang bị giảm tạm thời
    public bool IsDead => CurrentHealth <= 0f;
    public bool Invulnerable { get; set; }
    public event Action<float, float> OnHealthChanged;             // (máu hiện tại, máu tối đa hiệu lực)
    public event Action OnDied;

    public void Init(float maxHealth)
    {
        BaseMaxHealth = maxHealth; MaxReduction = 0f;
        MaxHealth = maxHealth; CurrentHealth = maxHealth; Invulnerable = false;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || Invulnerable) return;
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

    // Tăng máu tối đa gốc (nâng cấp trong trận). Máu hiện tại tăng đúng bằng phần tối đa hiệu lực tăng thêm.
    public void IncreaseMax(float amount)
    {
        float oldMax = MaxHealth;
        BaseMaxHealth += amount;
        RecalculateMax();
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + (MaxHealth - oldMax));
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // Yếu đuối: giảm TẠM THỜI máu tối đa (0.2 = giảm 20%). Máu hiện tại bị kẹp xuống;
    // khi hết hiệu ứng (fraction = 0) máu tối đa trở lại nhưng máu hiện tại KHÔNG tự hồi.
    public void SetMaxReduction(float fraction)
    {
        MaxReduction = Mathf.Clamp01(fraction);
        RecalculateMax();
        if (!IsDead && CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void RecalculateMax()
    {
        MaxHealth = Mathf.Max(1f, BaseMaxHealth * (1f - MaxReduction));   // luôn chừa tối thiểu 1 máu
    }
}