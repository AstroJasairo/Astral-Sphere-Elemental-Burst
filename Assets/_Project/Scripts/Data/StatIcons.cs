using UnityEngine;

// Cổng tra cứu icon chỉ số cho UI. Dữ liệu nằm trong MetaConfig (cùng chỗ với icon nâng cấp chung).
// Không đi qua MetaProgress vì icon không phải trạng thái tiến trình.
public static class StatIcons
{
    public static Sprite Get(StatType stat)
    {
        MetaConfig cfg = MetaProgress.Config;      // Nạp lười, đã có cảnh báo khi thiếu asset
        return cfg != null ? cfg.GetStatIcon(stat) : null;
    }

    public static Sprite Star
    {
        get
        {
            MetaConfig cfg = MetaProgress.Config;
            return cfg != null ? cfg.starIcon : null;
        }
    }
}