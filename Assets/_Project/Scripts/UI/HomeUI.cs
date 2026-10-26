using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        Application.targetFrameRate = 60;
        playButton.onClick.AddListener(SceneNavigator.LoadGame);
        quitButton.onClick.AddListener(SceneNavigator.QuitGame);
    }

    private void Update()
    {
        // Nút Back của Android được Input System gán vào phím Escape: nhấn thì thoát game
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SceneNavigator.QuitGame();
    }
}