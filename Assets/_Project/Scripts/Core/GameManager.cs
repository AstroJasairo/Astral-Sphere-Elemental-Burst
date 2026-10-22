using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform player;
    [Tooltip("CHỈ ĐỂ TEST: bắt đầu trận từ giây thứ N (ví dụ 1500 = 25:00) để thử độ khó cao")]
    [SerializeField] private float startTimeOffset = 0f;

    public Transform Player => player;
    public float ElapsedTime { get; private set; }   // Thời gian đã chơi (giây)

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;   // Mặc định mobile có thể chỉ 30 FPS, nên ta khóa 60
        ElapsedTime = startTimeOffset;
    }

    private void Update()
    {
        // Time.deltaTime tự về 0 khi Time.timeScale = 0 (pause), nên đồng hồ tự dừng theo
        ElapsedTime += Time.deltaTime;
    }
}