using UnityEngine;

// Liên kết linh hồn:
//  - ATK cộng thêm KHÔNG vào nhân vật mà chuyển sang MỌI vũ khí với hiệu quả atkEfficiency
//  - HP/DEF/SPD cộng thêm KHÔNG vào nhân vật mà chuyển thành hiệu quả hồi mana
// Bật/tắt toàn bộ cơ chế này bằng CombatRules.soulLinkEnabled.
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Soul Link")]
public class SoulLinkPassiveData : PassiveSkillData
{
    [Header("Hiệu quả chuyển ATK sang vũ khí theo cấp (1 = 100%)")]
    public float[] atkEfficiency = new float[MaxLevel];

    [Header("Hiệu quả hồi mana nhận được cho MỖI điểm chỉ số cộng thêm (0.01 = 1%)")]
    public float[] manaPerHp = new float[MaxLevel];
    public float[] manaPerDef = new float[MaxLevel];
    public float[] manaPerSpeed = new float[MaxLevel];        // MỖI 1 điểm tốc độ di chuyển cộng thêm

    public float GetAtk(int level) => Pick(atkEfficiency, level);
    public float GetHp(int level) => Pick(manaPerHp, level);
    public float GetDef(int level) => Pick(manaPerDef, level);
    public float GetSpeed(int level) => Pick(manaPerSpeed, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new SoulLinkPassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"ATK cộng thêm chuyển sang mọi vũ khí với hiệu quả {GetAtk(level) * 100f:0.#}%. " +
           $"HP, DEF, SPD cộng thêm chuyển thành hiệu quả hồi mana: 1 HP = {GetHp(level) * 100f:0.##}%, " +
           $"1 DEF = {GetDef(level) * 100f:0.##}%, 1 tốc độ = {GetSpeed(level) * 100f:0.##}%.";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_soul_link";
        skillName = "Liên kết linh hồn";
        atkEfficiency = new[] { 1.0f, 1.2f, 1.4f, 1.6f, 1.8f };
        manaPerHp = new[] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f };
        manaPerDef = new[] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f };
        manaPerSpeed = new[] { 0.002f, 0.004f, 0.006f, 0.008f, 0.01f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class SoulLinkPassiveEffect : PassiveSkillEffect
{
    private readonly SoulLinkPassiveData data;
    private float atkEff, hpRate, defRate, spdRate;

    public SoulLinkPassiveEffect(PlayerStats stats, SoulLinkPassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level)
    {
        atkEff = data.GetAtk(level);
        hpRate = data.GetHp(level);
        defRate = data.GetDef(level);
        spdRate = data.GetSpeed(level);
    }

    // Gọi nhiều lần vẫn an toàn (PlayerStats chỉ làm việc khi trạng thái ĐỔI, còn lại chỉ cập nhật hệ số)
    public override void Enable() { stats.SetSoulLink(true, atkEff, hpRate, defRate, spdRate); }
    public override void Disable() { stats.SetSoulLink(false, atkEff, hpRate, defRate, spdRate); }
}