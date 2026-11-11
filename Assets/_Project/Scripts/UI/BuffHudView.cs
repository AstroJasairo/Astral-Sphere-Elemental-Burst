using System.Collections.Generic;
using UnityEngine;

// Thanh buff/debuff: sinh icon khi có buff, trả về kho khi hết. Số lượng icon không cố định.
// Buff/debuff thường: một icon cho mỗi BuffType. DOT: một icon cho mỗi DotInstance (kèm ô số stack).
public class BuffHudView : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private BuffCatalog catalog;
    [SerializeField] private BuffIconView iconPrefab;
    [SerializeField] private Transform container;                  // Nơi chứa icon (có Grid/Horizontal Layout Group)

    // active[i] = icon đang hiển thị cho loại buff thứ i (null nếu buff đó không chạy)
    private readonly BuffIconView[] active = new BuffIconView[BuffController.TypeCount];
    private readonly Stack<BuffIconView> pool = new Stack<BuffIconView>();       // Kho icon rảnh
    private readonly Dictionary<DotInstance, BuffIconView> dotIcons = new Dictionary<DotInstance, BuffIconView>();
    private PlayerDebuffSystem debuffs;

    private void OnEnable()
    {
        debuffs = stats.Debuffs;
        debuffs.OnDotAdded += HandleDotAdded;
        debuffs.OnDotRemoved += HandleDotRemoved;
    }

    private void OnDisable()
    {
        if (debuffs != null)                                       // kiểm tra null: object có thể đã bị hủy khi đóng scene
        {
            debuffs.OnDotAdded -= HandleDotAdded;
            debuffs.OnDotRemoved -= HandleDotRemoved;
        }
    }

    private void Update()
    {
        BuffController buffs = stats.Buffs;

        for (int i = 0; i < active.Length; i++)
        {
            float remaining = buffs.GetRemaining01((BuffType)i);
            BuffIconView icon = active[i];

            if (remaining > 0f)
            {
                if (icon == null)                                  // Buff vừa bắt đầu: lấy một icon
                {
                    icon = Acquire((BuffType)i);
                    active[i] = icon;
                }
                icon.SetRemaining(remaining);
            }
            else if (icon != null)                                 // Buff vừa hết: trả icon về kho
            {
                Release(icon);
                active[i] = null;
            }
        }

        // DOT: chỉ cập nhật vòng thời gian và số stack (icon sinh/trả theo sự kiện bên dưới)
        foreach (KeyValuePair<DotInstance, BuffIconView> pair in dotIcons)
        {
            pair.Value.SetRemaining(pair.Key.Remaining01);
            pair.Value.SetStacks(pair.Key.stacks);
        }
    }

    // ---------------- DOT ----------------

    private void HandleDotAdded(DotInstance dot)
    {
        BuffIconView icon = TakeIcon();
        Color border = catalog != null ? catalog.debuffBorderColor : Color.red;
        icon.SetupFallback(dot.data.shortLabel, border, dot.data.icon);   // DOT luôn là debuff: viền đỏ
        dotIcons[dot] = icon;
    }

    private void HandleDotRemoved(DotInstance dot)
    {
        if (!dotIcons.TryGetValue(dot, out BuffIconView icon)) return;
        dotIcons.Remove(dot);
        Release(icon);
    }

    // ---------------- Buff/debuff thường ----------------

    // Icon thay thế: buff của skill chủ động dùng icon trong skill data (null = dùng icon/nhãn của BuffCatalog)
    private Sprite GetIconOverride(BuffType type)
    {
        if (type != BuffType.ActiveSkill) return null;
        SkillSystem skills = stats.Skills;
        return (skills != null && skills.ActiveData != null) ? skills.ActiveData.icon : null;
    }

    private BuffIconView Acquire(BuffType type)
    {
        BuffIconView icon = TakeIcon();

        Sprite overrideIcon = GetIconOverride(type);
        BuffVisual visual = catalog != null ? catalog.Get(type) : null;
        if (visual != null) icon.Setup(visual, catalog.BorderColor(visual.isDebuff), overrideIcon);
        else icon.SetupFallback(type.ToString(), catalog != null ? catalog.buffBorderColor : Color.green, overrideIcon);
        return icon;
    }

    private BuffIconView TakeIcon()
    {
        BuffIconView icon = pool.Count > 0 ? pool.Pop() : Instantiate(iconPrefab, container);
        icon.gameObject.SetActive(true);
        icon.transform.SetAsLastSibling();                         // Buff mới nằm cuối hàng
        return icon;
    }

    private void Release(BuffIconView icon)
    {
        icon.gameObject.SetActive(false);
        pool.Push(icon);
    }
}