using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform player;
    [Tooltip("CHỈ ĐỂ TEST: bắt đầu trận từ giây thứ N (ví dụ 1500 = 25:00)")]
    [SerializeField] private float startTimeOffset = 0f;

    public Transform Player => player;
    public float ElapsedTime { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsVictory { get; private set; }
    public bool IsEnded => IsGameOver || IsVictory;   // Trận đã kết thúc (thắng hoặc thua)

    public event Action OnGameOver;
    public event Action OnVictory;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        ElapsedTime = startTimeOffset;
        EnemyBase.ResetStatics();   // Dọn biến static còn sót từ trận trước
        Telegraph.ResetStatics();
    }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    // PlayerStats gọi khi người chơi chết
    public void TriggerGameOver()
    {
        if (IsEnded) return;   // Chỉ xử lý một lần
        IsGameOver = true;
        Pause();                  // Dừng toàn bộ game
        OnGameOver?.Invoke();
    }

    public void Restart()
    {
        Resume();   // BẮT BUỘC trả timeScale về 1 trước khi nạp lại, nếu không trận mới sẽ đứng yên
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TriggerVictory()
    {
        if (IsEnded) return;  // Nếu người chơi đã chết rồi thì không thắng nữa (và ngược lại)
        IsVictory = true;
        Pause();
        OnVictory?.Invoke();
    }
}