using System;
using UnityEngine;

public enum SweepDirection
{
    CounterClockwise,   // Bắt đầu từ cạnh trái miếng an toàn, quét ngược chiều kim đồng hồ
    Clockwise           // Bắt đầu từ cạnh phải miếng an toàn, quét cùng chiều kim đồng hồ
}

// Đòn "pizza": hình tròn trừ miếng an toàn. Vùng cam quét qua toàn bộ vùng nguy hiểm,
// tới cạnh còn lại thì gây sát thương. Hướng object (facing) = hướng tâm miếng an toàn.
public class SweepTelegraph : Telegraph
{
    private float radius;
    private float safeAngle;       // Góc miếng an toàn (ví dụ 60)
    private float dangerSpan;      // Góc vùng nguy hiểm = 360 - safeAngle (ví dụ 300)
    private SweepDirection direction;

    public void Begin(float radius, float safeAngleDeg, SweepDirection direction, float duration,
                      float damage, Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        this.radius = radius;
        this.safeAngle = Mathf.Clamp(safeAngleDeg, 1f, 359f);
        this.dangerSpan = 360f - safeAngle;
        this.direction = direction;
        StartCommon(duration, damage, onTriggered, follow);

        // Viền ngoài = toàn bộ vùng nguy hiểm: bắt đầu từ cạnh +safe/2 và mở ra ngược chiều kim đồng hồ
        // một góc dangerSpan, tức là chạy hết một vòng trừ miếng an toàn.
        int n = TelegraphGeometry.SectorFromAngle(points, radius, safeAngle * 0.5f, dangerSpan);
        outer.SetPolygon(points, n, Vector2.zero);

        Tick(0f);
    }

    protected override void Tick(float t)
    {
        float span = dangerSpan * t;               // Góc đã quét được
        bool visible = span > 0.5f;
        inner.Show(visible);
        if (!visible) return;

        // Ngược chiều kim đồng hồ: vùng cam bắt đầu ở cạnh +safe/2 và mở ra theo chiều tăng góc.
        // Cùng chiều kim đồng hồ: vùng cam bắt đầu ở cạnh -safe/2 và mở ra theo chiều giảm góc,
        // nên góc bắt đầu của hình quạt là (-safe/2 - span) rồi kéo dài tới -safe/2.
        float start = direction == SweepDirection.CounterClockwise
            ? safeAngle * 0.5f
            : -safeAngle * 0.5f - span;

        int n = TelegraphGeometry.SectorFromAngle(points, radius, start, span);
        inner.SetPolygon(points, n, Vector2.zero);
    }

    // "Inside" ở đây nghĩa là đang ở trong vùng nguy hiểm (không nằm trong miếng an toàn)
    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InSweepDanger(ToLocal(worldPosition), radius, safeAngle, playerMargin);
    }
}