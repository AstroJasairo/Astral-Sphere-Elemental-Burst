using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponData data;
    protected int level = 1;
    protected PlayerStats owner;
    private float timer;

    public WeaponData Data => data;
    public int Level => level;
    public bool IsMaxLevel => data != null && level >= data.levels.Length;

    // Thông số của cấp hiện tại (level 1 -> phần tử 0)
    protected WeaponLevel CurrentLevel => data.levels[Mathf.Clamp(level, 1, data.levels.Length) - 1];

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

    private void Update()
    {
        if (data == null) return;   // Chưa được khởi tạo
        timer += Time.deltaTime;
        if (timer < data.cooldown) return;

        // Fire() trả về true nếu tấn công được. Không có mục tiêu thì thử lại sau 0.2s
        timer = Fire() ? 0f : data.cooldown - 0.2f;
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