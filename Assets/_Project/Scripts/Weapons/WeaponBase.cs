using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponData data;
    protected int level = 1;
    protected PlayerStats owner;
    private float timer;
    private const float MinCooldown = 0.1f;   // Sàn: không bao giờ bắn nhanh hơn 10 lần/giây, bảo vệ hiệu năng

    public WeaponData Data => data;
    public int Level => level;
    public bool IsMaxLevel => data != null && level >= data.levels.Length;

    // Thông số của cấp hiện tại (level 1 -> phần tử 0)
    protected WeaponLevel CurrentLevel => data.levels[Mathf.Clamp(level, 1, data.levels.Length) - 1];

    // Hồi chiêu thực tế = hồi chiêu gốc x hệ số hiện tại (bình thường = 1, trong All Out Assault < 1)
    private float CurrentCooldown
    {
        get
        {
            float multiplier = owner != null ? owner.CooldownMultiplier : 1f;
            return Mathf.Max(MinCooldown, data.cooldown * multiplier);
        }
    }

    protected virtual void Awake()
    {
        owner = GetComponentInParent<PlayerStats>();   // Vũ khí là object con của Player
    }

    // WeaponInventory gọi ngay sau khi tạo vũ khí
    public void Initialize(WeaponData weaponData, int startLevel = 1)
    {
        data = weaponData;
        level = startLevel;
        timer = 0f;
        OnLevelChanged();
    }

    protected virtual void Update()
    {
        if (data == null) return;
        timer += Time.deltaTime;

        float cooldown = CurrentCooldown;
        if (timer < cooldown) return;

        // Bắn được thì đặt lại đồng hồ. Không có mục tiêu thì thử lại sau tối đa 0.2s,
        // nhưng không chờ lâu hơn chính hồi chiêu (nên khi bắn nhanh vẫn kịp phản ứng)
        timer = Fire() ? 0f : cooldown - Mathf.Min(0.2f, cooldown);
    }

    protected abstract bool Fire();

    public void LevelUp()
    {
        if (IsMaxLevel) return;
        level++;
        OnLevelChanged();
    }

    // Lớp con ghi đè nếu cần cập nhật khi đổi cấp (ví dụ phóng to Aura)
    protected virtual void OnLevelChanged() { }

    protected float CalculateDamage()
    {
        return DamageCalculator.Calculate(data.baseDamage, owner.Atk, owner.GetDamageBonus(data.element));
    }
}