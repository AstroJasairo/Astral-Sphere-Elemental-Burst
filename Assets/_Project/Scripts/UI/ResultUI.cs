using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResultType { GameOver, Victory }

// Màn kết quả cuối trận (thua hoặc thắng). Gắn vào object GameOverUI và VictoryUI, chọn "Type" tương ứng.
public class ResultUI : MonoBehaviour
{
    [SerializeField] private ResultType type;
    [SerializeField] private GameManager gameManager;   // Kéo vào Inspector (không dùng Instance để tránh lỗi thứ tự khởi tạo)
    [SerializeField] private RunStats runStats;
    [SerializeField] private GameObject panel;          // Mặc định TẮT

    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldText;

    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text bestText;     // Hiện "KỶ LỤC MỚI!" hoặc hạng (tùy chọn)
    [SerializeField] private HomeScreenId backDestination = HomeScreenId.CharacterSelect;   // Nút "quay lại" dẫn tới đâu

    private void OnEnable()
    {
        if (type == ResultType.GameOver) gameManager.OnGameOver += Show;
        else gameManager.OnVictory += Show;
    }

    private void OnDisable()
    {
        if (gameManager == null) return;                // Khi đóng scene, gameManager có thể đã bị hủy
        gameManager.OnGameOver -= Show;
        gameManager.OnVictory -= Show;
    }

    private void Start()
    {
        if (retryButton != null) retryButton.onClick.AddListener(gameManager.Restart);
        //if (homeButton != null) homeButton.onClick.AddListener(SceneNavigator.LoadHome);
        if (homeButton != null) homeButton.onClick.AddListener(GoBack);
        if (quitButton != null) quitButton.onClick.AddListener(SceneNavigator.QuitGame);
    }

    private void Show()
    {
        int sec = (int)gameManager.ElapsedTime;
        // Ghi kết quả vào tiến trình vĩnh viễn: cộng vàng vào ví, cập nhật bảng điểm
        RunSubmitResult submit = MetaProgress.SubmitRun(new RunResult
        {
            score = runStats.Score,
            timeSeconds = sec,
            kills = runStats.Kills,
            gold = runStats.Gold,
            victory = type == ResultType.Victory
        });
        timeText.text = $"Thời gian: {sec / 60:00}:{sec % 60:00}";
        killsText.text = $"Hạ gục: {runStats.Kills}";
        scoreText.text = $"Điểm: {runStats.Score}";
        goldText.text = $"Vàng: +{submit.goldEarned}  (Ví: {MetaProgress.Gold})";
        if (bestText != null)
            bestText.text = submit.isNewBest ? "KỶ LỤC MỚI!" : (submit.rank > 0 ? $"Hạng {submit.rank}" : "");
        panel.SetActive(true);
    }

    private void GoBack()
    {
        if (backDestination == HomeScreenId.CharacterSelect) SceneNavigator.LoadCharacterSelect();
        else SceneNavigator.LoadHome();
    }
}