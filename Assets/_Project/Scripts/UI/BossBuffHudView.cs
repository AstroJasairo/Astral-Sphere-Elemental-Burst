using UnityEngine;

// Thanh buff của BOSS (nằm dưới thanh máu BOSS), dùng lại BuffIconView của người chơi.
// Hiện: icon Miễn nhiễm (KHÔNG có đồng hồ) và icon Giảm sát thương (có đồng hồ, kèm số lớp khi chồng từ 2).
// LƯU Ý: gắn lên object LUÔN BẬT (cùng chỗ với BossHealthBarView), KHÔNG gắn lên BossInfo vì BossInfo tắt tới khi có BOSS.
public class BossBuffHudView : MonoBehaviour
{
    [SerializeField] private BuffCatalog catalog;
    [SerializeField] private BuffIconView iconPrefab;
    [SerializeField] private Transform container;          // BossBuffs: có GridLayoutGroup, nằm trong BossInfo

    private BuffIconView immuneIcon, reductionIcon;
    private BossBase boss;
    private BossDamageReduction reduction;

    private void OnEnable() { BossBase.BossSpawned += OnBossSpawned; }

    private void OnDisable()
    {
        BossBase.BossSpawned -= OnBossSpawned;
        Unbind();                                           // Có kiểm tra null: BOSS có thể đã bị hủy khi đóng scene
    }

    private void OnBossSpawned(BossBase newBoss)
    {
        Unbind();
        boss = newBoss;
        reduction = newBoss.GetComponent<BossDamageReduction>();    // Có thể null nếu BOSS không có giảm sát thương

        boss.ImmunityChanged += HandleImmunityChanged;
        HandleImmunityChanged(boss.IsImmune);
        if (reductionIcon != null) reductionIcon.gameObject.SetActive(false);
    }

    private void Unbind()
    {
        if (boss != null) boss.ImmunityChanged -= HandleImmunityChanged;
        boss = null;
        reduction = null;
    }

    // Miễn nhiễm: icon KHÔNG có đồng hồ, chỉ bật/tắt theo trạng thái
    private void HandleImmunityChanged(bool immune)
    {
        if (immune || immuneIcon != null)
        {
            BuffIconView icon = GetIcon(ref immuneIcon, BuffType.Immunity);
            icon.SetNoTimer();
            icon.gameObject.SetActive(immune);
        }
    }

    private void Update()
    {
        if (reduction == null) return;

        int stacks = reduction.Stacks;
        if (stacks > 0)
        {
            BuffIconView icon = GetIcon(ref reductionIcon, BuffType.DamageReduction);
            if (!icon.gameObject.activeSelf) icon.gameObject.SetActive(true);
            icon.SetRemaining(reduction.Remaining01);
            icon.SetStacks(stacks);                          // Chỉ hiện ô số khi từ 2 lớp trở lên
        }
        else if (reductionIcon != null && reductionIcon.gameObject.activeSelf)
        {
            reductionIcon.gameObject.SetActive(false);
        }
    }

    // Tạo icon lần đầu cần dùng (hai icon cố định, không cần kho)
    private BuffIconView GetIcon(ref BuffIconView icon, BuffType type)
    {
        if (icon != null) return icon;

        icon = Instantiate(iconPrefab, container);
        BuffVisual visual = catalog != null ? catalog.Get(type) : null;
        if (visual != null) icon.Setup(visual, catalog.BorderColor(visual.isDebuff));
        else icon.SetupFallback(type.ToString(), Color.green);
        return icon;
    }
}