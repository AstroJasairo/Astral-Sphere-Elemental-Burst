using UnityEngine;

// Một thanh kiếm Clarius: rơi xuống, cắm trên sân kèm aura gây sát thương, rồi mờ dần.
// Mỗi thanh kiếm tự quản lý vòng đời của mình, nên nhiều thanh cùng tồn tại được.
public class ClariusSword : MonoBehaviour
{
    private enum Phase { Falling, Active, Fading }

    [Header("Hình ảnh")]
    [SerializeField] private Transform visual;                  // Object con chứa hình thanh kiếm; mũi kiếm nằm ở gốc (pivot)
    [SerializeField] private SpriteRenderer swordRenderer;
    [SerializeField] private Material auraMaterial;             // Dùng Mat_Telegraph
    [SerializeField] private Color auraFill = new Color(1f, 0.35f, 0.1f, 0.22f);
    [SerializeField] private Color auraBorder = new Color(1f, 0.55f, 0.15f, 0.95f);
    [SerializeField] private int auraSortingOrder = 1;

    [Header("Hoạt ảnh")]
    [SerializeField] private float fallHeight = 7f;             // Độ cao bắt đầu rơi (đơn vị Unity)
    [SerializeField] private float fallTime = 0.22f;            // Thời gian rơi
    [SerializeField] private float fadeTime = 0.3f;             // Thời gian mờ dần lúc biến mất
    [SerializeField] private float pulseDecay = 6f;             // Tốc độ tắt dần của nhịp sáng mỗi lần gây sát thương

    private ClariusWeapon weapon;
    private TelegraphVisual aura;
    private readonly Vector2[] points = new Vector2[TelegraphGeometry.MaxPoints];

    private Phase phase;
    private float phaseTimer;
    private float lifeLeft;
    private float nextTick;
    private float shownRadius;
    private float pulse;

    private void Awake()
    {
        // Aura vẽ bằng lưới và viền tạo bằng code, cùng hệ thống với telegraph
        aura = new TelegraphVisual(transform, "Aura", auraMaterial, auraSortingOrder, auraFill, auraBorder, 0.08f);
    }

    private void OnDestroy()
    {
        aura?.Destroy();
    }

    // ClariusWeapon gọi mỗi lần thả kiếm
    public void Begin(ClariusWeapon owner, float duration)
    {
        weapon = owner;
        lifeLeft = duration;
        phase = Phase.Falling;
        phaseTimer = 0f;
        shownRadius = -1f;
        pulse = 0f;

        visual.localPosition = new Vector3(0f, fallHeight, 0f);   // Bắt đầu ở trên cao
        swordRenderer.color = Color.white;
        aura.Show(false);                                          // Aura chỉ hiện sau khi kiếm cắm xuống
    }

    private void Update()
    {
        if (weapon == null) return;

        float dt = Time.deltaTime;                                 // Dừng khi game tạm dừng

        switch (phase)
        {
            case Phase.Falling:
                UpdateFalling(dt);
                break;
            case Phase.Active:
                UpdateActive(dt);
                break;
            case Phase.Fading:
                UpdateFading(dt);
                break;
        }
    }

    private void UpdateFalling(float dt)
    {
        phaseTimer += dt;
        float t = Mathf.Clamp01(phaseTimer / fallTime);

        // t x t: rơi nhanh dần như rơi tự do. Chỉ di chuyển phần hình, tâm của object (và aura) đứng yên tại điểm đích.
        visual.localPosition = new Vector3(0f, fallHeight * (1f - t * t), 0f);

        if (t >= 1f) Land();
    }

    // Kiếm cắm xuống đất: bắt đầu tồn tại, gây sát thương lần đầu ngay lúc chạm
    private void Land()
    {
        phase = Phase.Active;
        visual.localPosition = Vector3.zero;

        weapon.PlayImpact(transform.position);
        weapon.DamageArea(transform.position);
        nextTick = Time.time + weapon.CurrentTickInterval;
        pulse = 1f;

        aura.Show(true);
        RefreshAuraShape(true);
    }

    private void UpdateActive(float dt)
    {
        lifeLeft -= dt;
        RefreshAuraShape(false);                                   // Bán kính có thể đổi giữa chừng (All Out Assault)

        // Gây sát thương theo nhịp. Khoảng cách giữa hai nhịp đọc lại từ vũ khí nên All Out Assault làm đánh dày hơn.
        if (Time.time >= nextTick)
        {
            weapon.DamageArea(transform.position);
            nextTick = Time.time + weapon.CurrentTickInterval;
            pulse = 1f;                                            // Aura chớp sáng mỗi lần gây sát thương
        }

        pulse = Mathf.Max(0f, pulse - pulseDecay * dt);
        aura.SetAlpha(0.6f + 0.4f * pulse);

        if (lifeLeft <= 0f)
        {
            phase = Phase.Fading;
            phaseTimer = 0f;
        }
    }

    private void UpdateFading(float dt)
    {
        phaseTimer += dt;
        float fade = 1f - Mathf.Clamp01(phaseTimer / fadeTime);

        aura.SetAlpha(0.6f * fade);
        swordRenderer.color = new Color(1f, 1f, 1f, fade);

        if (phaseTimer >= fadeTime)
            PoolManager.Instance.Release(gameObject);
    }

    // Vẽ lại hình aura khi bán kính thay đổi (chỉ khi cần)
    private void RefreshAuraShape(bool force)
    {
        float radius = weapon.CurrentRadius;
        if (!force && Mathf.Approximately(radius, shownRadius)) return;

        shownRadius = radius;
        int n = TelegraphGeometry.Sector(points, radius, 360f);
        aura.SetPolygon(points, n, Vector2.zero);
    }
}