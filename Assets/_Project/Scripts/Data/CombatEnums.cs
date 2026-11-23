using System;
using UnityEngine;

// Loại sát thương. Mechanic = sát thương của một CƠ CHẾ cụ thể (vd mốc 50 hit làm boss mất 10% máu).
// Miễn nhiễm sát thương CHỈ chặn Normal. Phần lớn sát thương trong game là Normal.
public enum DamageKind { Normal, Mechanic }

// Cờ miễn nhiễm (chọn nhiều trong Inspector)
[Flags]
public enum ImmunityFlags
{
    None = 0,
    Damage = 1 << 0,           // Miễn nhiễm sát thương Normal (đánh thường, DOT...). Sát thương Mechanic vẫn nhận
    Dot = 1 << 1,              // Miễn nhiễm DOT: không dính DOT mới, DOT đang có bị xóa
    CrowdControl = 1 << 2,     // Miễn nhiễm khống chế: Slow, Root, Stun, Tê cóng, Silence
    OtherDebuffs = 1 << 3,     // Debuff còn lại: Vulnerable, Exhaust, Blind, Yếu đuối, Suy giảm mana
    Death = 1 << 4,            // Miễn nhiễm hiệu ứng gây chết (instant kill)

    Indestructible = Damage | Dot | CrowdControl | OtherDebuffs,     // Bất hoại
    Supreme = Indestructible | Death                                 // Tối thượng
}

// Skill kích hoạt một lần hay có hồi chiêu
public enum SkillTriggerMode { OneTime, Cooldown }

// Máu hồi cố định hay theo phần trăm máu tối đa
public enum HealAmountMode { Fixed, Percent }

// "Giao thức": mức hiệu lực của một hiệu ứng lên từng bậc kẻ địch (bảng nằm trong CombatRulesConfig)
public enum ProtocolKind { Standard, Weak }

[Serializable]
public class TierProtocolTable
{
    [Range(0f, 1f)] public float standard = 0f;
    [Range(0f, 1f)] public float normal = 1f;        // Dùng cho cả người chơi
    [Range(0f, 1f)] public float elite = 0.5f;
    [Range(0f, 1f)] public float miniBoss = 0.2f;
    [Range(0f, 1f)] public float boss = 0.1f;
    [Range(0f, 1f)] public float cataclysm = 0f;

    public float Get(EnemyTier tier)
    {
        switch (tier)
        {
            case EnemyTier.Standard: return standard;
            case EnemyTier.Normal: return normal;
            case EnemyTier.Elite: return elite;
            case EnemyTier.MiniBoss: return miniBoss;
            case EnemyTier.Boss: return boss;
            case EnemyTier.Cataclysm: return cataclysm;
            default: return 0f;
        }
    }

    // Giao thức tiêu chuẩn: Standard 0, Normal 100%, Elite 50%, Mini Boss 20%, Boss 10%, Cataclysm 0
    public static TierProtocolTable CreateStandard()
    {
        return new TierProtocolTable { standard = 0f, normal = 1f, elite = 0.5f, miniBoss = 0.2f, boss = 0.1f, cataclysm = 0f };
    }

    // Giao thức yếu: Standard 0, Normal 100%, Elite 50%, Mini Boss 25%, Boss và Cataclysm 0
    public static TierProtocolTable CreateWeak()
    {
        return new TierProtocolTable { standard = 0f, normal = 1f, elite = 0.5f, miniBoss = 0.25f, boss = 0f, cataclysm = 0f };
    }
}

// Phân loại debuff theo nhóm miễn nhiễm
public static class ImmunityUtil
{
    public static bool IsCrowdControl(BuffType t)
    {
        return t == BuffType.Slow || t == BuffType.Root || t == BuffType.Stun
            || t == BuffType.Frostbite || t == BuffType.Silence;
    }

    // true: debuff này bị chặn bởi các cờ miễn nhiễm đang có
    public static bool BlocksDebuff(ImmunityFlags flags, BuffType type)
    {
        if (flags == ImmunityFlags.None || !BuffController.IsDebuff(type)) return false;
        return IsCrowdControl(type)
            ? (flags & ImmunityFlags.CrowdControl) != 0
            : (flags & ImmunityFlags.OtherDebuffs) != 0;
    }
}