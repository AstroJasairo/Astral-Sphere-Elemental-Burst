using UnityEngine;
using UnityEngine.UI;

// Hiển thị thanh mana, lớp phủ và hiệu ứng khi đang All Out Assault
public class AllOutAssaultView : MonoBehaviour
{
    [SerializeField] private AllOutAssaultSystem system;
    [SerializeField] private PlayerStats playerStats;     // Để lấy màu chính của nhân vật đang dùng
    [SerializeField] private Slider manaBar;
    [SerializeField] private GameObject activeOverlay;      // Lớp phủ màn hình + chữ "ALL OUT ASSAULT!"
    [SerializeField] private SpriteRenderer playerSprite;   // Tùy chọn: đổi màu nhân vật khi đang hiệu lực
    [SerializeField] private Color chargingColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color activeColor = new Color(1f, 0.75f, 0.1f);
    [SerializeField] private Color playerActiveColor = new Color(1f, 0.9f, 0.5f);

    private Image manaFill;
    private Color playerNormalColor = Color.white;

    private void Awake()
    {
        manaFill = manaBar.fillRect.GetComponent<Image>();   // Hình phần "đầy" của Slider
        if (playerSprite != null) playerNormalColor = playerSprite.color;
    }

    private void OnEnable()
    {
        system.OnProgressChanged += Refresh;
        system.OnActivated += ShowActive;
        system.OnEnded += ShowCharging;
    }

    private void OnDisable()
    {
        // Kiểm tra null: khi đóng scene, system có thể đã bị hủy trước view này
        if (system == null) return;
        system.OnProgressChanged -= Refresh;
        system.OnActivated -= ShowActive;
        system.OnEnded -= ShowCharging;
    }

    private void Start()
    {
        ShowCharging();
        Refresh(0f);
    }

    private void Refresh(float normalized)
    {
        manaBar.value = normalized;
    }

    private void ShowActive()
    {
        // Thanh mana chuyển sang MÀU CHÍNH của nhân vật
        manaFill.color = playerStats != null ? playerStats.PrimaryColor : activeColor;

        activeOverlay.SetActive(true);
        if (playerSprite != null) playerSprite.color = playerActiveColor;
    }

    private void ShowCharging()
    {
        manaFill.color = chargingColor;
        activeOverlay.SetActive(false);
        if (playerSprite != null) playerSprite.color = playerNormalColor;
    }
}