// Tên hiển thị tiếng Việt của nguyên tố (dùng cho mô tả skill)
public static class ElementDisplay
{
    public static string Name(ElementType element)
    {
        switch (element)
        {
            case ElementType.Metal: return "Kim loại";
            case ElementType.Fire: return "Lửa";
            case ElementType.Lightning: return "Lôi";
            case ElementType.Wind: return "Phong";
            case ElementType.Soul: return "Linh hồn";
            case ElementType.Light: return "Ánh sáng";
            case ElementType.Earth: return "Đất";
            case ElementType.Water: return "Nước";
            case ElementType.Plant: return "Mộc";
            case ElementType.Dark: return "Bóng tối";
            case ElementType.Pure: return "Thuần khiết";
            default: return element.ToString();
        }
    }
}