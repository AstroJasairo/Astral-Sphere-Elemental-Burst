using UnityEngine;

// Dữ liệu BOSS Chim sấm sét. Tạo bằng: Assets > Create > Astral Sphere > Boss > Thunder Bird Data
[CreateAssetMenu(menuName = "Astral Sphere/Boss/Thunder Bird Data")]
public class ThunderBirdData : EnemyData
{
    [Header("Nội tại: sét đánh liên tục")]
    public float firstStrikeDelay = 2f;      // Đợt sét đầu tiên sau khi BOSS xuất hiện
    public float strikeInterval = 2.5f;      // Khoảng cách giữa các đợt
    public int strikesPerWave = 3;           // Số vòng sét mỗi đợt (vòng đầu luôn nhắm thẳng người chơi)
    public float strikeRadius = 1.8f;
    public float strikeWarning = 1.5f;       // Thời gian vòng cam lớn dần (thời gian để né)
    public float strikeSpread = 5f;          // Các vòng còn lại rải ngẫu nhiên trong bán kính này quanh người chơi
    public float strikeDamage = 20f;

    [Header("Kỹ năng: lao tới")]
    public float firstDashDelay = 6f;
    public float dashInterval = 8f;
    public float dashWarning = 1.5f;
    public float dashLength = 14f;
    public float dashWidth = 3.2f;           // Nên bằng đường kính thân BOSS cộng chút lề
    public float dashSpeed = 18f;
}