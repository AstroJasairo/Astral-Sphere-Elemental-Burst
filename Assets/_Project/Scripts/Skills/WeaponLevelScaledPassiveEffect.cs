// Lớp cha cho passive có công thức: giá trị = (hệ số theo cấp skill) x (TỔNG CẤP của mọi vũ khí đang mang).
// Dùng cho Hộ giá và Lưỡi gươm của nữ hoàng.
// Ví dụ: skill Lv3 (1.5%), Lửa Lv4 + Lôi Lv1 => 1.5% x (4 + 1) = 7.5%.
public abstract class WeaponLevelScaledPassiveEffect : PassiveSkillEffect
{
    private WeaponInventory inventory;
    private bool enabled;
    protected float perLevel;                                  // Hệ số cho MỖI cấp vũ khí (0.015 = 1.5%)

    protected WeaponLevelScaledPassiveEffect(PlayerStats stats) : base(stats) { }

    protected abstract void ApplyValue(float value);           // Đăng ký nguồn với giá trị này
    protected abstract void RemoveValue();                     // Gỡ sạch nguồn

    public override void Enable()
    {
        enabled = true;
        inventory = stats.Weapons;

        if (inventory != null)
        {
            inventory.OnWeaponsChanged -= Refresh;             // Gỡ trước rồi đăng ký: gọi Enable nhiều lần vẫn không bị trùng
            inventory.OnWeaponsChanged += Refresh;
        }
        Refresh();
    }

    public override void Disable()
    {
        enabled = false;
        if (inventory != null) inventory.OnWeaponsChanged -= Refresh;
        RemoveValue();
    }

    private void Refresh()
    {
        if (!enabled || inventory == null) return;
        ApplyValue(perLevel * inventory.TotalWeaponLevels);    // Đặt lại giá trị: Set cùng khóa nguồn là cập nhật, không cộng dồn
    }
}