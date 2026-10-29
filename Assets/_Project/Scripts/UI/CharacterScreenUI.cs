using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Màn hình Character: lưới 6 chỉ số + nút mở hai bảng nâng cấp
public class CharacterScreenUI : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;

    [Header("Lưới chỉ số (mỗi ô một TMP_Text)")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text defText;
    [SerializeField] private TMP_Text spdText;
    [SerializeField] private TMP_Text magText;
    [SerializeField] private TMP_Text manaText;

    [Header("Nút và bảng")]
    [SerializeField] private Button upgradeButton;            // Nút xanh lá ở đáy
    [SerializeField] private Button sharedButton;             // Nút đỏ ở góc trên bên phải
    [SerializeField] private Button upgradeHandleButton;      // Tay nắm để đóng bảng nâng cấp
    [SerializeField] private Button sharedCloseButton;        // Mũi tên đóng bảng nâng cấp chung
    [SerializeField] private SlidePanel upgradePanel;
    [SerializeField] private SlidePanel sharedPanel;

    private void Start()
    {
        upgradeButton.onClick.AddListener(upgradePanel.Open);
        sharedButton.onClick.AddListener(sharedPanel.Open);
        upgradeHandleButton.onClick.AddListener(upgradePanel.Close);
        sharedCloseButton.onClick.AddListener(sharedPanel.Close);
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        CharacterData c = carousel.Current;
        if (c == null) return;

        CharacterStatSheet s = CharacterStatSheet.Build(c);
        hpText.text = $"HP\n<b>{s.maxHp:0.##}</b>";
        atkText.text = $"ATK\n<b>{s.atk:0.00}</b>";
        defText.text = $"DEF\n<b>{s.def:0.##}</b>";
        spdText.text = $"SPD\n<b>{s.moveSpeed:0.##}</b>";
        magText.text = $"MAG\n<b>{s.magnetRadius:0.##}</b>";
        manaText.text = $"MAX MANA\n<b>{s.maxMana:0.#}</b>";
    }
}