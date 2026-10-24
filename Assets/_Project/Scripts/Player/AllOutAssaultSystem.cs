using System;
using UnityEngine;

// Máy trạng thái 2 pha: Charging (nạp mana) <-> Active (đang hiệu lực)
public class AllOutAssaultSystem : MonoBehaviour
{
    [SerializeField] private AllOutAssaultData data;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private bool logPerkBonus;      // Bật để in bonus sát thương ra Console (kiểm tra perk 5 sao)

    public bool IsActive { get; private set; }
    public float Mana { get; private set; }

    // MP càng cao, mana cần càng ít
    public float MaxMana => data.maxMana * stats.ManaCostMultiplier;

    // Các sự kiện cho UI và hiệu ứng, hệ thống này không cần biết ai đang nghe
    public event Action OnActivated;
    public event Action OnEnded;
    public event Action<float> OnProgressChanged;    // 0..1: đang nạp = mana/tối đa; đang hiệu lực = thời gian còn lại

    private float activeTimer;

    private void OnEnable()
    {
        EnemyBase.EnemyKilled += HandleKilled;
        Pickup.Collected += HandleCollected;
    }

    private void OnDisable()
    {
        EnemyBase.EnemyKilled -= HandleKilled;
        Pickup.Collected -= HandleCollected;
    }

    private void HandleKilled(EnemyBase enemy)
    {
        AddMana(enemy.Data.manaValue);
    }

    private void HandleCollected(PickupType type, int amount)
    {
        if (type != PickupType.ManaPotion) return;
        Mana = MaxMana;      // Bình mana: nạp đầy ngay
        Activate();          // Đang hiệu lực thì Activate chỉ làm mới thời gian
    }

    public void AddMana(float amount)
    {
        if (IsActive) return;   // Trong lúc hiệu lực không nạp thêm

        Mana = Mathf.Min(Mana + amount, MaxMana);
        OnProgressChanged?.Invoke(Mana / MaxMana);
        if (Mana >= MaxMana) Activate();
    }

    private void Activate()
    {
        activeTimer = data.duration;   // Gán trước: nếu đang hiệu lực thì đây chính là "làm mới thời gian"
        if (IsActive) return;

        IsActive = true;
        stats.SetAllOutAssault(true, data.atkBonusPct, data.speedBonusPct, data.cooldownMultiplier);
        OnActivated?.Invoke();
        LogBonus("BẬT");
    }

    private void End()
    {
        IsActive = false;
        Mana = 0f;
        stats.SetAllOutAssault(false, 0f, 0f, 1f);
        OnEnded?.Invoke();
        OnProgressChanged?.Invoke(0f);
        LogBonus("TẮT");
    }

    private void Update()
    {
        if (!IsActive)
        {
            // Phòng khi nâng MP làm mana tối đa giảm xuống dưới mana hiện có
            if (Mana > 0f && Mana >= MaxMana) Activate();
            return;
        }

        // Time.deltaTime = 0 khi game tạm dừng (chọn nâng cấp), nên đồng hồ tự dừng theo
        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0f)
        {
            End();
            return;
        }
        OnProgressChanged?.Invoke(activeTimer / data.duration);   // Thanh mana cạn dần
    }

    private void LogBonus(string state)
    {
        if (!logPerkBonus) return;
        Debug.Log($"All Out Assault {state} | bonus Lôi = {stats.GetDamageBonus(ElementType.Lightning)}, " +
                  $"bonus Bóng tối = {stats.GetDamageBonus(ElementType.Dark)}");
    }

    // Chuột phải vào component trong Inspector (khi đang Play) để thử nhanh, khỏi phải giết 50 con quái
    [ContextMenu("Debug: Fill Mana")]
    private void DebugFillMana()
    {
        AddMana(MaxMana);
    }
}