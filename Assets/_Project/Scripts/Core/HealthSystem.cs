using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float BaseMaxHealth { get; private set; }               // Máu tối đa gốc (chưa bị Yếu đuối)
    public float MaxHealth { get; private set; }                   // Máu tối đa ĐANG HIỆU LỰC (đã trừ Yếu đuối)
    public float CurrentHealth { get; private set; }
    public float MaxReduction { get; private set; }                // 0..1: phần máu tối đa đang bị giảm tạm thời
    public bool IsDead => CurrentHealth <= 0f;
    public float DamageTakenMultiplier { get; set; } = 1f;       // < 1 khi BOSS đang có buff giảm sát thương

    // Bộ lọc sát thương đến (quái dùng cho DEF/kháng/miễn nhiễm). Người chơi để null.
    public Func<float, ElementType, float> IncomingFilter { get; set; }

    // Các nguồn miễn nhiễm (sát thương, DOT, khống chế, debuff, gây chết). Dùng chung cho người chơi và quái
    public ImmunitySet Immunities { get; } = new ImmunitySet();

    // Bất tử hoàn toàn (mọi sát thương, kể cả Mechanic). Gán trực tiếp (BOSS cũ) hoặc theo nguồn khóa (SetInvulnerable)
    private bool invulnerableManual;
    private readonly HashSet<object> invulnerableSources = new HashSet<object>();
    public bool Invulnerable
    {
        get { return invulnerableManual || invulnerableSources.Count > 0; }
        set { invulnerableManual = value; }
    }

    public void SetInvulnerable(object source, bool on)
    {
        if (on) invulnerableSources.Add(source);
        else invulnerableSources.Remove(source);
    }

    public event Action<float, float> OnHealthChanged;             // (máu hiện tại, máu tối đa hiệu lực)
    public event Action OnDied;

    // Phát sau mỗi đòn trúng (TakeHit) đã trừ máu: tham số = lượng máu thực mất
    public event Action<float> DamageTaken;

    // "Người bảo vệ khi chết" (Thiên thần hộ mệnh). Gọi khi máu về 0: trả về true nếu đã cứu (phải gọi Revive)
    public event Func<bool> LethalGuards;

    public void Init(float maxHealth)
    {
        DamageTakenMultiplier = 1f;
        BaseMaxHealth = maxHealth; MaxReduction = 0f;
        MaxHealth = maxHealth; CurrentHealth = maxHealth;
        invulnerableManual = false;
        invulnerableSources.Clear();
        Immunities.Clear();
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // Đòn có nguyên tố: qua miễn nhiễm, bộ lọc (DEF, kháng) rồi mới trừ máu.
    // TakeDamage(amount) vẫn là sát thương THÔ (không DEF): dùng cho máu của người chơi đã tính sẵn.
    public void TakeHit(float amount, ElementType element, DamageKind kind = DamageKind.Normal)
    {
        if (IsDead || Invulnerable) return;
        if (kind == DamageKind.Normal && Immunities.Has(ImmunityFlags.Damage)) return;   // Miễn nhiễm sát thương chỉ chặn Normal

        if (IncomingFilter != null) amount = IncomingFilter(amount, element);
        if (amount <= 0f) return;                       // Miễn nhiễm nguyên tố hoặc bị giảm hết

        TakeDamage(amount);                             // BOSS: DamageTakenMultiplier áp dụng ở đây
        DamageTaken?.Invoke(amount * DamageTakenMultiplier);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || Invulnerable) return;
        amount *= DamageTakenMultiplier;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (IsDead) TryPreventDeath();                  // Thiên thần hộ mệnh có thể cứu (đã Revive nên IsDead = false)

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        if (IsDead) OnDied?.Invoke();
    }

    private bool TryPreventDeath()
    {
        if (LethalGuards == null) return false;

        foreach (Delegate d in LethalGuards.GetInvocationList())      // Chỉ chạy khi sắp chết (hiếm) nên cấp phát là chấp nhận được
            if (((Func<bool>)d)()) return true;

        return false;
    }

    // Hồi sinh với "hp" máu (chỉ khi đang chết). Người bảo vệ khi chết gọi.
    public void Revive(float hp)
    {
        if (!IsDead) return;
        CurrentHealth = Mathf.Clamp(hp, Mathf.Min(1f, MaxHealth), MaxHealth);
    }

    // Hiệu ứng gây chết (instant kill). Không bị Invulnerable hay Miễn nhiễm sát thương chặn,
    // chỉ bị cờ Death chặn. Thiên thần hộ mệnh KHÔNG cứu được. Trả về true nếu đã giết.
    public bool Kill()
    {
        if (IsDead) return false;
        if (Immunities.Has(ImmunityFlags.Death)) return false;

        CurrentHealth = 0f;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnDied?.Invoke();
        return true;
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

    // Tăng HOẶC giảm máu tối đa gốc (dùng khi bật/tắt Liên kết linh hồn).
    // Máu hiện tại đổi cùng lượng nhưng KHÔNG BAO GIỜ làm chết người chơi (tối thiểu 1).
    public void ShiftMax(float delta)
    {
        if (IsDead || Mathf.Approximately(delta, 0f)) return;

        float oldMax = MaxHealth;
        BaseMaxHealth = Mathf.Max(1f, BaseMaxHealth + delta);
        RecalculateMax();

        float change = MaxHealth - oldMax;
        CurrentHealth = Mathf.Clamp(CurrentHealth + change, Mathf.Min(1f, MaxHealth), MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void RecalculateMax()
    {
        MaxHealth = Mathf.Max(1f, BaseMaxHealth * (1f - MaxReduction));   // luôn chừa tối thiểu 1 máu
    }
}