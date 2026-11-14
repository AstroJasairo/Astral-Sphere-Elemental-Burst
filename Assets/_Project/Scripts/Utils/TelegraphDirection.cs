// Hướng chạy của vùng cam. Mỗi dạng telegraph chỉ dùng một phần các giá trị,
// giá trị không phù hợp sẽ rơi về mặc định (xem hàm hỗ trợ bên dưới).
//   Tròn, nón, nhẫn, nón nhẫn : InToOut (lan ra ngoài), OutToIn (co vào trong)
//   Sweep, sweep nhẫn         : Clockwise (trái sang phải khi nhìn về phía trước), CounterClockwise
//   Chữ nhật (LineWeapon)     : Forward, Backward, Sideways, LeftToRight, RightToLeft
public enum TelegraphDirection
{
    InToOut,
    OutToIn,
    Forward,
    Backward,
    Sideways,
    LeftToRight,
    RightToLeft,
    Clockwise,
    CounterClockwise
}

// Đổi enum chung sang enum riêng của từng loại telegraph
public static class TelegraphDirectionUtil
{
    // Tròn, nón, nhẫn: true = vùng cam co từ ngoài vào trong
    public static bool IsInward(TelegraphDirection d)
    {
        return d == TelegraphDirection.OutToIn;
    }

    // Sweep: LeftToRight cũng là chiều kim đồng hồ (nhìn về phía trước, cạnh trái ở phía +Y)
    public static bool IsClockwise(TelegraphDirection d)
    {
        return d == TelegraphDirection.Clockwise || d == TelegraphDirection.LeftToRight;
    }

    // Chữ nhật: InToOut tương đương Forward (từ cạnh gần ra cạnh xa)
    public static RectFill ToRectFill(TelegraphDirection d)
    {
        switch (d)
        {
            case TelegraphDirection.Backward:
            case TelegraphDirection.OutToIn: return RectFill.SweepBackward;
            case TelegraphDirection.Sideways: return RectFill.ExpandSideways;
            case TelegraphDirection.LeftToRight: return RectFill.SweepLeftToRight;
            case TelegraphDirection.RightToLeft: return RectFill.SweepRightToLeft;
            default: return RectFill.SweepForward;
        }
    }
}