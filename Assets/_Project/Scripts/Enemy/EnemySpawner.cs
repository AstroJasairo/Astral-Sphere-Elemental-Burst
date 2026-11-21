using UnityEngine;

public enum EdgeSpawnMode
{
    SkipWhenOutside,    // Ngoài biên thì thử góc khác, không được thì bỏ lượt spawn
    ClampIntoBounds     // Ngoài biên thì kẹp vào mép biên (chỉ nhận nếu nằm ngoài tầm nhìn camera)
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private StageData stage;
    [Tooltip("Thứ tự khớp enum EnemyType: Melee, Ranged, Tank, Mage, Assassin. Loại chưa có thì để trống")]
    [SerializeField] private EnemyData[] enemyByType = new EnemyData[5];
    [SerializeField] private int maxAlive = 300;        // Giới hạn quái cùng lúc để bảo vệ hiệu năng
    [SerializeField] private float spawnMargin = 1.5f;  // Khoảng cách thêm ra ngoài mép màn hình

    [Header("Spawn gần biên bản đồ")]
    [SerializeField] private EdgeSpawnMode edgeSpawnMode = EdgeSpawnMode.SkipWhenOutside;

    private const float ViewPadding = 0.5f;      // Lề thêm quanh vùng camera nhìn thấy

    private const int MaxSpawnAttempts = 10;     // Số lần thử chọn góc khác khi điểm rơi không hợp lệ
    private const float SpawnEdgeMargin = 0.5f;  // Quái không spawn sát mép biên
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
        float defMul = stage.defMultiplier.Evaluate(minutes);

        for (int i = 0; i < enemyByType.Length; i++)
        {
            EnemyData data = enemyByType[i];
            if (data == null) continue;         // Loại quái này chưa làm thì bỏ qua

            int count = phase.GetCount((EnemyType)i);
            for (int n = 0; n < count; n++)
                SpawnOne(data, hpMul, dmgMul, defMul);
        }
    }

    private void SpawnOne(EnemyData data, float hpMul, float dmgMul, float defMul)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float baseRadius = Mathf.Sqrt(halfW * halfW + halfH * halfH) + spawnMargin;

        Vector3 playerPos = GameManager.Instance.Player.position;
        Vector3 camPos = cam.transform.position;            // Camera có thể lệch người chơi khi chạm biên (Confiner)

        Vector3 pos = playerPos;
        float radius = baseRadius;
        bool found = false;

        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            radius = baseRadius + Random.Range(0f, 2f);
            float angle = Random.value * Mathf.PI * 2f;
            Vector3 candidate = playerPos + (Vector3)(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);

            if (!MapArea.Contains(candidate, SpawnEdgeMargin))
            {
                if (edgeSpawnMode == EdgeSpawnMode.SkipWhenOutside) continue;     // Cách 1: thử góc khác

                // Cách 2: kẹp vào trong biên, nhưng loại nếu rơi vào vùng camera đang nhìn thấy
                candidate = (Vector3)MapArea.Clamp(candidate, SpawnEdgeMargin);

                bool insideView = Mathf.Abs(candidate.x - camPos.x) < halfW + ViewPadding
                               && Mathf.Abs(candidate.y - camPos.y) < halfH + ViewPadding;
                if (insideView) continue;
            }

            pos = candidate;
            found = true;
            break;
        }

        if (!found) return;      // Không có chỗ hợp lệ: bỏ lượt này

        // Chạm trần: không tạo thêm, mà kéo một con đang ở quá xa về vị trí này
        if (EnemyBase.ActiveCount >= maxAlive)
        {
            TryRecycleFarEnemy(pos, radius * 1.5f);
            return;
        }

        GameObject go = PoolManager.Instance.Get(data.prefab, pos, Quaternion.identity);
        go.GetComponent<EnemyBase>().Init(data, hpMul, dmgMul, defMul);
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

    public void SetStage(StageData newStage)
    {
        stage = newStage;
    }
}