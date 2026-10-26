using UnityEngine;

// Các hàm hình học thuần túy dùng cho Telegraph. Mọi hình nằm trong hệ tọa độ cục bộ, hướng về +X.
public static class TelegraphGeometry
{
    public const int ArcSegments = 40;                // Số đoạn thẳng xấp xỉ cung tròn
    public const int MaxPoints = ArcSegments + 2;     // Quạt: 1 đỉnh + (ArcSegments + 1) điểm trên cung

    // Hình tròn (angleDeg >= 360) hoặc hình quạt quanh tâm (0,0), hướng +X.
    // Ghi các điểm vào "pts", trả về số điểm đã ghi.
    public static int Sector(Vector2[] pts, float radius, float angleDeg)
    {
        int n = 0;

        if (angleDeg >= 359.9f)
        {
            // Hình tròn: các điểm trên vòng, KHÔNG lặp lại điểm đầu (viền sẽ tự khép kín)
            for (int i = 0; i < ArcSegments; i++)
            {
                float a = i * (2f * Mathf.PI / ArcSegments);
                pts[n++] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            return n;
        }

        float half = angleDeg * 0.5f * Mathf.Deg2Rad;
        pts[n++] = Vector2.zero;                      // Đỉnh quạt nằm ở tâm
        for (int i = 0; i <= ArcSegments; i++)
        {
            float a = -half + i * (2f * half / ArcSegments);
            pts[n++] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }
        return n;
    }

    // Hình chữ nhật từ (xMin, yMin) đến (xMax, yMax). Trả về 4 điểm.
    public static int Box(Vector2[] pts, float xMin, float xMax, float yMin, float yMax)
    {
        pts[0] = new Vector2(xMin, yMin);
        pts[1] = new Vector2(xMax, yMin);
        pts[2] = new Vector2(xMax, yMax);
        pts[3] = new Vector2(xMin, yMax);
        return 4;
    }

    // Thanh mảnh xuất phát từ tâm (0,0), dài "length", rộng "width", hướng theo góc angleDeg
    public static int Bar(Vector2[] pts, float length, float width, float angleDeg)
    {
        float a = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        Vector2 side = new Vector2(-dir.y, dir.x) * (width * 0.5f);   // Vector vuông góc với hướng
        pts[0] = -side;
        pts[1] = dir * length - side;
        pts[2] = dir * length + side;
        pts[3] = side;
        return 4;
    }

    // Trọng tâm của các điểm (dùng làm điểm gốc để tô màu hình chữ nhật)
    public static Vector2 Centroid(Vector2[] pts, int count)
    {
        Vector2 sum = Vector2.zero;
        for (int i = 0; i < count; i++) sum += pts[i];
        return sum / count;
    }

    // ----- Kiểm tra trúng đòn (point là tọa độ CỤC BỘ). margin = bán kính người chơi -----

    public static bool InSector(Vector2 local, float radius, float angleDeg, float margin)
    {
        float maxR = radius + margin;
        if (local.sqrMagnitude > maxR * maxR) return false;     // Ngoài bán kính
        if (angleDeg >= 359.9f) return true;                    // Hình tròn: chỉ cần trong bán kính

        // Góc lệch so với hướng +X phải nằm trong nửa góc mở
        return Mathf.Abs(Vector2.SignedAngle(Vector2.right, local)) <= angleDeg * 0.5f;
    }

    public static bool InBox(Vector2 local, float xMin, float xMax, float halfWidth, float margin)
    {
        return local.x >= xMin - margin && local.x <= xMax + margin
            && Mathf.Abs(local.y) <= halfWidth + margin;
    }

    // Hình quạt bắt đầu từ góc startDeg, mở rộng ngược chiều kim đồng hồ một góc spanDeg.
    // Đỉnh quạt nằm ở tâm (0,0). Khác với Sector(): quạt này KHÔNG bắt buộc đối xứng quanh +X.
    public static int SectorFromAngle(Vector2[] pts, float radius, float startDeg, float spanDeg)
    {
        int n = 0;
        pts[n++] = Vector2.zero;                              // Đỉnh quạt
        float start = startDeg * Mathf.Deg2Rad;
        float span = spanDeg * Mathf.Deg2Rad;
        for (int i = 0; i <= ArcSegments; i++)
        {
            float a = start + i * (span / ArcSegments);
            pts[n++] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }
        return n;
    }

    // Người chơi có đang ở trong vùng NGUY HIỂM của đòn Sweep không?
    // Vùng nguy hiểm = hình tròn bán kính "radius" trừ miếng an toàn (góc safeAngleDeg, tâm miếng ở hướng +X cục bộ)
    public static bool InSweepDanger(Vector2 local, float radius, float safeAngleDeg, float margin)
    {
        float maxR = radius + margin;
        if (local.sqrMagnitude > maxR * maxR) return false;     // Ngoài bán kính: an toàn
        if (local.sqrMagnitude < 0.01f) return true;            // Sát tâm: góc không xác định, coi như nguy hiểm

        // Ở trong miếng an toàn nếu góc lệch so với hướng +X nhỏ hơn nửa góc an toàn
        return Mathf.Abs(Vector2.SignedAngle(Vector2.right, local)) > safeAngleDeg * 0.5f;
    }
}