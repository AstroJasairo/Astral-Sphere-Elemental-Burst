using System.Collections.Generic;
using UnityEngine;

// Thanh buff/debuff: sinh icon khi có buff, trả về kho khi hết. Số lượng icon không cố định.
public class BuffHudView : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private BuffCatalog catalog;
    [SerializeField] private BuffIconView iconPrefab;
    [SerializeField] private Transform container;                  // Nơi chứa icon (có Grid/Horizontal Layout Group)

    // active[i] = icon đang hiển thị cho loại buff thứ i (null nếu buff đó không chạy)
    private readonly BuffIconView[] active = new BuffIconView[BuffController.TypeCount];
    private readonly Stack<BuffIconView> pool = new Stack<BuffIconView>();   // Kho icon rảnh

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
    }

    // Lấy icon từ kho (hết thì tạo mới) và cấu hình theo loại buff
    private BuffIconView Acquire(BuffType type)
    {
        BuffIconView icon = pool.Count > 0 ? pool.Pop() : Instantiate(iconPrefab, container);
        icon.gameObject.SetActive(true);
        icon.transform.SetAsLastSibling();                         // Buff mới nằm cuối hàng

        Sprite overrideIcon = GetIconOverride(type);
        BuffVisual visual = catalog != null ? catalog.Get(type) : null;
        if (visual != null) icon.Setup(visual, catalog.BorderColor(visual.isDebuff));
        else icon.SetupFallback(type.ToString(), catalog != null ? catalog.buffBorderColor : Color.green);
        return icon;
    }

    // Icon thay thế: buff của skill chủ động dùng icon trong skill data (null = dùng icon/nhãn của BuffCatalog)
    private Sprite GetIconOverride(BuffType type)
    {
        if (type != BuffType.ActiveSkill) return null;
        SkillSystem skills = stats.Skills;
        return (skills != null && skills.ActiveData != null) ? skills.ActiveData.icon : null;
    }

    private void Release(BuffIconView icon)
    {
        icon.gameObject.SetActive(false);
        pool.Push(icon);
    }
}