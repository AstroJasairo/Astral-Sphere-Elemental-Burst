using UnityEngine;

// Nền lát gạch bám theo camera. Chỉ phủ vừa vùng nhìn thấy (+1 ô mỗi phía) và dịch theo từng Ô NGUYÊN,
// nên hoa văn đứng yên trong thế giới và số ô luôn nhỏ, dù bản đồ lớn bao nhiêu.
// (SpriteRenderer chế độ Tiled phủ cả bản đồ sẽ vỡ khi quá nhiều ô: hàng triệu đỉnh.)
// Gắn vào object Background (có SpriteRenderer). Kích thước "Size" trong Inspector sẽ bị script ghi đè.
// Chạy sau Cinemachine (200) để đọc vị trí camera cuối cùng.
[RequireComponent(typeof(SpriteRenderer))]
[DefaultExecutionOrder(300)]
public class TiledBackground : MonoBehaviour
{
    private const int MaxTiles = 30000;      // Chốt an toàn: ô quá nhỏ so với màn hình

    [Tooltip("Kích thước MỘT ô theo đơn vị Unity. 0 = giữ kích thước gốc của sprite (theo Pixels Per Unit)")]
    [SerializeField] private float tileWorldSize = 0f;
    [Tooltip("Số ô lề thêm mỗi phía ngoài vùng nhìn thấy")]
    [SerializeField, Min(1)] private int marginTiles = 1;

    private SpriteRenderer sr;
    private Camera cam;
    private Vector2 nativeSize;              // Kích thước một ô gốc (đơn vị cục bộ của sprite)
    private Vector2 pivot01 = new Vector2(0.5f, 0.5f);
    private float scale = 1f;
    private Vector2 tile;                    // Kích thước một ô trong thế giới = nativeSize x scale

    // Ô đã dựng lần trước: không đổi thì khỏi dựng lại
    private int cellMinX = int.MinValue, cellMinY, countX, countY;
    private bool warned;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        Rebuild();
    }

    // MapLoader gọi khi map muốn đổi nền. Sprite null thì giữ sprite hiện có trong scene
    public void Configure(Sprite sprite, Color color, float tileSize)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (sprite != null) sr.sprite = sprite;
        sr.color = color;
        tileWorldSize = Mathf.Max(0f, tileSize);
        Rebuild();
    }

    // Tính lại kích thước ô khi đổi sprite hoặc tileWorldSize
    private void Rebuild()
    {
        cellMinX = int.MinValue;                          // Buộc dựng lại ở LateUpdate tới

        Sprite sprite = sr.sprite;
        if (sprite == null) return;

        nativeSize = sprite.bounds.size;
        if (nativeSize.x <= 0f || nativeSize.y <= 0f) return;

        pivot01 = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
        scale = tileWorldSize > 0f ? tileWorldSize / nativeSize.x : 1f;
        tile = nativeSize * scale;

        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void LateUpdate()
    {
        if (sr.sprite == null || tile.x <= 0f) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 c = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // Vùng nhìn thấy quy ra chỉ số ô, thêm lề
        int minX = Mathf.FloorToInt((c.x - halfW) / tile.x) - marginTiles;
        int maxX = Mathf.CeilToInt((c.x + halfW) / tile.x) + marginTiles;
        int minY = Mathf.FloorToInt((c.y - halfH) / tile.y) - marginTiles;
        int maxY = Mathf.CeilToInt((c.y + halfH) / tile.y) + marginTiles;

        int nx = maxX - minX;
        int ny = maxY - minY;
        if ((nx & 1) != 0) nx++;                          // Số ô CHẴN: tâm và góc đều nằm trên lưới nên hoa văn không bị lệch
        if ((ny & 1) != 0) ny++;

        if (minX == cellMinX && minY == cellMinY && nx == countX && ny == countY) return;

        if (nx * ny > MaxTiles)
        {
            if (!warned)
            {
                warned = true;
                Debug.LogWarning($"TiledBackground: {nx * ny} ô là quá nhiều. Tăng Tile World Size của map.", this);
            }
            return;
        }

        cellMinX = minX; cellMinY = minY; countX = nx; countY = ny;

        // Kích thước theo đơn vị cục bộ của sprite (scale của transform sẽ nhân thêm)
        sr.size = new Vector2(nx * nativeSize.x, ny * nativeSize.y);

        // Đặt sao cho GÓC DƯỚI TRÁI của vùng phủ nằm đúng trên lưới. Có tính pivot của sprite
        Vector2 worldSize = new Vector2(nx * tile.x, ny * tile.y);
        Vector2 lowerLeft = new Vector2(minX * tile.x, minY * tile.y);
        Vector2 position = lowerLeft + new Vector2(worldSize.x * pivot01.x, worldSize.y * pivot01.y);
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}