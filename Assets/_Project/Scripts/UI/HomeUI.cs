using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Điều hướng trong scene Home: màn hình chính <-> màn hình Character
public class HomeUI : MonoBehaviour
{
    [Header("Màn hình")]
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject characterScreen;

    [Header("Nút ở màn hình chính")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button characterButton;
    [SerializeField] private Button inventoryButton;      // Tương lai: khóa
    [SerializeField] private Button settingsButton;       // Tương lai: khóa
    [SerializeField] private Button quitButton;

    [Header("Màn hình Character")]
    [SerializeField] private Button characterBackButton;
    [SerializeField] private SlidePanel upgradePanel;
    [SerializeField] private SlidePanel sharedPanel;

    private void Start()
    {
        Application.targetFrameRate = 60;

        playButton.onClick.AddListener(SceneNavigator.LoadGame);    // Tạm thời: vào thẳng game. 7C sẽ đổi thành mở màn Map
        characterButton.onClick.AddListener(ShowCharacter);
        quitButton.onClick.AddListener(SceneNavigator.QuitGame);
        characterBackButton.onClick.AddListener(ShowHome);

        inventoryButton.interactable = false;                       // "Sẽ có trong tương lai"
        settingsButton.interactable = false;

        ShowHome();
    }

    private void Update()
    {
        // Nút Back của Android được Input System gán vào phím Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleBack();
    }

    private void ShowHome()
    {
        upgradePanel.CloseImmediate();       // Rời màn hình Character thì đóng mọi bảng
        sharedPanel.CloseImmediate();
        characterScreen.SetActive(false);
        homeScreen.SetActive(true);
    }

    private void ShowCharacter()
    {
        homeScreen.SetActive(false);
        characterScreen.SetActive(true);
    }

    // Back: đóng lớp trên cùng trước. Bảng chung > bảng nhân vật > màn Character > thoát game
    private void HandleBack()
    {
        if (sharedPanel.IsOpen) { sharedPanel.Close(); return; }
        if (upgradePanel.IsOpen) { upgradePanel.Close(); return; }
        if (characterScreen.activeSelf) { ShowHome(); return; }
        SceneNavigator.QuitGame();
    }
}