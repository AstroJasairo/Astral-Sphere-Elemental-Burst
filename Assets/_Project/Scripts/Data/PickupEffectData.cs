using UnityEngine;

// Cấu hình hiệu ứng của vật phẩm. Tạo bằng: Assets > Create > Astral Sphere > Pickup Effect Data
// Giá trị phần trăm: 0.5 = +50%
[CreateAssetMenu(menuName = "Astral Sphere/Pickup Effect Data")]
public class PickupEffectData : ScriptableObject
{
    [Header("Bình máu")]
    [Range(0f, 1f)] public float healPercent = 0.3f;     // Hồi % máu tối đa

    [Header("Thuốc tăng tốc")]
    public float speedBonusPct = 0.5f;
    public float speedDuration = 8f;

    [Header("Bình cuồng nộ")]
    public float rageBonusPct = 0.5f;
    public float rageDuration = 8f;

    [Header("Nam châm")]
    public float magnetBonusPct = 3f;                    // +300% bán kính hút
    public float magnetDuration = 8f;

    [Header("Khiên")]
    public float shieldBase = 30f;                       // Lá chắn cơ bản
    public float shieldDefScale = 0.1f;                  // Mỗi điểm DEF tăng thêm 10% lá chắn

    [Header("Tăng điểm")]
    public float scoreBonusPct = 1f;                     // +100% điểm (nhân đôi)
    public float scoreDuration = 10f;
}