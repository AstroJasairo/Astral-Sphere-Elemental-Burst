using UnityEngine;

// Vùng chơi hình chữ nhật (hình vuông là trường hợp đặc biệt)
public class RectMapArea : IMapArea
{
    private readonly Vector2 min;
    private readonly Vector2 max;

    public Vector2 Center { get; }
    public Vector2 Size { get; }

    public RectMapArea(Vector2 center, Vector2 size)
    {
        Center = center;
        Size = size;
        min = center - size * 0.5f;
        max = center + size * 0.5f;
    }

    public bool Contains(Vector2 p, float margin = 0f)
    {
        return p.x >= min.x + margin && p.x <= max.x - margin
            && p.y >= min.y + margin && p.y <= max.y - margin;
    }

    public Vector2 Clamp(Vector2 p, float margin = 0f)
    {
        return new Vector2(
            Mathf.Clamp(p.x, min.x + margin, max.x - margin),
            Mathf.Clamp(p.y, min.y + margin, max.y - margin));
    }

    public Vector2 RandomPoint(float margin = 0f)
    {
        return new Vector2(
            Random.Range(min.x + margin, max.x - margin),
            Random.Range(min.y + margin, max.y - margin));
    }

    // Bốn góc, ngược chiều kim đồng hồ
    public Vector2[] GetOutline()
    {
        return new[]
        {
            new Vector2(min.x, min.y), new Vector2(max.x, min.y),
            new Vector2(max.x, max.y), new Vector2(min.x, max.y)
        };
    }
}