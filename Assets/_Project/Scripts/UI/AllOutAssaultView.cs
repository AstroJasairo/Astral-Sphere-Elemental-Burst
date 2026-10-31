using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hiển thị thanh mana, lớp phủ và hiệu ứng khi đang All Out Assault, theo MÀU CHÍNH của nhân vật đang chơi
public class AllOutAssaultView : MonoBehaviour
{
    [SerializeField] private AllOutAssaultSystem system;
    [SerializeField] private PlayerStats playerStats;          // Để lấy màu chính của nhân vật
    [SerializeField] private Slider manaBar;

    [Header("Lớp phủ khi đang hiệu lực")]
    [SerializeField] private GameObject activeOverlay;         // Object chứa lớp phủ và chữ banner
    [SerializeField] private Image overlayImage;               // Image của lớp phủ (sẽ được tô màu chính)
    [SerializeField, Range(0f, 1f)] private float overlayAlpha = 0.2f;
    [SerializeField] private TMP_Text bannerText;              // Chữ "ALL OUT ASSAULT!" (tùy chọn)

    [Header("Nhân vật")]
    [SerializeField] private SpriteRenderer playerSprite;      // Tùy chọn
    [SerializeField, Range(0f, 1f)] private float playerTintStrength = 0.6f;   // 0 = giữ màu gốc, 1 = đổi hẳn sang màu chính

    [Header("Màu")]
    [SerializeField] private Color chargingColor = new Color(0.3f, 0.6f, 1f);          // Thanh mana khi đang nạp
    [SerializeField] private Color activeColor = new Color(1f, 0.75f, 0.1f);           // Dự phòng khi thiếu PlayerStats

    private Image manaFill;
    private Color playerNormalColor = Color.white;

    private void Awake()
    {
        manaFill = manaBar.fillRect.GetComponent<Image>();     // Hình phần "đầy" của Slider
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
        Color primary = playerStats != null ? playerStats.PrimaryColor : activeColor;

        manaFill.color = primary;                              // Thanh mana

        if (overlayImage != null)                              // Lớp phủ màn hình: màu chính nhưng giữ độ mờ riêng
        {
            Color c = primary;
            c.a = overlayAlpha;
            overlayImage.color = c;
        }
        if (bannerText != null) bannerText.color = primary;    // Chữ banner

        if (playerSprite != null)                              // Nhân vật: pha giữa màu gốc và màu chính
            playerSprite.color = Color.Lerp(playerNormalColor, primary, playerTintStrength);

        activeOverlay.SetActive(true);
    }

    private void ShowCharging()
    {
        manaFill.color = chargingColor;
        activeOverlay.SetActive(false);
        if (playerSprite != null) playerSprite.color = playerNormalColor;
    }
}