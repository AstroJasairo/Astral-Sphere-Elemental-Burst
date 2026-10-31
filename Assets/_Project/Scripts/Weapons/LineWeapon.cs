using UnityEngine;

public class LineWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject flashPrefab;         // Prefab có ShapeFlashEffect (tùy chọn)

    protected override bool Fire()
    {
        Vector2 origin = transform.position;

        Transform target = TargetFinder.FindNearest(origin, data.targetRange, enemyMask);
        if (target == null) return false;

        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float length = data.beamLength;
        float width = CurrentLevel.beamWidth;

        // Hình chữ nhật dài "length", rộng "width", bắt đầu từ người chơi: tâm nằm cách một nửa chiều dài
        int count = TargetFinder.FindBox(origin + dir * (length * 0.5f), new Vector2(length, width), angle, enemyMask);

        float damage = CalculateDamage();
        float rootDuration = CurrentLevel.rootDuration;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = TargetFinder.Results[i];

            // Trói trước, rồi mới gây sát thương (kẻ địch chết sẽ bị trả về pool)
            if (rootDuration > 0f && col.TryGetComponent(out StatusEffectHandler status))
                status.ApplyRoot(rootDuration);

            if (col.TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }

        if (flashPrefab != null)
        {
            GameObject fx = PoolManager.Instance.Get(flashPrefab, origin, Quaternion.Euler(0f, 0f, angle));
            fx.GetComponent<ShapeFlashEffect>().PlayBox(length, width);
        }
        return true;
    }
}