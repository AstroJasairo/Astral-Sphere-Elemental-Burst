using UnityEngine;
using UnityEngine.SceneManagement;

// Chuyển cảnh và thoát game. Tên scene phải khớp với tên file scene và có trong Build Profiles.
public static class SceneNavigator
{
    public const string HomeScene = "Home";
    public const string GameScene = "Game";

    public static void LoadHome() { Load(HomeScene); }
    public static void LoadGame() { Load(GameScene); }

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