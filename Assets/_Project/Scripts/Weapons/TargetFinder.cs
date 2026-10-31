using UnityEngine;

// Tìm kẻ địch gần nhất. Dùng bộ đệm tĩnh nên KHÔNG sinh rác mỗi lần gọi.
public static class TargetFinder
{
    private static readonly Collider2D[] buffer = new Collider2D[64];
    private static ContactFilter2D filter;
    public static Collider2D[] Results => buffer;

    public static Transform FindNearest(Vector2 origin, float range, LayerMask mask)
    {
        filter.SetLayerMask(mask);
        filter.useTriggers = true;   // QUAN TRỌNG: collider quái là Trigger, mặc định bộ lọc sẽ bỏ qua chúng

        int count = Physics2D.OverlapCircle(origin, range, filter, buffer);

        Transform nearest = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            // So sánh bình phương khoảng cách để tránh phép căn bậc hai (nhanh hơn)
            float sqr = ((Vector2)buffer[i].transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearest = buffer[i].transform;
            }
        }
        return nearest;
    }

    // Tìm mọi collider trong một hình chữ nhật xoay góc angleDeg. Kết quả nằm trong Results.
    public static int FindBox(Vector2 center, Vector2 size, float angleDeg, LayerMask mask)
    {
        filter.SetLayerMask(mask);
        filter.useTriggers = true;           // Collider quái là Trigger
        return Physics2D.OverlapBox(center, size, angleDeg, filter, buffer);
    }

    // Trả về số collider tìm được, kết quả nằm trong Results
    public static int FindAll(Vector2 origin, float radius, LayerMask mask)
    {
        filter.SetLayerMask(mask);
        filter.useTriggers = true;   // Collider quái là Trigger
        return Physics2D.OverlapCircle(origin, radius, filter, buffer);
    }
}