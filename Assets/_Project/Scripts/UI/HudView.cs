using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudView : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider crystalBar;
    [SerializeField] private Slider hpBar;

    [SerializeField] private CrystalLevelSystem levelSystem;
    [SerializeField] private RunStats runStats;
    [SerializeField] private PlayerStats playerStats;

    private int lastSecond = -1;
    private HealthSystem playerHealth;   // Lưu sẵn để OnDisable không phải gọi GetComponent nữa

    private void OnEnable()
    {
        levelSystem.OnChanged += RefreshLevel;
        runStats.OnChanged += RefreshRun;
        // Fix: Lỗi Player bị hủy trước HUD nên không lấy được dữ liệu
        playerHealth = playerStats.Health;            // Lấy MỘT LẦN, khi mọi thứ còn sống
        playerStats.Health.OnHealthChanged += RefreshHp;
    }

    private void OnDisable()
    {
        // Kiểm tra null trước khi hủy đăng ký: nếu object đã bị hủy thì bỏ qua,
        // vì chính nó cũng đã mất nên không còn gì để rò rỉ.
        if (levelSystem != null) levelSystem.OnChanged -= RefreshLevel;
        if (runStats != null) runStats.OnChanged -= RefreshRun;
        if (playerHealth != null) playerHealth.OnHealthChanged -= RefreshHp;
    }

    private void Start()
    {
        RefreshLevel();
        RefreshRun();
        // Fix: Lấy giá trị đã lưu thay vì truy cập phần tử đã bị hủy
        RefreshHp(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void Update()
    {
        // Chỉ đổi chữ khi sang giây mới (1 lần/giây) để không tạo chuỗi mới mỗi frame
        int sec = (int)GameManager.Instance.ElapsedTime;
        if (sec == lastSecond) return;
        lastSecond = sec;
        timerText.text = $"{sec / 60:00}:{sec % 60:00}";
    }

    private void RefreshLevel()
    {
        levelText.text = $"{levelSystem.Level}";
        // Thanh pha lê chỉ hiển thị tiến độ trong cấp hiện tại
        int have = levelSystem.Crystals - levelSystem.PreviousThreshold;
        int need = levelSystem.NextThreshold - levelSystem.PreviousThreshold;
        crystalBar.value = Mathf.Clamp01((float)have / need);
    }

    private void RefreshRun()
    {
        goldText.text = runStats.Gold.ToString();
        scoreText.text = runStats.Score.ToString();
    }

    private void RefreshHp(float current, float max)
    {
        hpBar.value = current / max;
    }
}