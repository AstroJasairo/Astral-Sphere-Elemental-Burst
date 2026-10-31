using UnityEngine;

// Dữ liệu BOSS Lord of the Wolf. Tạo bằng: Assets > Create > Astral Sphere > Boss > Lord Of The Wolf Data
[CreateAssetMenu(menuName = "Astral Sphere/Boss/Lord Of The Wolf Data")]
public class LordOfTheWolfData : EnemyData
{
    [Header("Nội tại: miễn nhiễm sát thương khi người chơi đứng quá xa")]
    public float immuneDistance = 8f;          // Xa hơn khoảng này thì miễn nhiễm
    public float immuneMargin = 0.8f;          // Phải lại gần hơn (immuneDistance - margin) mới hết miễn nhiễm (tránh chớp bật tắt)

    [Header("Kỹ năng: cào AOE")]
    public float firstClawDelay = 4f;
    public float clawInterval = 6f;
    public float clawRadius = 3.5f;
    public float clawWarning = 1.2f;           // Thời gian vòng cảnh báo (thời gian để né)
    public float clawDamage = 30f;
    public bool clawShrink = false;            // false: vòng cam lớn dần từ tâm. true: nhỏ dần từ viền vào tâm
}