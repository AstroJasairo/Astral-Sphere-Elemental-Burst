using UnityEngine;

// Vũ khí đặc trưng của Claire: định kỳ thả một thanh kiếm xuống nơi đông kẻ địch nhất
public class ClariusWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private ClariusSword swordPrefab;
    [SerializeField] private GameObject impactFlashPrefab;      // Hiệu ứng khi kiếm cắm xuống (ShapeFlashEffect). Tùy chọn

    // Bản sao vị trí kẻ địch. Phải chép ra vì TargetFinder.Results dùng chung và sẽ bị ghi đè
    private readonly Vector2[] positions = new Vector2[128];

    // Các giá trị được kiếm đọc LẠI MỖI LẦN cần, nên thay đổi giữa trận (All Out Assault) có hiệu lực ngay
    public float CurrentRadius => CurrentLevel.radius * AreaScale;

    public float CurrentTickInterval
    {
        get
        {
            float multiplier = owner != null ? owner.CooldownMultiplier : 1f;     // All Out Assault: giảm n
            return Mathf.Max(0.08f, data.tickInterval * multiplier);
        }
    }

    protected override bool Fire()
    {
        Vector2 origin = transform.position;

        int count = TargetFinder.FindAll(origin, data.targetRange, enemyMask);
        if (count == 0) return false;

        int n = Mathf.Min(count, positions.Length);
        for (int i = 0; i < n; i++)
            positions[i] = TargetFinder.Results[i].transform.position;

        Vector2 spot = FindDensestSpot(n, CurrentRadius);

        GameObject go = PoolManager.Instance.Get(swordPrefab.gameObject, spot, Quaternion.identity);
        go.GetComponent<ClariusSword>().Begin(this, CurrentLevel.duration);
        return true;
    }

    // Tìm điểm có nhiều kẻ địch nhất trong bán kính "radius" (so sánh từng cặp, tối đa 128 x 128, chỉ chạy mỗi lần thả kiếm)
    private Vector2 FindDensestSpot(int n, float radius)
    {
        float radiusSqr = radius * radius;
        int best = 0;
        int bestCount = -1;

        for (int i = 0; i < n; i++)
        {
            int neighbours = 0;
            for (int j = 0; j < n; j++)
                if ((positions[i] - positions[j]).sqrMagnitude <= radiusSqr) neighbours++;

            if (neighbours > bestCount)
            {
                bestCount = neighbours;
                best = i;
            }
        }

        // Tinh chỉnh: dời tâm về trọng tâm các kẻ địch quanh điểm tốt nhất để phủ đều hơn
        Vector2 sum = Vector2.zero;
        int members = 0;
        for (int j = 0; j < n; j++)
        {
            if ((positions[best] - positions[j]).sqrMagnitude > radiusSqr) continue;
            sum += positions[j];
            members++;
        }
        return sum / members;
    }

    // Thanh kiếm gọi mỗi lần đến nhịp gây sát thương
    public void DamageArea(Vector2 center)
    {
        int count = TargetFinder.FindAll(center, CurrentRadius, enemyMask);
        if (count == 0) return;

        float damage = CalculateDamage();              // Tính tại thời điểm đánh nên buff ATK, nội tại, All Out Assault đều được áp dụng
        for (int i = 0; i < count; i++)
        {
            if (TargetFinder.Results[i].TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }
    }

    // Thanh kiếm gọi lúc cắm xuống đất
    public void PlayImpact(Vector2 position)
    {
        if (impactFlashPrefab == null) return;

        GameObject fx = PoolManager.Instance.Get(impactFlashPrefab, position, Quaternion.identity);
        fx.GetComponent<ShapeFlashEffect>().PlaySector(CurrentRadius, 360f);
    }
}