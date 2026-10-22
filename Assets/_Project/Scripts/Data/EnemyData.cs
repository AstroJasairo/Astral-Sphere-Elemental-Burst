using UnityEngine;

// Dữ liệu một loại kẻ địch. Tạo bằng: Assets > Create > Astral Sphere > Enemy Data
[CreateAssetMenu(menuName = "Astral Sphere/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public EnemyType type;
    public GameObject prefab;      // Prefab của quái (đi kèm dữ liệu)

    [Header("Chỉ số gốc (trước khi nhân hệ số theo thời gian)")]
    public float baseHp = 20f;
    public float baseDamage = 5f;
    public float moveSpeed = 2f;

    [Header("Phần thưởng")]
    public int scoreValue = 10;    // Điểm cộng khi tiêu diệt
}