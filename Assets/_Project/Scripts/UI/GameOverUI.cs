using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;   // Kéo vào Inspector (không dùng Instance để tránh lỗi thứ tự khởi tạo)
    [SerializeField] private RunStats runStats;
    [SerializeField] private GameObject panel;          // Mặc định TẮT
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button retryButton;

    private void OnEnable() { gameManager.OnGameOver += Show; }
    private void OnDisable() { gameManager.OnGameOver -= Show; }

    private void Start()
    {
        retryButton.onClick.AddListener(gameManager.Restart);
    }

    private void Show()
    {
        int sec = (int)gameManager.ElapsedTime;
        timeText.text = $"Sống sót: {sec / 60:00}:{sec % 60:00}";
        killsText.text = $"Hạ gục: {runStats.Kills}";
        scoreText.text = $"Điểm: {runStats.Score}";
        goldText.text = $"Vàng: {runStats.Gold}";
        panel.SetActive(true);
    }
}