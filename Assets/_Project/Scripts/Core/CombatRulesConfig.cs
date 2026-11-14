using UnityEngine;

public enum StackMode { Multiplicative, Additive }   // Nhân tích lũy hay cộng thẳng

// Luật chiến đấu chỉnh được. Tạo bằng: Assets > Create > Astral Sphere > Combat Rules
// TÙY CHỌN: đặt vào Resources với tên "CombatRules". Thiếu thì dùng mặc định bên dưới.
[CreateAssetMenu(menuName = "Astral Sphere/Combat Rules")]
public class CombatRulesConfig : ScriptableObject
{
    [Header("Giảm sát thương nhận vào")]
    [Tooltip("Cách gộp các nguồn CÙNG nhóm (cùng 'chung' hoặc cùng một nguyên tố). Khác nhóm luôn CỘNG.")]
    public StackMode reductionStacking = StackMode.Multiplicative;
    [Range(0f, 1f)] public float maxTotalReduction = 0.9f;     // Tổng giảm tối đa

    [Header("Hiệu quả hồi phục")]
    public StackMode healingStacking = StackMode.Multiplicative;

    [Header("BOSS")]
    [Tooltip("Các hiệu ứng giảm sát thương theo mốc máu của BOSS chồng thời gian: bật = nhân tích lũy, tắt = làm mới thời gian, không cộng dồn")]
    public bool overlappingBossReductionsStack = true;

    [Header("Tăng sát thương (nguồn từ passive). Cách gộp trong CÙNG nhóm; khác nhóm luôn CỘNG")]
    public StackMode damageBonusStacking = StackMode.Additive;
    [Header("Hiệu quả hồi mana")]
    public StackMode manaGainStacking = StackMode.Additive;      // Cộng thẳng. Đổi sang Multiplicative để nhân tích lũy
}