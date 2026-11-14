using UnityEngine;

// Liên hoa kích: khi kích hoạt All Out Assault, giảm thời gian hồi chiêu CÒN LẠI của skill chủ động.
// Tạo bằng: Assets > Create > Astral Sphere > Skills > Passive > Lotus Strike
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Passive/Lotus Strike")]
public class LotusStrikePassiveData : PassiveSkillData
{
    [Header("Số giây hồi chiêu được giảm theo cấp 1..5")]
    public float[] cooldownReduction = new float[MaxLevel];

    public float GetSeconds(int level) => Pick(cooldownReduction, level);

    public override PassiveSkillEffect CreateEffect(PlayerStats stats) => new LotusStrikePassiveEffect(stats, this);

    public override string DescribeLevel(int level)
        => $"Khi kích hoạt All Out Assault, giảm {GetSeconds(level):0.#} giây hồi chiêu còn lại của kỹ năng chủ động.";

    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        id = "acronomia_lotus_strike";
        skillName = "Liên hoa kích";
        cooldownReduction = new[] { 5f, 10f, 15f, 20f, 25f };
        upgradeCost = new[] { 1000, 3000, 8000, 20000, 50000 };          // TẠM
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class LotusStrikePassiveEffect : PassiveSkillEffect
{
    private readonly LotusStrikePassiveData data;
    private float seconds;
    private AllOutAssaultSystem aoa;

    public LotusStrikePassiveEffect(PlayerStats stats, LotusStrikePassiveData data) : base(stats) { this.data = data; }

    public override void SetLevel(int level) { seconds = data.GetSeconds(level); }

    public override void Enable()
    {
        // AllOutAssaultSystem nằm trong _Managers, không nằm trên Player. Tìm một lần (hiếm khi gọi)
        if (aoa == null) aoa = Object.FindFirstObjectByType<AllOutAssaultSystem>();
        if (aoa == null) return;

        aoa.OnActivated -= HandleAoaActivated;                 // Gọi Enable nhiều lần không bị đăng ký trùng
        aoa.OnActivated += HandleAoaActivated;
    }

    public override void Disable()
    {
        if (aoa != null) aoa.OnActivated -= HandleAoaActivated;
    }

    // Dùng lại ReduceCooldown của Đồng hồ cát: chỉ có tác dụng khi hồi chiêu ĐÃ BẮT ĐẦU và chưa xong
    private void HandleAoaActivated()
    {
        SkillSystem skills = stats.Skills;
        if (skills != null) skills.ReduceCooldown(seconds);
    }
}