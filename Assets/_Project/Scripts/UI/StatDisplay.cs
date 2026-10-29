// Chuỗi hiển thị cho các chỉ số
public static class StatDisplay
{
    public static string Label(StatType stat)
    {
        switch (stat)
        {
            case StatType.Hp: return "HP";
            case StatType.Mp: return "MP";
            case StatType.Atk: return "ATK";
            case StatType.Def: return "DEF";
            case StatType.Spd: return "SPD";
            case StatType.Mag: return "MAG";
            default: return stat.ToString();
        }
    }

    public static string Describe(StatType stat)
    {
        switch (stat)
        {
            case StatType.Hp: return "Máu tối đa";
            case StatType.Mp: return "Giảm mana cần cho All Out Assault";
            case StatType.Atk: return "Sức tấn công";
            case StatType.Def: return "Giảm sát thương nhận vào";
            case StatType.Spd: return "Tốc độ di chuyển";
            case StatType.Mag: return "Bán kính hút vật phẩm";
            default: return "";
        }
    }

    // HP và DEF cộng thẳng; các chỉ số còn lại là phần trăm (MP là phần trăm GIẢM)
    public static string FormatBonus(StatType stat, float value)
    {
        if (stat == StatType.Hp || stat == StatType.Def) return $"+{value:0.##}";
        if (stat == StatType.Mp) return $"-{value * 100f:0.#}%";
        return $"+{value * 100f:0.#}%";
    }
}