using UnityEngine;

// Vùng chơi của một map. Bây giờ chỉ có hình chữ nhật (RectMapArea).
// Sau này mê cung/hình đa giác chỉ cần cài lại giao diện này, spawner/drop/boss không phải sửa.
public interface IMapArea
{
    Vector2 Center { get; }
    Vector2 Size { get; }

    // margin: lề thụt vào trong (điểm cách mép chưa tới margin thì coi như ngoài)
    bool Contains(Vector2 point, float margin = 0f);

    // Điểm gần nhất nằm trong vùng chơi
    Vector2 Clamp(Vector2 point, float margin = 0f);

    // Một điểm ngẫu nhiên trong vùng chơi
    Vector2 RandomPoint(float margin = 0f);

    // Các đỉnh viền ngoài theo thứ tự vòng (dùng cho đường viền và khung camera)
    Vector2[] GetOutline();
}