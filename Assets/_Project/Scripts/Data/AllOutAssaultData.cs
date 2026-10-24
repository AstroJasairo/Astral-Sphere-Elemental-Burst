using UnityEngine;

// Cấu hình All Out Assault. Tạo bằng: Assets > Create > Astral Sphere > All Out Assault Data
[CreateAssetMenu(menuName = "Astral Sphere/All Out Assault Data")]
public class AllOutAssaultData : ScriptableObject
{
    [Header("Nạp mana")]
    public float maxMana = 100f;             // Chỉ số MP sẽ nhân giá trị này với ManaCostMultiplier

    [Header("Hiệu lực")]
    public float duration = 8f;              // Số giây buff kéo dài
    public float atkBonusPct = 0.5f;         // +50% ATK
    public float speedBonusPct = 0.25f;      // +25% tốc độ di chuyển
    [Range(0.1f, 1f)]
    public float cooldownMultiplier = 0.25f; // Hồi chiêu vũ khí còn 25% (bắn nhanh gấp 4)
}