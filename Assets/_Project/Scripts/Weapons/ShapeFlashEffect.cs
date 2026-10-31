using UnityEngine;

// Hiệu ứng chớp hình quạt/tròn hoặc chữ nhật rồi mờ dần, sau đó tự trả về pool.
// Hướng của object = hướng của hình (trục +X). Gắn vào prefab hiệu ứng.
public class ShapeFlashEffect : MonoBehaviour
{
    [SerializeField] private Material material;      // Dùng Mat_Telegraph
    [SerializeField] private Color color = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private int sortingOrder = 5;   // Vẽ phía trên nền và quái

    private TelegraphVisual visual;
    private readonly Vector2[] points = new Vector2[TelegraphGeometry.MaxPoints];
    private float timer;

    private void Awake()
    {
        visual = new TelegraphVisual(transform, "Flash", material, sortingOrder, color, color, 0.05f);
    }

    private void OnDestroy()
    {
        visual?.Destroy();
    }

    // Hình tròn (angleDeg = 360) hoặc hình quạt
    public void PlaySector(float radius, float angleDeg)
    {
        int n = TelegraphGeometry.Sector(points, radius, angleDeg);
        visual.SetPolygon(points, n, Vector2.zero);
        Begin();
    }

    // Hình chữ nhật từ gốc đi ra phía trước (dùng cho tia)
    public void PlayBox(float length, float width)
    {
        int n = TelegraphGeometry.Box(points, 0f, length, -width * 0.5f, width * 0.5f);
        visual.SetPolygon(points, n, TelegraphGeometry.Centroid(points, n));
        Begin();
    }

    private void Begin()
    {
        timer = duration;
        visual.Show(true);
        visual.SetAlpha(1f);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PoolManager.Instance.Release(gameObject);
            return;
        }
        visual.SetAlpha(timer / duration);               // Mờ dần theo thời gian còn lại
    }
}