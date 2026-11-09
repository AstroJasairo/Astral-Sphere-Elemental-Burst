using TMPro;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

// Nút kích hoạt skill chủ động trong trận: icon, hồi chiêu (vòng tối + số giây), viền sáng khi đang hiệu lực
public class SkillButtonView : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;            // Image Filled, Radial 360, màu đen mờ
    [SerializeField] private TMP_Text cooldownText;            // Tùy chọn: thêm sau cũng được
    [SerializeField] private Image glowImage;                  // GlowRing: dùng Mat_SkillBorderGlow

    [Header("Hiệu ứng viền")]
    [SerializeField] private float bandSpeed = 0.7f;           // Số vòng quanh viền mỗi giây
    [SerializeField] private float bandWidth = 0.18f;
    [SerializeField] private float borderWidth = 0.06f;
    [SerializeField] private float softness = 0.03f;

    private static readonly int PhaseId = Shader.PropertyToID("_Phase");
    private static readonly int ColorId = Shader.PropertyToID("_BandColor");
    private static readonly int BandWidthId = Shader.PropertyToID("_BandWidth");
    private static readonly int BorderWidthId = Shader.PropertyToID("_BorderWidth");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
    private static readonly int UVRectId = Shader.PropertyToID("_UVRect");

    private SkillSystem skills;
    private Material glowMaterial;
    private bool initialized, glowVisible;
    private int lastShownSecond = -1;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
        glowImage.enabled = false;
        cooldownOverlay.enabled = false;
        if (cooldownText != null) cooldownText.enabled = false;
    }

    private void OnDestroy()
    {
        if (glowMaterial != null) Destroy(glowMaterial);        // Material tạo bằng code phải tự hủy
    }

    private void OnClick()
    {
        if (skills != null) skills.TryActivate();
    }

    // Khởi tạo ở Update đầu tiên: lúc đó mọi Start (kể cả SkillSystem.Start nạp skill) đã chạy xong
    private bool TryInit()
    {
        skills = stats.Skills;
        if (skills == null || !skills.HasActive)                // Nhân vật chưa có skill chủ động: ẩn nút
        {
            gameObject.SetActive(false);
            return false;
        }

        Sprite icon = skills.ActiveData.icon;
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;                       // Chưa có icon thì chỉ thấy Background

        // Mỗi nút dùng bản sao material riêng để đặt màu theo nhân vật
        glowMaterial = new Material(glowImage.material);
        glowImage.material = glowMaterial;
        if (!glowMaterial.HasProperty(PhaseId))
            Debug.LogWarning("SkillButtonView: GlowRing chưa dùng Mat_SkillBorderGlow (thiếu thuộc tính _Phase).", this);

        glowMaterial.SetColor(ColorId, stats.PrimaryColor);     // Màu chính của nhân vật đang chơi
        glowMaterial.SetFloat(BandWidthId, bandWidth);
        glowMaterial.SetFloat(BorderWidthId, borderWidth);
        glowMaterial.SetFloat(SoftnessId, softness);

        // Chuẩn hóa UV: không sprite => cả khung (0,0,1,1); có sprite từ sheet Multiple => vùng UV của sprite đó
        Vector4 uvRect = glowImage.sprite != null ? DataUtility.GetOuterUV(glowImage.sprite) : new Vector4(0f, 0f, 1f, 1f);
        glowMaterial.SetVector(UVRectId, uvRect);

        initialized = true;
        return true;
    }

    private void Update()
    {
        if (!initialized && !TryInit()) return;

        // ---- Viền sáng: chỉ khi đang hiệu lực và hồi chiêu tính SAU khi hết hiệu lực ----
        bool showGlow = skills.IsEffectActive && skills.ActiveData.cooldownStart == CooldownStart.AfterEffectEnds;
        if (showGlow != glowVisible)
        {
            glowVisible = showGlow;
            glowImage.enabled = showGlow;
        }
        if (showGlow)   // Time.time bị đóng băng khi tạm dừng nên dải sáng cũng dừng theo
            glowMaterial.SetFloat(PhaseId, Mathf.Repeat(Time.time * bandSpeed, 1f));

        // ---- Hồi chiêu: vòng tối + số giây ----
        bool cooling = skills.IsCooldownRunning;
        cooldownOverlay.enabled = cooling;
        if (cooling)
        {
            cooldownOverlay.fillAmount = skills.CooldownRemaining01;     // đầy lúc bắt đầu, vơi dần về 0
            if (cooldownText != null)
            {
                cooldownText.enabled = true;
                int seconds = Mathf.CeilToInt(skills.CooldownRemaining);
                if (seconds != lastShownSecond)                           // chỉ đổi chữ khi sang giây mới
                {
                    lastShownSecond = seconds;
                    cooldownText.text = seconds.ToString();
                }
            }
        }
        else
        {
            lastShownSecond = -1;
            if (cooldownText != null) cooldownText.enabled = false;
        }

        button.interactable = skills.IsReady;
    }
}