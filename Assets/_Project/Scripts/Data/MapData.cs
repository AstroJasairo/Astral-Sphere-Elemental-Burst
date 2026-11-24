using UnityEngine;

// Dữ liệu một map. Tạo bằng: Assets > Create > Astral Sphere > Map Data
[CreateAssetMenu(menuName = "Astral Sphere/Map Data")]
public class MapData : ScriptableObject
{
    public string id;                      // Duy nhất, dùng để lưu xếp hạng. Ví dụ "forest". KHÔNG đổi sau khi phát hành
    public string displayName;             // Tên hiển thị: "Rừng"
    public Sprite background;              // Hình nền thẻ (trống thì hiện màu xám)
    public string sceneName = "Game";      // Scene sẽ nạp khi chơi map này (phải có trong Build Profiles)
    public bool isAvailable = true;        // false: hiện "Sắp ra mắt", chưa chơi được

    [Header("Cấu hình trận đấu của map")]
    public StageData stage;                    // Bảng spawn, độ khó theo thời gian, thời điểm BOSS
    public EnemyData boss;                     // BOSS của map (ThunderBirdData, LordOfTheWolfData...)
    public bool overrideBackground = false;    // Bật để map này đổi nền (tắt thì giữ nền của scene)
    public Sprite gameBackground;              // Hình nền (tùy chọn, dùng khi nền là một SpriteRenderer)
    public Color backgroundColor = Color.white;

    public const float MinBoundsSize = 20f;       // Nhỏ hơn thế thì camera (6.8 x 12) gần như phủ hết bản đồ

    [Header("Độ khó")]
    [Tooltip("Kỹ năng BOSS dùng cấp này (1 đến 3). Cấp vượt quá số cấp kỹ năng có thì dùng cấp cao nhất")]
    [Range(1, 3)] public int difficulty = 1;

    [Header("Biên bản đồ (hình chữ nhật, tâm tại (0,0))")]
    [Tooltip("Kích thước khung biên theo đơn vị Unity. Đặt 0 để KHÔNG có biên")]
    public Vector2 boundsSize = new Vector2(40f, 40f);
    public Color boundsBorderColor = new Color(1f, 1f, 1f, 0.9f);
    public float boundsBorderWidth = 0.25f;
    [Tooltip("Prefab bố cục (tường, vật cản...) sinh vào scene khi vào trận. Để trống nếu chưa cần")]
    public GameObject layoutPrefab;

    [Header("Nền")]
    [Tooltip("Kích thước MỘT ô nền (đơn vị Unity). 0 = giữ kích thước gốc của sprite (theo Pixels Per Unit)")]
    public float tileWorldSize = 0f;

    [Header("Âm thanh")]
    public AudioCue music = new AudioCue();           // Nhạc nền của map (lặp lại)

    private void OnValidate()
    {
        if (boundsSize.x > 0f || boundsSize.y > 0f)       // Có biên thì không nhỏ hơn mức tối thiểu
            boundsSize = new Vector2(Mathf.Max(boundsSize.x, MinBoundsSize), Mathf.Max(boundsSize.y, MinBoundsSize));

        tileWorldSize = Mathf.Max(0f, tileWorldSize);
        boundsBorderWidth = Mathf.Max(0f, boundsBorderWidth);
    }
}