using UnityEngine;

// Vũ khí dạng "phóng đạn về mục tiêu gần nhất" (Kim loại; sau này tái dùng cho Nước, Bóng tối)
public class ProjectileWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;       // Chọn layer Enemy
    [SerializeField] private float spreadAngle = 12f;   // Góc tỏa giữa các viên đạn

    protected override bool Fire()
    {
        Transform target = TargetFinder.FindNearest(transform.position, data.targetRange, enemyMask);
        if (target == null) return false;               // Không có kẻ địch trong tầm

        Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        int count = CurrentLevel.projectileCount;
        float damage = CalculateDamage();

        for (int i = 0; i < count; i++)
        {
            // Tỏa đều quanh hướng ngắm: ví dụ 3 dao -> lệch -12°, 0°, +12°
            float angleOffset = (i - (count - 1) * 0.5f) * spreadAngle;
            Vector2 dir = Quaternion.Euler(0f, 0f, angleOffset) * baseDir;

            GameObject go = PoolManager.Instance.Get(data.projectilePrefab, transform.position, Quaternion.identity);
            go.GetComponent<Projectile>().Init(damage, dir, data.projectileSpeed, data.projectileLifetime, data.pierce);
        }
        return true;
    }
}