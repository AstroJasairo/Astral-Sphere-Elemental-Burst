using UnityEngine;

// Vũ khí dạng "phóng đạn về mục tiêu gần nhất".
//   burstInterval = 0 (Kim loại, Bóng tối): các viên bắn CÙNG LÚC và tỏa quạt.
//   burstInterval > 0 (Nước): bắn LẦN LƯỢT từng viên theo loạt, mỗi viên ngắm lại mục tiêu.
public class ProjectileWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float spreadAngle = 12f;     // Chế độ tỏa: góc giữa các viên
    [SerializeField] private float burstJitter = 3f;      // Chế độ bắn loạt: mỗi viên lệch ngẫu nhiên tối đa bao nhiêu độ

    private int burstRemaining;                           // Số viên còn phải bắn trong loạt hiện tại
    private float burstTimer;

    // Khoảng cách giữa hai viên. Nhân hệ số hồi chiêu nên All Out Assault làm loạt bắn nhanh hơn.
    private float BurstInterval
    {
        get
        {
            float multiplier = owner != null ? owner.CooldownMultiplier : 1f;
            return Mathf.Max(0.03f, data.burstInterval * multiplier);
        }
    }

    protected override void Update()
    {
        base.Update();         // Đếm hồi chiêu và gọi Fire() khi tới lúc
        TickBurst();           // Bắn tiếp các viên còn lại của loạt (nếu có)
    }

    protected override bool Fire()
    {
        if (burstRemaining > 0) return false;             // Loạt trước chưa bắn xong

        Transform target = TargetFinder.FindNearest(transform.position, data.targetRange, enemyMask);
        if (target == null) return false;

        int count = CurrentLevel.projectileCount;

        if (data.burstInterval > 0f)
        {
            // Chế độ bắn loạt: viên đầu tiên bắn ngay, các viên còn lại bắn trong TickBurst
            ShootAt(target, 0f);
            burstRemaining = count - 1;
            burstTimer = BurstInterval;
            return true;
        }

        // Chế độ tỏa: bắn cùng lúc, trải đều quanh hướng ngắm (3 viên: -12°, 0°, +12°)
        Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float damage = CalculateDamage();
        for (int i = 0; i < count; i++)
        {
            float angleOffset = (i - (count - 1) * 0.5f) * spreadAngle;
            Vector2 dir = Quaternion.Euler(0f, 0f, angleOffset) * baseDir;
            Spawn(dir, damage);
        }
        return true;
    }

    private void TickBurst()
    {
        if (burstRemaining <= 0 || data == null) return;

        burstTimer -= Time.deltaTime;                      // Dừng khi game tạm dừng
        if (burstTimer > 0f) return;

        // Mỗi viên ngắm lại mục tiêu gần nhất tại thời điểm bắn
        Transform target = TargetFinder.FindNearest(transform.position, data.targetRange, enemyMask);
        if (target == null)
        {
            burstRemaining = 0;                            // Hết mục tiêu: dừng loạt
            return;
        }

        ShootAt(target, Random.Range(-burstJitter, burstJitter));
        burstRemaining--;
        burstTimer = BurstInterval;
    }

    private void ShootAt(Transform target, float jitterDegrees)
    {
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        if (jitterDegrees != 0f) dir = Quaternion.Euler(0f, 0f, jitterDegrees) * dir;
        Spawn(dir, CalculateDamage());                     // Tính sát thương mỗi viên để buff đang có được áp dụng ngay
    }

    private void Spawn(Vector2 dir, float damage)
    {
        GameObject go = PoolManager.Instance.Get(data.projectilePrefab, transform.position, Quaternion.identity);
        go.GetComponent<Projectile>().Init(
            damage, dir, data.projectileSpeed, data.projectileLifetime, data.pierce,
            CurrentLevel.explosionRadius, enemyMask);
    }
}