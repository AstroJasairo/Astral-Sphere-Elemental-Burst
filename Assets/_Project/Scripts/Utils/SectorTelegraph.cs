using System;
using UnityEngine;

// Vòng tròn (angleDeg = 360) hoặc hình quạt. Vòng trong lớn dần từ tâm (expand) hoặc nhỏ dần vào tâm (shrink).
// Khi vòng trong chạm đích thì kích hoạt.
public class SectorTelegraph : Telegraph
{
    private float radius;
    private float angle;
    private bool shrink;

    // Hướng của quạt = hướng xoay của object (đặt bởi TelegraphSpawner)
    public void Begin(float radius, float angleDeg, float duration, bool shrink, float damage,
                      Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        this.radius = radius;
        this.angle = angleDeg;
        this.shrink = shrink;
        StartCommon(duration, damage, onTriggered, follow);

        // Viền ngoài cố định: vẽ một lần
        int n = TelegraphGeometry.Sector(points, radius, angleDeg);
        outer.SetPolygon(points, n, Vector2.zero);

        Tick(0f);   // Vẽ luôn vòng trong ở frame đầu để không bị nháy
    }

    protected override void Tick(float t)
    {
        // Lớn dần: 0 -> radius. Nhỏ dần: radius -> 0.
        float r = shrink ? Mathf.Lerp(radius, 0f, t) : Mathf.Lerp(0f, radius, t);

        bool visible = r > 0.02f;      // Quá nhỏ thì ẩn, tránh chấm viền lạ ở tâm
        inner.Show(visible);
        if (!visible) return;

        int n = TelegraphGeometry.Sector(points, r, angle);
        inner.SetPolygon(points, n, Vector2.zero);
    }

    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InSector(ToLocal(worldPosition), radius, angle, playerMargin);
    }
}