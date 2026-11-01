using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Menu tạm dừng trong trận. Mở bằng nút trên màn hình, nút Back của Android (phím Escape),
// hoặc tự động khi ứng dụng bị đẩy xuống nền.
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;     // Kéo vào Inspector (không dùng Instance để tránh lỗi thứ tự khởi tạo)
    [SerializeField] private RunStats runStats;
    [SerializeField] private GameObject panel;            // Mặc định TẮT

    [SerializeField] private Button pauseButton;          // Nút nhỏ trên HUD
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button characterSelectButton;
    [SerializeField] private Button quitButton;

    private bool isOpen;
    private bool started;

    private void Start()
    {
        pauseButton.onClick.AddListener(Open);
        resumeButton.onClick.AddListener(Close);
        restartButton.onClick.AddListener(Restart);
        characterSelectButton.onClick.AddListener(LeaveToCharacterSelect);
        quitButton.onClick.AddListener(QuitGame);

        panel.SetActive(false);
        started = true;
    }

    private void Update()
    {
        // Nút Back của Android được Input System gán vào phím Escape
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (isOpen) Close();
        else Open();
    }

    // Người chơi chuyển sang ứng dụng khác hoặc khóa màn hình: tự tạm dừng
    private void OnApplicationPause(bool paused)
    {
        if (paused) Open();
    }

    private void Open()
    {
        if (!started || isOpen) return;
        // Đã kết thúc trận, hoặc game đang tạm dừng vì lý do khác (màn chọn nâng cấp): không mở chồng
        if (gameManager.IsEnded || gameManager.IsPaused) return;

        isOpen = true;
        panel.SetActive(true);
        gameManager.Pause();
    }

    private void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        panel.SetActive(false);
        gameManager.Resume();
    }

    // Rời trận giữa chừng: vàng đã nhặt được giữ lại, điểm không được ghi
    private void Restart()
    {
        MetaProgress.BankGold(runStats.Gold);
        gameManager.Restart();
    }

    private void LeaveToCharacterSelect()
    {
        MetaProgress.BankGold(runStats.Gold);
        SceneNavigator.LoadCharacterSelect();
    }

    private void QuitGame()
    {
        MetaProgress.BankGold(runStats.Gold);
        SceneNavigator.QuitGame();
    }
}