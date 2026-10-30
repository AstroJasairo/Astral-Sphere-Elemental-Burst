using UnityEngine;
using UnityEngine.SceneManagement;

// Màn hình mà scene Home cần mở ngay khi nạp xong
public enum HomeScreenId { Home, CharacterSelect }

// Chuyển cảnh và thoát game. Tên scene phải khớp với tên file scene và có trong Build Profiles.
public static class SceneNavigator
{
    public const string HomeScene = "Home";
    public const string GameScene = "Game";

    // Màn hình cần mở khi vào scene Home (HomeUI đọc một lần rồi xóa)
    public static HomeScreenId PendingHomeScreen { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        PendingHomeScreen = HomeScreenId.Home;
    }

    public static void LoadHome()
    {
        PendingHomeScreen = HomeScreenId.Home;
        Load(HomeScene);
    }

    // Về scene Home và mở thẳng màn hình chọn nhân vật (dùng cho nút ở màn hình kết quả)
    public static void LoadCharacterSelect()
    {
        PendingHomeScreen = HomeScreenId.CharacterSelect;
        Load(HomeScene);
    }

    public static void LoadGame() { Load(GameScene); }
    public static void LoadScene(string sceneName) { Load(sceneName); }

    // Trả về màn hình đang chờ rồi đặt lại về Home, nên lần nạp sau không bị mở nhầm
    public static HomeScreenId ConsumePendingHomeScreen()
    {
        HomeScreenId screen = PendingHomeScreen;
        PendingHomeScreen = HomeScreenId.Home;
        return screen;
    }

    private static void Load(string sceneName)
    {
        Time.timeScale = 1f;   // Trước đó game có thể đang tạm dừng (màn kết quả): phải trả về 1, nếu không cảnh mới đứng yên
        SceneManager.LoadScene(sceneName);
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // Trong Editor: dừng chế độ Play
#else
        Application.Quit();                                // Trên điện thoại: đóng ứng dụng
#endif
    }
}