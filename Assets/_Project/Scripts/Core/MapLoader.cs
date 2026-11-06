using UnityEngine;

// Đọc map đã chọn và cấu hình màn chơi. Chạy sớm nhất (trước các spawner) nhờ DefaultExecutionOrder.
[DefaultExecutionOrder(-100)]
public class MapLoader : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private BossSpawner bossSpawner;
    [SerializeField] private SpriteRenderer backgroundRenderer;     // Nền dạng sprite (nếu có)
    [SerializeField] private Camera backgroundCamera;               // Dùng màu nền của camera khi không có nền sprite

    private void Awake()
    {
        MapData map = MetaProgress.GetSelectedMap();
        if (map == null) return;                                    // Chạy thẳng scene Game: giữ cấu hình trong scene

        if (map.stage != null) enemySpawner.SetStage(map.stage);
        bossSpawner.Configure(map.stage, map.boss);

        if (!map.overrideBackground) return;

        if (backgroundRenderer != null)
        {
            if (map.gameBackground != null) backgroundRenderer.sprite = map.gameBackground;
            backgroundRenderer.color = map.backgroundColor;
            backgroundRenderer.size = new Vector2(100f, 100f);
        }
        else if (backgroundCamera != null)
        {
            backgroundCamera.backgroundColor = map.backgroundColor;
        }
    }
}