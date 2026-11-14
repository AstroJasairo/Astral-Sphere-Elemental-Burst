using UnityEngine;

// Bảng chỉ số của một nhân vật khi VÀO TRẬN = chỉ số cơ bản + nâng cấp vĩnh viễn.
// PlayerStats (trong trận) và menu Character cùng dùng class này nên luôn khớp nhau.
public struct CharacterStatSheet
{
    public float maxHp;
    public float atk;
    public float def;
    public float moveSpeed;
    public float magnetRadius;
    public float manaCostReduction;     // Phần giảm mana cần để dùng All Out Assault (0.1 = giảm 10%)
    public float maxMana;               // Mana tối đa thực tế = mana gốc x hệ số MP

    public static CharacterStatSheet Build(CharacterData c)
    {
        var sheet = new CharacterStatSheet
        {
            maxHp = c.baseMaxHp + MetaProgress.GetStatBonus(c, StatType.Hp),
            atk = c.baseAtk * (1f + MetaProgress.GetStatBonus(c, StatType.Atk)),
            def = c.baseDef + MetaProgress.GetStatBonus(c, StatType.Def),
            moveSpeed = c.baseMoveSpeed * (1f + MetaProgress.GetStatBonus(c, StatType.Spd)),
            magnetRadius = c.baseMagnetRadius * (1f + MetaProgress.GetStatBonus(c, StatType.Mag)),
            manaCostReduction = MetaProgress.GetStatBonus(c, StatType.Mp)
        };

        // Mana gốc riêng từng nhân vật (trước đây lấy chung từ AllOutAssaultData)
        sheet.maxMana = c.maxMana * Mathf.Max(PlayerStats.MinManaCostMultiplier, 1f - sheet.manaCostReduction);
        return sheet;
    }
}