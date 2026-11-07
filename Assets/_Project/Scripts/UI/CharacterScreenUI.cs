using UnityEngine;
using UnityEngine.UI;

// Màn hình Character: lưới 6 chỉ số (mỗi ô có icon + chữ) + nút mở hai bảng nâng cấp
public class CharacterScreenUI : MonoBehaviour
{
    [SerializeField] private CharacterCarouselView carousel;

    [Header("Lưới chỉ số (mỗi ô một StatCellView: icon + chữ)")]
    [SerializeField] private StatCellView hpCell;
    [SerializeField] private StatCellView atkCell;
    [SerializeField] private StatCellView defCell;
    [SerializeField] private StatCellView spdCell;
    [SerializeField] private StatCellView magCell;
    [SerializeField] private StatCellView manaCell;

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

        // Mỗi ô: icon lấy từ StatIcons (một nguồn chung), chữ giữ định dạng cũ
        hpCell.Set(StatIcons.Get(StatType.Hp), $"HP\n<b>{s.maxHp:0.##}</b>");
        atkCell.Set(StatIcons.Get(StatType.Atk), $"ATK\n<b>{s.atk:0.00}</b>");
        defCell.Set(StatIcons.Get(StatType.Def), $"DEF\n<b>{s.def:0.##}</b>");
        spdCell.Set(StatIcons.Get(StatType.Spd), $"SPD\n<b>{s.moveSpeed:0.##}</b>");
        magCell.Set(StatIcons.Get(StatType.Mag), $"MAG\n<b>{s.magnetRadius:0.##}</b>");
        manaCell.Set(StatIcons.Get(StatType.Mp), $"MAX MANA\n<b>{s.maxMana:0.#}</b>");
    }
}