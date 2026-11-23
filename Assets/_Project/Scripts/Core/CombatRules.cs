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

    private static readonly TierProtocolTable defaultStandard = TierProtocolTable.CreateStandard();
    private static readonly TierProtocolTable defaultWeak = TierProtocolTable.CreateWeak();

    // Hiệu lực (0..1) của một giao thức lên một bậc kẻ địch. Người chơi dùng Normal.
    public static float GetProtocolEffectiveness(ProtocolKind kind, EnemyTier tier)
    {
        CombatRulesConfig config = Config;
        TierProtocolTable table = kind == ProtocolKind.Weak ? config.weakProtocol : config.standardProtocol;
        if (table == null) table = kind == ProtocolKind.Weak ? defaultWeak : defaultStandard;
        return table.Get(tier);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { config = null; }
}