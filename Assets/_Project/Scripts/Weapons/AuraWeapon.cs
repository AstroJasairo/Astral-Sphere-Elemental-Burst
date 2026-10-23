using UnityEngine;

// Vũ khí dạng "vùng quanh người": Lửa (sau này tái dùng cho Đất)
// WeaponBase gọi Fire() theo cooldown, nên cooldown chính là chu kỳ gây sát thương liên tục.
public class AuraWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;   // Chọn layer Enemy
    [SerializeField] private Transform visual;      // Hình tròn minh họa Aura (con của object này)

    // Mỗi lần đổi cấp thì phóng to hình minh họa theo bán kính mới
    protected override void OnLevelChanged()
    {
        if (visual == null) return;
        float diameter = CurrentLevel.radius * 2f;   // Sprite Circle mặc định có đường kính 1 đơn vị
        visual.localScale = new Vector3(diameter, diameter, 1f);
    }

    protected override bool Fire()
    {
        int count = TargetFinder.FindAll(transform.position, CurrentLevel.radius, enemyMask);
        float damage = CalculateDamage();

        // Gây sát thương cho mọi kẻ địch trong vòng tròn
        for (int i = 0; i < count; i++)
        {
            if (TargetFinder.Results[i].TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }
        return true;   // Aura luôn "bắn" dù không có ai trong vùng
    }
}