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
}