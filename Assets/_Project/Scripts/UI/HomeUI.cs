using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Điều hướng trong scene Home: Home, Map, Chọn nhân vật, Character
public class HomeUI : MonoBehaviour
{
    [Header("Màn hình")]
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject mapScreen;
    [SerializeField] private GameObject characterSelectScreen;
    [SerializeField] private GameObject characterScreen;

    [Header("Nút ở màn hình chính")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button characterButton;
    [SerializeField] private Button inventoryButton;      // Tương lai: khóa
    [SerializeField] private Button settingsButton;       // Tương lai: khóa
    [SerializeField] private Button quitButton;

    [Header("Màn hình Map")]
    [SerializeField] private Button mapBackButton;
    [SerializeField] private MapWheelCarousel mapWheel;

    [Header("Màn hình chọn nhân vật")]
    [SerializeField] private Button selectBackButton;

    [Header("Màn hình Character")]
    [SerializeField] private Button characterBackButton;
    [SerializeField] private SlidePanel upgradePanel;
    [SerializeField] private SlidePanel sharedPanel;

    [Header("Hộp thoại xếp hạng (nằm ngoài các màn hình, vẽ đè lên trên)")]
    [SerializeField] private LeaderboardPopup leaderboard;

    private void Start()
    {
        Application.targetFrameRate = 60;

        playButton.onClick.AddListener(() => Show(mapScreen));
        characterButton.onClick.AddListener(() => Show(characterScreen));
        quitButton.onClick.AddListener(SceneNavigator.QuitGame);
        mapBackButton.onClick.AddListener(() => Show(homeScreen));
        selectBackButton.onClick.AddListener(() => Show(mapScreen));
        characterBackButton.onClick.AddListener(() => Show(homeScreen));

        inventoryButton.interactable = false;
        settingsButton.interactable = false;

        mapWheel.MapChosen += OnMapChosen;
        mapWheel.RankingRequested += leaderboard.Show;

        // Vào Home từ màn hình kết quả có thể yêu cầu mở thẳng màn hình chọn nhân vật
        switch (SceneNavigator.ConsumePendingHomeScreen())
        {
            case HomeScreenId.CharacterSelect:
                Show(characterSelectScreen);
                break;
            default:
                Show(homeScreen);
                break;
        }
    }

    private void OnDestroy()
    {
        // Kiểm tra null: khi đóng scene các object có thể đã bị hủy theo thứ tự bất kỳ
        if (mapWheel != null) mapWheel.MapChosen -= OnMapChosen;
        if (mapWheel != null && leaderboard != null) mapWheel.RankingRequested -= leaderboard.Show;
    }

    private void Update()
    {
        // Nút Back của Android được Input System gán vào phím Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleBack();
    }

    private void OnMapChosen(MapData map)
    {
        MetaProgress.SelectMap(map.id);       // Lưu map đã chọn (dùng cho xếp hạng và nạp scene)
        Show(characterSelectScreen);
    }

    // Bật đúng một màn hình, đóng mọi bảng và hộp thoại đang mở
    private void Show(GameObject screen)
    {
        upgradePanel.CloseImmediate();
        sharedPanel.CloseImmediate();
        leaderboard.Close();

        homeScreen.SetActive(screen == homeScreen);
        mapScreen.SetActive(screen == mapScreen);
        characterSelectScreen.SetActive(screen == characterSelectScreen);
        characterScreen.SetActive(screen == characterScreen);
    }

    // Back: đóng lớp trên cùng trước
    private void HandleBack()
    {
        if (leaderboard.IsOpen) { leaderboard.Close(); return; }
        if (sharedPanel.IsOpen) { sharedPanel.Close(); return; }
        if (upgradePanel.IsOpen) { upgradePanel.Close(); return; }
        if (characterSelectScreen.activeSelf) { Show(mapScreen); return; }
        if (mapScreen.activeSelf || characterScreen.activeSelf) { Show(homeScreen); return; }
        SceneNavigator.QuitGame();
    }
}