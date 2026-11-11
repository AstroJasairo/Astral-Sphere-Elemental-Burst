using System.Collections.Generic;
using UnityEngine;

// Dữ liệu BOSS Lord of the Wolf. Tạo bằng: Assets > Create > Astral Sphere > Boss > Lord Of The Wolf Data
[CreateAssetMenu(menuName = "Astral Sphere/Boss/Lord Of The Wolf Data")]
public class LordOfTheWolfData : EnemyData
{
    [Header("Nội tại: miễn nhiễm sát thương khi người chơi đứng quá xa")]
    public float immuneDistance = 8f;          // Xa hơn khoảng này thì miễn nhiễm
    public float immuneMargin = 0.8f;          // Phải lại gần hơn (immuneDistance - margin) mới hết miễn nhiễm (tránh chớp bật tắt)

    [Header("Nội tại: giảm sát thương khi máu xuống thấp")]
    [Tooltip("Mốc máu còn lại (0.75 = 75%). Tự sắp xếp giảm dần. Mỗi mốc kích hoạt MỘT lần.")]
    public float[] reductionHpThresholds = { 0.75f, 0.5f, 0.25f };
    [Range(0f, 1f)] public float thresholdReduction = 0.5f;   // Giảm 50% sát thương nhận vào
    public float thresholdDuration = 20f;

    [Header("Kỹ năng: cào AOE")]
    public float firstClawDelay = 4f;
    public float clawInterval = 6f;
    public float clawRadius = 3.5f;
    public float clawWarning = 1.2f;           // Thời gian vòng cảnh báo (thời gian để né)
    public float clawDamage = 30f;
    public bool clawShrink = false;            // false: vòng cam lớn dần từ tâm. true: nhỏ dần từ viền vào tâm
    public List<HitEffect> clawEffects = new List<HitEffect>();      // Đòn cào: Câm lặng 5s (đặt trong asset)

    protected override void OnValidate()
    {
        base.OnValidate();                                            // giữ cảnh báo DOT của EnemyData
        if (reductionHpThresholds != null)
            System.Array.Sort(reductionHpThresholds, (a, b) => b.CompareTo(a));   // giảm dần: 0.75, 0.5, 0.25
    }
}