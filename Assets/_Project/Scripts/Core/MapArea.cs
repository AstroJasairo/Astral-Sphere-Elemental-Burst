using UnityEngine;

// Cổng truy cập vùng chơi hiện tại. Không có vùng chơi (Current = null) thì mọi hàm cho qua, như trước đây.
public static class MapArea
{
    public static IMapArea Current { get; set; }
    public static bool HasArea => Current != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Current = null; }

    public static bool Contains(Vector2 point, float margin = 0f)
    {
        return Current == null || Current.Contains(point, margin);
    }

    public static Vector2 Clamp(Vector2 point, float margin = 0f)
    {
        return Current == null ? point : Current.Clamp(point, margin);
    }

    // Giới hạn vận tốc để bước vật lý kế tiếp không vượt biên: vượt thì thu về mép (trượt dọc tường)
    public static Vector2 ConstrainVelocity(Vector2 position, Vector2 velocity, float deltaTime, float margin = 0f)
    {
        if (Current == null || deltaTime <= 0f) return velocity;

        Vector2 next = position + velocity * deltaTime;
        Vector2 clamped = Current.Clamp(next, margin);
        if (clamped == next) return velocity;

        return (clamped - position) / deltaTime;
    }
}