using UnityEngine;

// Cổng đọc luật: nạp lười, không có asset thì dùng mặc định (không báo lỗi)
public static class CombatRules
{
    private static CombatRulesConfig config;

    public static CombatRulesConfig Config
    {
        get
        {
            if (config == null)
            {
                config = Resources.Load<CombatRulesConfig>("CombatRules");
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<CombatRulesConfig>();
                    config.hideFlags = HideFlags.HideAndDontSave;
                }
            }
            return config;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { config = null; }
}