using System;
using UnityEngine;

public enum RectFill
{
    SweepForward,       // Dạng 2: vạch cam quét từ cạnh gần kẻ thi triển tới cạnh xa
    ExpandSideways,     // Dạng 4: vùng trong mở rộng từ giữa ra hai bên
    SweepBackward,      // MỚI: quét từ cạnh xa về cạnh gần
    SweepLeftToRight,   // MỚI: quét ngang, từ cạnh trái sang cạnh phải (nhìn theo hướng của hình)
    SweepRightToLeft    // MỚI: quét ngang, từ cạnh phải sang cạnh trái
}

// Hình chữ nhật xuất phát từ vị trí object, kéo dài về phía +X
public class RectTelegraph : Telegraph
{
    private float length;
    public float Length => length;
    private float width;
    public float Width => width;
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
        float halfW = width * 0.5f;
        float x0 = 0f, x1 = length;            // Phạm vi theo chiều dài (trục +X: hướng tiến)
        float y0 = -halfW, y1 = halfW;         // Phạm vi theo chiều rộng (+Y là bên TRÁI khi nhìn theo hướng tiến)

        switch (fill)
        {
            case RectFill.SweepForward: x1 = length * t; break;                 // Dài ra dần về phía trước
            case RectFill.SweepBackward: x0 = length * (1f - t); break;         // Dài ra dần từ cạnh xa về phía gần
            case RectFill.ExpandSideways: y0 = -halfW * t; y1 = halfW * t; break;
            case RectFill.SweepLeftToRight: y0 = halfW - width * t; break;      // Bắt đầu ở mép trái (+Y), đi xuống mép phải
            case RectFill.SweepRightToLeft: y1 = -halfW + width * t; break;     // Bắt đầu ở mép phải (-Y), đi lên mép trái
        }

        bool visible = (x1 - x0) > 0.02f && (y1 - y0) > 0.02f;
        inner.Show(visible);
        if (!visible) return;

        int n = TelegraphGeometry.Box(points, x0, x1, y0, y1);
        inner.SetPolygon(points, n, TelegraphGeometry.Centroid(points, n));
    }

    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InBox(ToLocal(worldPosition), 0f, length, width * 0.5f, playerMargin);
    }
}