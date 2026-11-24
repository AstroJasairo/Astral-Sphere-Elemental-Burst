using Unity.Cinemachine;
using UnityEngine;

// Đọc map đã chọn và cấu hình màn chơi. Chạy sớm nhất (trước các spawner) nhờ DefaultExecutionOrder.
[DefaultExecutionOrder(-100)]
public class MapLoader : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private BossSpawner bossSpawner;
    [SerializeField] private SpriteRenderer backgroundRenderer;     // Nền dạng sprite (nếu có). Nên gắn thêm TiledBackground
    [SerializeField] private Camera backgroundCamera;               // Dùng màu nền của camera khi không có nền sprite

    [Header("Biên bản đồ")]
    [SerializeField] private Material borderMaterial;               // Dùng Mat_Telegraph (shader Sprite Unlit)
    [SerializeField] private int borderSortingOrder = -50;          // Trên nền (-99), dưới quái và vật phẩm (0)

    [Header("CHỈ ĐỂ TEST (Editor/Development Build): ghi đè kích thước biên của map")]
    [SerializeField] private bool useDebugBounds;
    [SerializeField] private Vector2 debugBoundsSize = new Vector2(40f, 40f);

    private IMapArea area;

    private void Awake()
    {
        MapData map = MetaProgress.GetSelectedMap();          // null: chạy thẳng scene Game, giữ cấu hình trong scene
        GameDifficulty.Current = map != null ? Mathf.Clamp(map.difficulty, 1, 3) : 1;

        // ---- Biên bản đồ (làm TRƯỚC các spawner) ----
        Vector2 size = map != null ? map.boundsSize : Vector2.zero;
        Color borderColor = map != null ? map.boundsBorderColor : Color.white;
        float borderWidth = map != null ? map.boundsBorderWidth : 0.25f;

        if (useDebugBounds && Debug.isDebugBuild) size = debugBoundsSize;

        SetupBounds(size, borderColor, borderWidth);

        if (map == null) return;

        // ---- Cấu hình trận ----
        if (map.stage != null) enemySpawner.SetStage(map.stage);
        bossSpawner.Configure(map.stage, map.boss);

        if (map.layoutPrefab != null) Instantiate(map.layoutPrefab);      // Bố cục riêng của map (tường, vật cản...)

        if (map.overrideBackground) ApplyBackground(map);
    }

    private void OnDestroy()
    {
        if (MapArea.Current == area) MapArea.Current = null;
    }

    // ---------------- Biên ----------------

    private void SetupBounds(Vector2 size, Color borderColor, float borderWidth)
    {
        if (size.x <= 0f || size.y <= 0f)
        {
            MapArea.Current = null;                           // Không biên: mọi thứ hoạt động như cũ
            return;
        }

        size = Vector2.Max(size, new Vector2(MapData.MinBoundsSize, MapData.MinBoundsSize));

        area = new RectMapArea(Vector2.zero, size);
        MapArea.Current = area;

        Vector2[] outline = area.GetOutline();
        CreateBorder(outline, borderColor, borderWidth);
        SetupCameraConfiner(outline);
    }

    // Đường viền nhìn thấy
    private void CreateBorder(Vector2[] outline, Color color, float width)
    {
        if (borderMaterial == null)
        {
            Debug.LogWarning("MapLoader: chưa gán Border Material (dùng Mat_Telegraph), sẽ không có đường viền.", this);
            return;
        }

        var go = new GameObject("MapBorder");
        var line = go.AddComponent<LineRenderer>();
        line.sharedMaterial = borderMaterial;
        line.useWorldSpace = true;
        line.loop = true;                                      // Nối điểm cuối về điểm đầu
        line.positionCount = outline.Length;
        for (int i = 0; i < outline.Length; i++)
            line.SetPosition(i, new Vector3(outline[i].x, outline[i].y, 0f));

        line.widthMultiplier = width;
        line.startColor = color;
        line.endColor = color;
        line.numCornerVertices = 4;                            // Góc bo tròn, không hở
        line.sortingOrder = borderSortingOrder;
    }

    // Giới hạn camera trong biên bằng Cinemachine Confiner 2D
    private void SetupCameraConfiner(Vector2[] outline)
    {
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam == null)
        {
            Debug.LogWarning("MapLoader: không thấy CinemachineCamera, camera sẽ không bị giới hạn trong biên.", this);
            return;
        }

        // Khung giới hạn: PolygonCollider2D dạng Trigger (dùng polygon để sau này đổi sang hình bất kỳ được)
        var shape = new GameObject("MapBoundsShape");
        shape.layer = 2;                                       // Ignore Raycast: không va chạm với ai
        var poly = shape.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.SetPath(0, outline);

        if (!vcam.TryGetComponent(out CinemachineConfiner2D confiner))
            confiner = vcam.gameObject.AddComponent<CinemachineConfiner2D>();

        confiner.BoundingShape2D = poly;
        confiner.Damping = 0f;                                 // Camera dừng ngay ở mép, không trượt
        confiner.InvalidateBoundingShapeCache();               // Báo Confiner tính lại khung
    }

    // ---------------- Nền ----------------

    private void ApplyBackground(MapData map)
    {
        if (backgroundRenderer != null)
        {
            if (backgroundRenderer.TryGetComponent(out TiledBackground tiled))
            {
                tiled.Configure(map.gameBackground, map.backgroundColor, map.tileWorldSize);
            }
            else
            {
                // Nền kiểu cũ (không có TiledBackground): phủ cố định 100 x 100
                if (map.gameBackground != null) backgroundRenderer.sprite = map.gameBackground;
                backgroundRenderer.color = map.backgroundColor;
                backgroundRenderer.size = new Vector2(100f, 100f);
                Debug.LogWarning("MapLoader: object nền chưa có TiledBackground, đang dùng nền cũ cố định 100 x 100.", this);
            }
        }
        else if (backgroundCamera != null)
        {
            backgroundCamera.backgroundColor = map.backgroundColor;
        }
    }

    // ---------------- Gizmo (Scene view) ----------------

    private void OnDrawGizmosSelected()
    {
        Vector2 size = Vector2.zero;
        if (Application.isPlaying && MapArea.Current != null) size = MapArea.Current.Size;
        else if (useDebugBounds) size = debugBoundsSize;

        if (size.x <= 0f || size.y <= 0f) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0f));
    }
}