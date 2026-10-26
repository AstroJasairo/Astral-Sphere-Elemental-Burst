using System;
using UnityEngine;

public enum RectFill
{
    SweepForward,     // Dạng 2: vạch cam quét từ cạnh gần kẻ thi triển tới cạnh xa
    ExpandSideways    // Dạng 4: vùng trong mở rộng từ giữa ra hai bên, chạm hai cạnh bên thì kích hoạt
}

// Hình chữ nhật xuất phát từ vị trí object, kéo dài về phía +X
public class RectTelegraph : Telegraph
{
    private float length;
    private float width;
    private RectFill fill;

    public void Begin(float length, float width, float duration, RectFill fill, float damage,
                      Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        this.length = length;
        this.width = width;
        this.fill = fill;
        StartCommon(duration, damage, onTriggered, follow);

        // Viền ngoài: từ cạnh gần (x = 0) tới cạnh xa (x = length), rộng "width" đối xứng hai bên
        int n = TelegraphGeometry.Box(points, 0f, length, -width * 0.5f, width * 0.5f);
        outer.SetPolygon(points, n, TelegraphGeometry.Centroid(points, n));

        Tick(0f);
    }

    protected override void Tick(float t)
    {
        float x1 = length;
        float halfW = width * 0.5f;

        if (fill == RectFill.SweepForward) x1 = length * t;   // Vùng cam dài ra dần về phía trước
        else halfW *= t;                                      // Vùng cam rộng ra dần sang hai bên

        bool visible = x1 > 0.02f && halfW > 0.02f;
        inner.Show(visible);
        if (!visible) return;

        int n = TelegraphGeometry.Box(points, 0f, x1, -halfW, halfW);
        inner.SetPolygon(points, n, TelegraphGeometry.Centroid(points, n));
    }

    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InBox(ToLocal(worldPosition), 0f, length, width * 0.5f, playerMargin);
    }
}