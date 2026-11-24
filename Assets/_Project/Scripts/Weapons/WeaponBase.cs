using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponData data;
    protected int level = 1;
    protected PlayerStats owner;
    private float timer;
    private const float MinCooldown = 0.1f;   // Sàn: không bao giờ bắn nhanh hơn 10 lần/giây, bảo vệ hiệu năng
    protected float AreaScale => owner != null ? owner.GetAreaMultiplier(data.element) : 1f;        // Hệ số bán kính từ nội tại nhân vật (Claire 5 sao: +100% bán kính hỏa khi All Out Assault)

    public WeaponData Data => data;
    public int Level => level;
    public bool IsMaxLevel => data != null && level >= data.levels.Length;

    // WeaponInventory nghe để cập nhật passive của vũ khí này và phát OnWeaponsChanged
    public event System.Action<WeaponBase> Leveled;

    // Thông số của cấp hiện tại (level 1 -> phần tử 0)
    protected WeaponLevel CurrentLevel => data.levels[Mathf.Clamp(level, 1, data.levels.Length) - 1];

    // ---------------- Chỉ số theo cấp (13B) ----------------
    // Giá trị của cấp (> 0) thay cho giá trị chung trong WeaponData; bằng 0 thì dùng giá trị chung.
    private static float Inherit(float levelValue, float fallback) { return levelValue > 0f ? levelValue : fallback; }

    protected float TargetRange => Inherit(CurrentLevel.targetRange, data.targetRange);
    protected int Pierce => Mathf.Max(0, CurrentLevel.overridePierce ? CurrentLevel.pierce : data.pierce);
    protected float ProjectileSpeed => Inherit(CurrentLevel.projectileSpeed, data.projectileSpeed);
    protected float ProjectileLifetime => Inherit(CurrentLevel.projectileLifetime, data.projectileLifetime);
    protected float BaseBurstInterval => Inherit(CurrentLevel.burstInterval, data.burstInterval);
    protected float ChainRange => Inherit(CurrentLevel.chainRange, data.chainRange);
    protected float OrbitSpeed => Inherit(CurrentLevel.orbitSpeed, data.orbitSpeed);
    protected float OrbitRadius => Inherit(CurrentLevel.orbitRadius, data.orbitRadius);
    protected float HitInterval => Inherit(CurrentLevel.hitInterval, data.hitInterval);
    protected float BeamLength => Inherit(CurrentLevel.beamLength, data.beamLength);
    protected float BaseTickInterval => Inherit(CurrentLevel.tickInterval, data.tickInterval);

    // Sát thương cơ bản của cấp hiện tại (không có thì dùng giá trị chung của vũ khí)
    protected float CurrentBaseDamage
    {
        get
        {
            float d = CurrentLevel.baseDamage;
            return d > 0f ? d : data.baseDamage;
        }
    }

    // Hồi chiêu gốc của cấp hiện tại (0 thì dùng giá trị chung của vũ khí)
    protected float CurrentBaseCooldown
    {
        get
        {
            float c = CurrentLevel.cooldown;
            return c > 0f ? c : data.cooldown;
        }
    }

    // Hồi chiêu thực tế = hồi chiêu gốc x hệ số hiện tại (bình thường = 1, trong All Out Assault < 1)
    // CurrentCooldown: chia cho tốc đánh (Tê cóng 50% => hồi chiêu gấp đôi)
    private float CurrentCooldown
    {
        get
        {
            float m = owner != null ? owner.CooldownMultiplier : 1f;
            float speed = owner != null ? owner.AttackSpeedMultiplier : 1f;
            return Mathf.Max(MinCooldown, CurrentBaseCooldown * m / speed);
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
        if (owner != null && owner.WeaponsDisabled) return;      // Choáng: không khai hỏa, hồi chiêu đóng băng
        timer += Time.deltaTime;

        float cooldown = CurrentCooldown;
        if (timer < cooldown) return;

        // Bắn được thì đặt lại đồng hồ. Không có mục tiêu thì thử lại sau tối đa 0.2s,
        // nhưng không chờ lâu hơn chính hồi chiêu (nên khi bắn nhanh vẫn kịp phản ứng)
        // timer = Fire() ? 0f : cooldown - Mathf.Min(0.2f, cooldown);
        bool fired = Fire();
        timer = fired ? 0f : cooldown - Mathf.Min(0.2f, cooldown);
        if (fired) AudioManager.Play(data.fireCue);
    }

    protected abstract bool Fire();

    public void LevelUp()
    {
        if (IsMaxLevel) return;
        level++;
        OnLevelChanged();
        Leveled?.Invoke(this);
    }

    // Lớp con ghi đè nếu cần cập nhật khi đổi cấp (ví dụ phóng to Aura)
    protected virtual void OnLevelChanged() { }

    protected float CalculateDamage()
    {
        // Liên kết linh hồn: ATK cộng thêm được chuyển vào sát thương cơ bản của vũ khí.
        // Không có passive này thì WeaponAtkMultiplier = 1 và WeaponFlatDamage = 0 nên công thức giữ nguyên như cũ.
        float baseDamage = (CurrentBaseDamage + owner.WeaponFlatDamage) * owner.WeaponAtkMultiplier;
        return DamageCalculator.Calculate(baseDamage, owner.Atk, owner.GetDamageBonus(data.element));
    }

    // Mọi vũ khí gây sát thương lên quái qua đây: sát thương có nguyên tố (DEF/kháng của quái) + hiệu ứng theo cấp
    protected void HitEnemy(Component target, float damage)
    {
        if (owner != null && owner.IsDormant) return;
        List<WeaponHitEffect> effects = CurrentLevel.hitEffects;
        bool hasEffects = effects != null && effects.Count > 0;
        float atk = (hasEffects && owner != null) ? owner.Atk * owner.WeaponAtkMultiplier : 1f;      // Chỉ tính ATK khi thật sự cần
        CombatHit.HitEnemy(target, damage, data.element, effects, atk);
    }
}