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
    public float DamageTakenMultiplier { get; set; } = 1f;       // < 1 khi BOSS đang có buff giảm sát thương
    // Bộ lọc sát thương đến (quái dùng cho DEF/kháng/miễn nhiễm). Người chơi để null.
    public Func<float, ElementType, float> IncomingFilter { get; set; }
    public event Action<float, float> OnHealthChanged;             // (máu hiện tại, máu tối đa hiệu lực)
    public event Action OnDied;

    public void Init(float maxHealth)
    {
        DamageTakenMultiplier = 1f;
        BaseMaxHealth = maxHealth; MaxReduction = 0f;
        MaxHealth = maxHealth; CurrentHealth = maxHealth; Invulnerable = false;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // Đòn có nguyên tố: đi qua bộ lọc (DEF, kháng, miễn nhiễm) rồi mới trừ máu.
    // TakeDamage(amount) vẫn là sát thương THÔ (không DEF): dùng cho máu của người chơi đã tính sẵn.
    public void TakeHit(float amount, ElementType element)
    {
        if (IsDead || Invulnerable) return;
        if (IncomingFilter != null) amount = IncomingFilter(amount, element);
        if (amount <= 0f) return;                       // Miễn nhiễm hoặc bị giảm hết
        TakeDamage(amount);                             // BOSS: DamageTakenMultiplier áp dụng ở đây
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || Invulnerable) return;
        amount *= DamageTakenMultiplier;
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