using System;
using UnityEngine;

public enum AreaShape
{
    Circle,    // Đất: gây sát thương mọi kẻ địch quanh người chơi
    Cone       // Phong: hình nón hướng về kẻ địch gần nhất, kèm đẩy lùi
}

// Vũ khí đánh vùng quanh người chơi: một lần gây sát thương mỗi hồi chiêu
public class AreaWeapon : WeaponBase
{
    [SerializeField] private AreaShape shape = AreaShape.Circle;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject flashPrefab;            // Hiệu ứng chớp (Đất). Tùy chọn

    [Header("Phong: telegraph quét hình nón từ tâm ra (thay cho hiệu ứng chớp)")]
    [SerializeField] private SectorTelegraph coneTelegraphPrefab;
    [SerializeField] private float telegraphDuration = 0.35f;   // Thời gian vùng trong quét từ tâm ra viền

    private Action<Telegraph> coneCallback;                     // Lưu sẵn để không tạo delegate mới mỗi lần bắn

    protected override void Awake()
    {
        base.Awake();
        coneCallback = OnConeTriggered;
    }

    protected override bool Fire()
    {
        Vector2 origin = transform.position;
        WeaponLevel level = CurrentLevel;
        float radius = level.radius * AreaScale;

        if (shape == AreaShape.Cone) return FireCone(origin, radius, level);

        // Hình tròn (Đất)
        int count = TargetFinder.FindAll(origin, radius, enemyMask);
        if (count == 0) return false;                           // Không có ai trong vùng thì không đánh

        ApplyDamage(count, origin, Vector2.right, 360f, level.knockbackDistance);
        PlayFlash(origin, 0f, radius, 360f);
        return true;
    }

    private bool FireCone(Vector2 origin, float radius, WeaponLevel level)
    {
        // Hình nón hướng về kẻ địch gần nhất
        Transform target = TargetFinder.FindNearest(origin, data.targetRange, enemyMask);
        if (target == null) return false;

        Vector2 aim = ((Vector2)target.position - origin).normalized;
        float facing = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

        // Có telegraph: hiện vùng quét, sát thương gây ra khi quét xong (trong OnConeTriggered)
        if (coneTelegraphPrefab != null)
        {
            GameObject go = PoolManager.Instance.Get(coneTelegraphPrefab.gameObject, origin, Quaternion.Euler(0f, 0f, facing));
            // shrink = false: vùng trong lớn dần từ tâm ra. damage = 0: không đụng tới người chơi.
            // follow = transform: vùng nón đi theo người chơi trong lúc quét, hướng thì đã khóa.
            go.GetComponent<SectorTelegraph>().Begin(radius, level.coneAngle, telegraphDuration, false, 0f, coneCallback, transform);
            return true;
        }

        // Không có telegraph: đánh ngay và chớp
        int count = TargetFinder.FindAll(origin, radius, enemyMask);
        ApplyDamage(count, origin, aim, level.coneAngle, level.knockbackDistance);
        PlayFlash(origin, facing, radius, level.coneAngle);
        return true;
    }

    // Telegraph gọi khi vùng trong chạm viền: lúc này mới tính trúng đòn
    private void OnConeTriggered(Telegraph t)
    {
        var sector = (SectorTelegraph)t;
        Vector2 origin = t.transform.position;
        Vector2 aim = t.transform.right;                        // Hướng đã khóa lúc bắn

        int count = TargetFinder.FindAll(origin, sector.Radius, enemyMask);
        ApplyDamage(count, origin, aim, sector.Angle, CurrentLevel.knockbackDistance);
    }

    // Gây sát thương (và đẩy lùi) cho các collider đã tìm được, chỉ giữ những con nằm trong góc mở
    private void ApplyDamage(int count, Vector2 origin, Vector2 aim, float angle, float knockback)
    {
        float damage = CalculateDamage();
        bool useAngle = angle < 359.9f;
        float halfAngle = angle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = TargetFinder.Results[i];
            Vector2 toEnemy = (Vector2)col.transform.position - origin;

            if (useAngle && Vector2.Angle(aim, toEnemy) > halfAngle) continue;

            // Đẩy lùi ra xa người chơi (BOSS miễn nhiễm do StatusEffectHandler tự bỏ qua)
            if (knockback > 0f && col.TryGetComponent(out StatusEffectHandler status))
                status.ApplyKnockback(toEnemy, knockback);

            if (col.TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }
    }

    private void PlayFlash(Vector2 origin, float facing, float radius, float angle)
    {
        if (flashPrefab == null) return;

        GameObject fx = PoolManager.Instance.Get(flashPrefab, origin, Quaternion.Euler(0f, 0f, facing));
        fx.GetComponent<ShapeFlashEffect>().PlaySector(radius, angle);
    }
}