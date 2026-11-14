using System;
using UnityEngine;

public enum AreaShape
{
    Circle,     // Đất: gây sát thương mọi kẻ địch quanh người chơi (đánh ngay, chớp)
    Cone,       // Phong: hình nón hướng về kẻ địch gần nhất, kèm đẩy lùi
    Ring,       // MỚI: nhẫn quanh người chơi (không ngắm), lan từ vòng trong ra hoặc ngược lại
    ConeRing,   // MỚI: nón nhẫn hướng về kẻ địch gần nhất
    Sweep,      // MỚI: quạt quét từ một cạnh bên sang cạnh còn lại, hướng về kẻ địch gần nhất
    SweepRing   // MỚI: sweep có lỗ giữa (quạt khuyết tâm)
}

// Vũ khí đánh vùng quanh người chơi: một lần gây sát thương mỗi hồi chiêu
public class AreaWeapon : WeaponBase
{
    [SerializeField] private AreaShape shape = AreaShape.Circle;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject flashPrefab;            // Hiệu ứng chớp (Đất, hoặc khi chưa có telegraph). Tùy chọn

    [Header("Telegraph (tùy chọn: thiếu prefab thì đánh ngay và chớp)")]
    [Tooltip("Tròn/Nón/Nhẫn: InToOut hoặc OutToIn. Sweep: Clockwise (LeftToRight) hoặc CounterClockwise (RightToLeft)")]
    [SerializeField] private TelegraphDirection direction = TelegraphDirection.InToOut;
    [Tooltip("Bật: mỗi lần bắn Sweep/SweepRing đổi phía quét (trái sang phải, rồi phải sang trái, rồi lại trái sang phải...)")]
    [SerializeField] private bool alternateSweepSide = false;
    private bool sweepFlipped;     // false: dùng đúng hướng đã chọn; true: đảo ngược
    [SerializeField] private float telegraphDuration = 0.35f;   // Thời gian vùng cam chạy hết
    [SerializeField] private SectorTelegraph coneTelegraphPrefab;       // Cone
    [SerializeField] private RingTelegraph ringTelegraphPrefab;         // Ring, ConeRing
    [SerializeField] private SweepRingTelegraph sweepTelegraphPrefab;   // Sweep, SweepRing

    private Action<Telegraph> triggeredCallback;                // Lưu sẵn để không tạo delegate mới mỗi lần bắn

    protected override void Awake()
    {
        base.Awake();
        triggeredCallback = OnTelegraphTriggered;
    }

    protected override bool Fire()
    {
        Vector2 origin = transform.position;
        WeaponLevel level = CurrentLevel;
        float radius = level.radius * AreaScale;

        if (shape == AreaShape.Circle) return FireCircle(origin, radius, level);
        return FireShaped(origin, radius, level);
    }

    // Hình tròn (Đất): đánh ngay những kẻ địch trong bán kính
    private bool FireCircle(Vector2 origin, float radius, WeaponLevel level)
    {
        int count = TargetFinder.FindAll(origin, radius, enemyMask);
        if (count == 0) return false;                           // Không có ai trong vùng thì không đánh

        ApplyDamage(count, origin, Vector2.right, 360f, level.knockbackDistance, 0f);
        PlayFlash(origin, 0f, 0f, radius, 360f);
        return true;
    }

    // Các dạng còn lại: nón, nhẫn, nón nhẫn, sweep, sweep nhẫn
    private bool FireShaped(Vector2 origin, float radius, WeaponLevel level)
    {
        // Nhẫn đầy đủ không cần hướng ngắm. Các dạng khác hướng về kẻ địch gần nhất
        Vector2 aim = Vector2.right;
        float facing = 0f;

        if (shape == AreaShape.Ring)
        {
            if (TargetFinder.FindAll(origin, radius, enemyMask) == 0) return false;
        }
        else
        {
            Transform target = TargetFinder.FindNearest(origin, data.targetRange, enemyMask);
            if (target == null) return false;

            aim = ((Vector2)target.position - origin).normalized;
            facing = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        }

        // Cone và Sweep không có lỗ giữa; các dạng nhẫn lấy innerRadius của cấp (co giãn theo bán kính như outer)
        bool hasHole = shape == AreaShape.Ring || shape == AreaShape.ConeRing || shape == AreaShape.SweepRing;
        float inner = hasHole ? level.innerRadius * AreaScale : 0f;
        float angle = shape == AreaShape.Ring ? 360f : level.coneAngle;   // Với Sweep: coneAngle là GÓC VÙNG NGUY HIỂM

        Quaternion rotation = Quaternion.Euler(0f, 0f, facing);
        bool inward = TelegraphDirectionUtil.IsInward(direction);

        // Có prefab telegraph: hiện vùng chạy, sát thương gây ra khi chạy xong (OnTelegraphTriggered)
        // damage = 0: không đụng tới người chơi. follow = transform: vùng đi theo người chơi, hướng thì đã khóa lúc bắn.
        switch (shape)
        {
            case AreaShape.Cone:
                if (coneTelegraphPrefab != null)
                {
                    GameObject go = PoolManager.Instance.Get(coneTelegraphPrefab.gameObject, origin, rotation);
                    go.GetComponent<SectorTelegraph>().Begin(radius, angle, telegraphDuration, inward, 0f, triggeredCallback, transform);
                    return true;
                }
                break;

            case AreaShape.Ring:
            case AreaShape.ConeRing:
                if (ringTelegraphPrefab != null)
                {
                    GameObject go = PoolManager.Instance.Get(ringTelegraphPrefab.gameObject, origin, rotation);
                    go.GetComponent<RingTelegraph>().Begin(inner, radius, angle, telegraphDuration, inward, 0f, triggeredCallback, transform);
                    return true;
                }
                break;

            case AreaShape.Sweep:
            case AreaShape.SweepRing:
                if (sweepTelegraphPrefab != null)
                {
                    GameObject go = PoolManager.Instance.Get(sweepTelegraphPrefab.gameObject, origin, rotation);

                    // XOR (^): sweepFlipped = true thì đảo chiều đã chọn
                    bool clockwise = TelegraphDirectionUtil.IsClockwise(direction) ^ sweepFlipped;

                    go.GetComponent<SweepRingTelegraph>().Begin(inner, radius, angle, clockwise,
                        telegraphDuration, 0f, triggeredCallback, transform);

                    // Chỉ đảo khi đã bắn thật (lần thử không có mục tiêu thì Fire() đã return false từ trước)
                    if (alternateSweepSide) sweepFlipped = !sweepFlipped;
                    return true;
                }
                break;
        }

        // Không có prefab telegraph: đánh ngay và chớp
        int count = TargetFinder.FindAll(origin, radius, enemyMask);
        ApplyDamage(count, origin, aim, angle, level.knockbackDistance, inner);
        PlayFlash(origin, facing, inner, radius, angle);
        return true;
    }

    // Telegraph gọi khi vùng cam chạy xong: lúc này mới tính trúng đòn
    private void OnTelegraphTriggered(Telegraph t)
    {
        float inner = 0f, outer, angle;

        // Mỗi loại telegraph đều quy về ba số: bán kính trong, bán kính ngoài, góc mở
        switch (t)
        {
            case RingTelegraph ring:
                inner = ring.InnerRadius; outer = ring.OuterRadius; angle = ring.Angle;
                break;
            case SweepRingTelegraph sweep:
                inner = sweep.InnerRadius; outer = sweep.OuterRadius; angle = sweep.Span;
                break;
            case SectorTelegraph sector:
                outer = sector.Radius; angle = sector.Angle;
                break;
            default:
                return;
        }

        Vector2 origin = t.transform.position;
        Vector2 aim = t.transform.right;                        // Hướng đã khóa lúc bắn

        int count = TargetFinder.FindAll(origin, outer, enemyMask);
        ApplyDamage(count, origin, aim, angle, CurrentLevel.knockbackDistance, inner);
    }

    // Gây sát thương (và đẩy lùi) cho các collider đã tìm được, chỉ giữ những con nằm trong góc mở và ngoài lỗ giữa
    private void ApplyDamage(int count, Vector2 origin, Vector2 aim, float angle, float knockback, float innerRadius)
    {
        float damage = CalculateDamage();
        bool useAngle = angle < 359.9f;
        float halfAngle = angle * 0.5f;
        float innerSqr = innerRadius * innerRadius;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = TargetFinder.Results[i];
            Vector2 toEnemy = (Vector2)col.transform.position - origin;

            if (innerRadius > 0f && toEnemy.sqrMagnitude < innerSqr) continue;      // Nằm trong lỗ giữa
            if (useAngle && Vector2.Angle(aim, toEnemy) > halfAngle) continue;      // Ngoài góc mở

            // Đẩy lùi ra xa người chơi (BOSS miễn nhiễm do StatusEffectHandler tự bỏ qua)
            if (knockback > 0f && col.TryGetComponent(out StatusEffectHandler status))
                status.ApplyKnockback(toEnemy, knockback);

            //if (col.TryGetComponent(out HealthSystem hp))
            //    hp.TakeDamage(damage);
            HitEnemy(col, damage);
        }
    }

    private void PlayFlash(Vector2 origin, float facing, float innerRadius, float radius, float angle)
    {
        if (flashPrefab == null) return;

        GameObject fx = PoolManager.Instance.Get(flashPrefab, origin, Quaternion.Euler(0f, 0f, facing));
        ShapeFlashEffect flash = fx.GetComponent<ShapeFlashEffect>();

        if (innerRadius > 0f) flash.PlayRing(innerRadius, radius, angle);
        else flash.PlaySector(radius, angle);
    }
}