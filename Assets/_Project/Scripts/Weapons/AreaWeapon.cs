using UnityEngine;

public enum AreaShape
{
    Circle,    // Đất: gây sát thương mọi kẻ địch quanh người chơi
    Cone       // Phong: hình nón hướng về kẻ địch gần nhất, kèm đẩy lùi
}

// Vũ khí đánh vùng tức thời quanh người chơi: một lần gây sát thương mỗi hồi chiêu
public class AreaWeapon : WeaponBase
{
    [SerializeField] private AreaShape shape = AreaShape.Circle;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject flashPrefab;       // Prefab có ShapeFlashEffect (tùy chọn)

    protected override bool Fire()
    {
        Vector2 origin = transform.position;
        WeaponLevel level = CurrentLevel;

        // Hình nón cần biết ngắm về đâu: hướng về kẻ địch gần nhất. Hình tròn thì không cần.
        Vector2 aimDir = Vector2.right;
        float facing = 0f;
        if (shape == AreaShape.Cone)
        {
            Transform target = TargetFinder.FindNearest(origin, data.targetRange, enemyMask);
            if (target == null) return false;

            aimDir = ((Vector2)target.position - origin).normalized;
            facing = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        }

        int count = TargetFinder.FindAll(origin, level.radius, enemyMask);
        if (shape == AreaShape.Circle && count == 0) return false;   // Không có ai trong vùng thì không đánh

        float damage = CalculateDamage();
        float halfAngle = level.coneAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = TargetFinder.Results[i];
            Vector2 toEnemy = (Vector2)col.transform.position - origin;

            // Hình nón: bỏ qua kẻ địch nằm ngoài góc mở
            if (shape == AreaShape.Cone && Vector2.Angle(aimDir, toEnemy) > halfAngle) continue;

            // Đẩy lùi ra xa người chơi (BOSS miễn nhiễm do StatusEffectHandler tự bỏ qua)
            if (level.knockbackDistance > 0f && col.TryGetComponent(out StatusEffectHandler status))
                status.ApplyKnockback(toEnemy, level.knockbackDistance);

            if (col.TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }

        PlayFlash(origin, facing, level);
        return true;
    }

    private void PlayFlash(Vector2 origin, float facing, WeaponLevel level)
    {
        if (flashPrefab == null) return;

        GameObject fx = PoolManager.Instance.Get(flashPrefab, origin, Quaternion.Euler(0f, 0f, facing));
        float angle = shape == AreaShape.Cone ? level.coneAngle : 360f;
        fx.GetComponent<ShapeFlashEffect>().PlaySector(level.radius, angle);
    }
}