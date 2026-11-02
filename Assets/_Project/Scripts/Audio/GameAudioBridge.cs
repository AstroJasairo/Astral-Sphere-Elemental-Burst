using UnityEngine;

// Cầu nối giữa sự kiện của game và âm thanh
public class GameAudioBridge : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private AllOutAssaultSystem allOutAssault;

    private HealthSystem playerHealth;
    private float lastPlayerHp = -1f;

    private static AudioLibrary Lib => AudioManager.Library;

    private void OnEnable()
    {
        EnemyBase.EnemyKilled += OnEnemyKilled;
        Pickup.Collected += OnPickupCollected;
        BossBase.BossSpawned += OnBossSpawned;

        allOutAssault.OnActivated += OnAoaStart;
        allOutAssault.OnEnded += OnAoaEnd;
        gameManager.OnGameOver += OnGameOver;
        gameManager.OnVictory += OnVictory;

        playerHealth = playerStats.Health;               // Thuộc tính lấy lười nên an toàn dù thứ tự khởi tạo thế nào
        playerHealth.OnHealthChanged += OnPlayerHealthChanged;
    }

    private void OnDisable()
    {
        EnemyBase.EnemyKilled -= OnEnemyKilled;
        Pickup.Collected -= OnPickupCollected;
        BossBase.BossSpawned -= OnBossSpawned;

        // Kiểm tra null: khi đóng scene, các object này có thể đã bị hủy trước
        if (allOutAssault != null)
        {
            allOutAssault.OnActivated -= OnAoaStart;
            allOutAssault.OnEnded -= OnAoaEnd;
        }
        if (gameManager != null)
        {
            gameManager.OnGameOver -= OnGameOver;
            gameManager.OnVictory -= OnVictory;
        }
        if (playerHealth != null) playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
    }

    private void Start()
    {
        MapData map = MetaProgress.GetSelectedMap();
        AudioManager.PlayMusic(map != null ? map.music : null);          // Nhạc nền của map
    }

    private void OnEnemyKilled(EnemyBase enemy)
    {
        if (Lib == null) return;
        bool isBoss = enemy.Data != null && enemy.Data.isBoss;
        AudioManager.Play(isBoss ? Lib.bossDeath : Lib.enemyDeath);
    }

    private void OnPickupCollected(PickupType type, int amount)
    {
        if (Lib == null) return;
        switch (type)
        {
            case PickupType.Gold: AudioManager.Play(Lib.pickupGold); break;
            case PickupType.Crystal: AudioManager.Play(Lib.pickupCrystal); break;
            default: AudioManager.Play(Lib.pickupItem); break;            // Mọi vật phẩm hiếm dùng chung một âm
        }
    }

    private void OnBossSpawned(BossBase boss)
    {
        if (Lib != null) AudioManager.Play(Lib.bossAppear);

        // Có nhạc BOSS riêng thì chuyển sang, không thì giữ nhạc của map
        AudioCue bossMusic = boss.Enemy.Data != null ? boss.Enemy.Data.bossMusic : null;
        if (bossMusic != null && bossMusic.HasClip) AudioManager.PlayMusic(bossMusic);
    }

    private void OnAoaStart() { if (Lib != null) AudioManager.Play(Lib.aoaStart); }
    private void OnAoaEnd() { if (Lib != null) AudioManager.Play(Lib.aoaEnd); }

    private void OnGameOver()
    {
        if (Lib != null) AudioManager.Play(Lib.gameOver);
        AudioManager.PlayMusic(null);                                      // Tắt nhạc dần
    }

    private void OnVictory()
    {
        if (Lib != null) AudioManager.Play(Lib.victory);
        AudioManager.PlayMusic(null);
    }

    // Phát tiếng khi máu GIẢM (hồi máu hoặc tăng máu tối đa thì không)
    private void OnPlayerHealthChanged(float current, float max)
    {
        if (lastPlayerHp >= 0f && current < lastPlayerHp && Lib != null)
            AudioManager.Play(Lib.playerHurt);
        lastPlayerHp = current;
    }
}