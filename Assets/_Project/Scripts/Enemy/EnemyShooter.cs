using UnityEngine;

// Gắn thêm vào prefab quái tầm xa. Thông số lấy từ EnemyData nên mỗi loại quái có số riêng.
[RequireComponent(typeof(EnemyBase))]
public class EnemyShooter : MonoBehaviour
{
    private EnemyBase enemy;
    private float timer;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        // Lệch nhịp ngẫu nhiên để cả đàn không bắn cùng một lúc
        timer = Random.Range(0f, 1f);
    }

    private void Update()
    {
        EnemyData data = enemy.Data;
        if (data == null || data.projectilePrefab == null || enemy.Target == null) return;

        timer += Time.deltaTime;
        if (timer < data.attackCooldown) return;

        Vector2 toTarget = (Vector2)enemy.Target.position - (Vector2)transform.position;
        // Chưa vào tầm thì chờ: timer vẫn đầy nên sẽ bắn ngay khi vào tầm
        if (toTarget.sqrMagnitude > data.attackRange * data.attackRange) return;

        timer = 0f;
        GameObject go = PoolManager.Instance.Get(data.projectilePrefab, transform.position, Quaternion.identity);
        go.GetComponent<EnemyProjectile>().Init(
            enemy.Damage, toTarget.normalized, data.projectileSpeed, data.projectileLifetime);
    }
}