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
}