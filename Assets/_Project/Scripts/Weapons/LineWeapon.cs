using System;
using UnityEngine;

// Vũ khí dạng tia thẳng: Ánh sáng (chỉ sát thương, chớp ngay) và Plant (sát thương + trói, dùng telegraph)
public class LineWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject flashPrefab;            // Hiệu ứng chớp (Ánh sáng). Tùy chọn

    [Header("Plant: telegraph chữ nhật ngắn quét từ cạnh gần nhân vật (thay cho hiệu ứng chớp)")]
    [SerializeField] private RectTelegraph telegraphPrefab;
    [SerializeField] private float telegraphDuration = 0.35f;

    private Action<Telegraph> telegraphCallback;

    protected override void Awake()
    {
        base.Awake();
        telegraphCallback = OnTelegraphTriggered;
    }

    protected override bool Fire()
    {
        Vector2 origin = transform.position;

        Transform target = TargetFinder.FindNearest(origin, data.targetRange, enemyMask);
        if (target == null) return false;

        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float length = data.beamLength;
        float width = CurrentLevel.beamWidth;

        if (telegraphPrefab != null)
        {
            // Chữ nhật hiện ra và vùng trong quét từ cạnh gần nhân vật tới cạnh xa; xong mới gây sát thương và trói
            GameObject go = PoolManager.Instance.Get(telegraphPrefab.gameObject, origin, Quaternion.Euler(0f, 0f, angle));
            go.GetComponent<RectTelegraph>().Begin(length, width, telegraphDuration, RectFill.SweepForward,
                                                   0f, telegraphCallback, transform);
            return true;
        }

        // Không có telegraph: đánh ngay và chớp
        ApplyHit(origin, dir, length, width);
        if (flashPrefab != null)
        {
            GameObject fx = PoolManager.Instance.Get(flashPrefab, origin, Quaternion.Euler(0f, 0f, angle));
            fx.GetComponent<ShapeFlashEffect>().PlayBox(length, width);
        }
        return true;
    }

    // Telegraph quét xong: tính trúng đòn
    private void OnTelegraphTriggered(Telegraph t)
    {
        var rect = (RectTelegraph)t;
        ApplyHit(t.transform.position, t.transform.right, rect.Length, rect.Width);
    }

    // Gây sát thương (và trói nếu có) cho mọi kẻ địch nằm trong hình chữ nhật
    private void ApplyHit(Vector2 origin, Vector2 dir, float length, float width)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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
    }
}