using System.Collections.Generic;
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
    public float manaValue = 2f;   // Lượng mana nạp khi tiêu diệt quái này

    [Header("Tấn công tầm xa (attackRange = 0 nghĩa là cận chiến)")]
    public float attackRange = 0f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;
    public float projectileLifetime = 4f;

    [Header("Rơi đồ")]
    [Tooltip("Luôn rơi (vàng)")]
    public List<DropEntry> guaranteedDrops = new List<DropEntry>();
    [Tooltip("Xác suất để rơi THÊM 1 món từ danh sách bên dưới (0..1)")]
    [Range(0f, 1f)] public float randomDropChance = 0.5f;
    [Tooltip("Nếu rơi thêm, chọn 1 món theo trọng số")]
    public List<DropEntry> randomDrops = new List<DropEntry>();
    [Tooltip("Bảng rơi dùng chung (vật phẩm hiếm). Được gộp với Random Drops riêng của quái")]
    public DropTableData sharedDrops;

    [Header("Trạng thái")]
    [Tooltip("Miễn nhiễm đẩy lùi và trói (dành cho BOSS)")]
    public bool statusImmune = false;
    [Tooltip("Là BOSS: không bị dịch chuyển (recycle) khi chạm trần số lượng quái")]
    public bool isBoss = false;
}