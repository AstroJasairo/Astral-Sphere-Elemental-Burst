using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private StageData stage;
    [Tooltip("Thứ tự khớp enum EnemyType: Melee, Ranged, Tank, Mage, Assassin. Loại chưa có thì để trống")]
    [SerializeField] private EnemyData[] enemyByType = new EnemyData[5];
    [SerializeField] private int maxAlive = 300;        // Giới hạn quái cùng lúc để bảo vệ hiệu năng
    [SerializeField] private float spawnMargin = 1.5f;  // Khoảng cách thêm ra ngoài mép màn hình

    private float spawnTimer;
    //private int aliveCount;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    //private void OnEnable() { EnemyBase.EnemyKilled += OnEnemyKilled; }
    //private void OnDisable() { EnemyBase.EnemyKilled -= OnEnemyKilled; }

    //private void OnEnemyKilled(EnemyBase enemy) { aliveCount--; }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer < stage.spawnInterval) return;
        spawnTimer -= stage.spawnInterval;     // Trừ đi (không đặt về 0) để không lệch nhịp theo thời gian

        SpawnWave();
    }

    private void SpawnWave()
    {
        if (stage.stopNormalSpawnOnBoss && GameManager.Instance.ElapsedTime >= stage.bossTime) return;
        float time = GameManager.Instance.ElapsedTime;
        SpawnPhase phase = stage.GetPhase(time);
        if (phase == null) return;              // Chưa tới 00:15 thì chưa spawn

        // Hệ số máu/sát thương tăng theo thời gian (trục X của đường cong là phút)
        float minutes = time / 60f;
        float hpMul = stage.hpMultiplier.Evaluate(minutes);
        float dmgMul = stage.damageMultiplier.Evaluate(minutes);

        for (int i = 0; i < enemyByType.Length; i++)
        {
            EnemyData data = enemyByType[i];
            if (data == null) continue;         // Loại quái này chưa làm thì bỏ qua

            int count = phase.GetCount((EnemyType)i);
            for (int n = 0; n < count; n++)
                SpawnOne(data, hpMul, dmgMul);
        }
    }

    private void SpawnOne(EnemyData data, float hpMul, float dmgMul)
    {
        // Tính vị trí spawn ngoài mép màn hình (giữ nguyên như trước)
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float radius = Mathf.Sqrt(halfW * halfW + halfH * halfH) + spawnMargin + Random.Range(0f, 2f);
        float angle = Random.value * Mathf.PI * 2f;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        Vector3 pos = GameManager.Instance.Player.position + (Vector3)offset;

        // Chạm trần: không tạo thêm, mà kéo một con đang ở quá xa về vị trí này
        if (EnemyBase.ActiveCount >= maxAlive)
        {
            TryRecycleFarEnemy(pos, radius * 1.5f);
            return;
        }

        GameObject go = PoolManager.Instance.Get(data.prefab, pos, Quaternion.identity);
        go.GetComponent<EnemyBase>().Init(data, hpMul, dmgMul);
    }

    // Tìm một con quái cách người chơi xa hơn minDistance và dịch chuyển nó tới newPos
    private void TryRecycleFarEnemy(Vector3 newPos, float minDistance)
    {
        float minSqr = minDistance * minDistance;
        Vector3 playerPos = GameManager.Instance.Player.position;

        // Duyệt HashSet bằng foreach không sinh rác
        foreach (EnemyBase enemy in EnemyBase.Active)
        {
            if (enemy.Data.isBoss) continue;      // Không bao giờ tái sử dụng BOSS
            if ((enemy.transform.position - playerPos).sqrMagnitude > minSqr)
            {
                enemy.TeleportTo(newPos);
                return;   // Mỗi lần chỉ tái sử dụng 1 con
            }
        }
    }
}