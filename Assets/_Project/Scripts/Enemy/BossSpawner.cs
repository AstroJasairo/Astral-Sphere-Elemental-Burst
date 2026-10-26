using UnityEngine;

// Triệu hồi BOSS một lần khi tới thời điểm bossTime của màn chơi
public class BossSpawner : MonoBehaviour
{
    [SerializeField] private StageData stage;
    [SerializeField] private EnemyData bossData;        // Asset ThunderBirdData
    [SerializeField] private float spawnMargin = 2f;

    private bool spawned;

    private void Update()
    {
        if (spawned) return;
        if (GameManager.Instance.ElapsedTime < stage.bossTime) return;
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        spawned = true;

        // Xuất hiện ngoài mép màn hình theo một góc ngẫu nhiên (cùng cách tính với EnemySpawner)
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float radius = Mathf.Sqrt(halfW * halfW + halfH * halfH) + spawnMargin;
        float angle = Random.value * Mathf.PI * 2f;
        Vector3 pos = GameManager.Instance.Player.position
                    + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

        GameObject go = PoolManager.Instance.Get(bossData.prefab, pos, Quaternion.identity);
        go.GetComponent<EnemyBase>().Init(bossData, 1f, 1f);   // Hệ số 1: BOSS không tăng chỉ số theo thời gian
        go.GetComponent<BossBase>().Begin();
    }

    // Chuột phải vào component khi đang Play để gọi BOSS ngay, khỏi phải chờ 30 phút
    [ContextMenu("Debug: Spawn Boss Now")]
    private void DebugSpawn()
    {
        if (!spawned) SpawnBoss();
    }
}