using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Thanh máu BOSS: ẩn cho tới khi có BOSS xuất hiện
public class BossHealthBarView : MonoBehaviour
{
    [SerializeField] private GameObject root;      // Toàn bộ khối thanh máu (mặc định tắt)
    [SerializeField] private Slider hpBar;
    [SerializeField] private TMP_Text nameText;

    private HealthSystem bossHealth;

    private void OnEnable()
    {
        BossBase.BossSpawned += OnBossSpawned;     // Sự kiện tĩnh nên đăng ký lúc nào cũng an toàn
    }

    private void OnDisable()
    {
        BossBase.BossSpawned -= OnBossSpawned;
        Unbind();                                  // Có kiểm tra null bên trong: BOSS có thể đã bị hủy khi đóng scene
    }

    private void Start()
    {
        root.SetActive(false);
    }

    private void OnBossSpawned(BossBase boss)
    {
        Unbind();
        bossHealth = boss.Health;
        nameText.text = boss.BossName;
        Refresh(bossHealth.CurrentHealth, bossHealth.MaxHealth);
        bossHealth.OnHealthChanged += Refresh;
        bossHealth.OnDied += Hide;
        root.SetActive(true);
    }

    private void Refresh(float current, float max)
    {
        hpBar.value = max > 0f ? current / max : 0f;
    }

    private void Hide()
    {
        root.SetActive(false);
    }

    private void Unbind()
    {
        if (bossHealth == null) return;            // Cũng đúng khi object Unity đã bị hủy
        bossHealth.OnHealthChanged -= Refresh;
        bossHealth.OnDied -= Hide;
        bossHealth = null;
    }
}