using System.Collections.Generic;
using UnityEngine;

// Thanh buff/debuff: sinh icon khi có buff, trả về kho khi hết. Số lượng icon không cố định.
// Bốn nhóm icon:
//   - Buff/debuff thường (viền xanh lá / đỏ): một icon cho mỗi BuffType
//   - DOT (viền đỏ): một icon cho mỗi DotInstance, kèm ô số stack
//   - Hiệu lực skill (viền XANH DƯƠNG): skill chủ động đang chạy, passive có hiệu lực
//   - Hồi chiêu passive (viền TÍM): passive đang hồi chiêu
public class BuffHudView : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private BuffCatalog catalog;
    [SerializeField] private BuffIconView iconPrefab;
    [SerializeField] private Transform container;                  // Nơi chứa icon (có Grid/Horizontal Layout Group)

    [Header("Passive đính kèm vũ khí")]
    [Tooltip("Hiện icon hiệu lực (xanh dương) của passive vũ khí")]
    [SerializeField] private bool showWeaponPassiveEffects = true;
    [Tooltip("Hiện icon hồi chiêu (tím) của passive vũ khí. Mặc định tắt: chỉ passive nhân vật hiện hồi chiêu")]
    [SerializeField] private bool showWeaponPassiveCooldown = false;

    // active[i] = icon đang hiển thị cho loại buff thứ i (null nếu buff đó không chạy)
    private readonly BuffIconView[] active = new BuffIconView[BuffController.TypeCount];
    private readonly Stack<BuffIconView> pool = new Stack<BuffIconView>();       // Kho icon rảnh
    private readonly Dictionary<DotInstance, BuffIconView> dotIcons = new Dictionary<DotInstance, BuffIconView>();

    // Icon của skill: khóa = đối tượng effect (so sánh theo tham chiếu, không sinh rác)
    private readonly Dictionary<PassiveSkillEffect, BuffIconView> effectIcons = new Dictionary<PassiveSkillEffect, BuffIconView>();
    private readonly Dictionary<PassiveSkillEffect, BuffIconView> cooldownIcons = new Dictionary<PassiveSkillEffect, BuffIconView>();
    private BuffIconView activeEffectIcon;                         // Icon hiệu lực của skill chủ động

    private PlayerDebuffSystem debuffs;

    private Color EffectColor => catalog != null ? catalog.skillEffectBorderColor : new Color(0.25f, 0.55f, 1f);
    private Color CooldownColor => catalog != null ? catalog.skillCooldownBorderColor : new Color(0.65f, 0.35f, 0.95f);

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
        UpdateBuffIcons();
        UpdateDotIcons();
        UpdateSkillIcons();
    }

    // ---------------- Buff/debuff thường ----------------

    private void UpdateBuffIcons()
    {
        BuffController buffs = stats.Buffs;

        for (int i = 0; i < active.Length; i++)
        {
            // ActiveSkill là kênh tính ATK của skill chủ động: icon của skill hiện ở kênh xanh dương nên ẩn icon này
            if ((BuffType)i == BuffType.ActiveSkill) continue;

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

    private BuffIconView Acquire(BuffType type)
    {
        BuffIconView icon = TakeIcon();

        BuffVisual visual = catalog != null ? catalog.Get(type) : null;
        if (visual != null) icon.Setup(visual, catalog.BorderColor(visual.isDebuff));
        else icon.SetupFallback(type.ToString(), catalog != null ? catalog.buffBorderColor : Color.green);
        return icon;
    }

    // ---------------- DOT ----------------

    // DOT: chỉ cập nhật vòng thời gian và số stack (icon sinh/trả theo sự kiện bên dưới)
    private void UpdateDotIcons()
    {
        foreach (KeyValuePair<DotInstance, BuffIconView> pair in dotIcons)
        {
            pair.Value.SetRemaining(pair.Key.Remaining01);
            pair.Value.SetStacks(pair.Key.stacks);
        }
    }

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

    // ---------------- Skill: hiệu lực (xanh dương) và hồi chiêu passive (tím) ----------------

    private void UpdateSkillIcons()
    {
        // Skill của nhân vật: chủ động + passive
        SkillSystem skills = stats.Skills;
        if (skills != null)
        {
            UpdateActiveEffectIcon(skills);
            for (int i = 0; i < skills.PassiveCount; i++)
                UpdatePassiveIcons(skills.GetPassiveEffect(i), skills.GetPassiveData(i), true);
        }

        // Passive gắn trên vũ khí
        WeaponInventory inventory = stats.Weapons;
        if (inventory != null)
        {
            for (int i = 0; i < inventory.PassiveCount; i++)
                UpdatePassiveIcons(inventory.GetPassiveEffect(i), inventory.GetPassiveData(i), false);
        }
    }

    // Skill chủ động đang hiệu lực: một icon xanh dương, dù skill có buff chỉ số hay không
    private void UpdateActiveEffectIcon(SkillSystem skills)
    {
        bool show = skills.HasActive && skills.IsEffectActive;

        if (show)
        {
            if (activeEffectIcon == null)
            {
                activeEffectIcon = TakeIcon();
                SetupSkillIcon(activeEffectIcon, skills.ActiveData, EffectColor);
            }
            activeEffectIcon.SetRemaining(skills.EffectRemaining01);
        }
        else if (activeEffectIcon != null)
        {
            Release(activeEffectIcon);
            activeEffectIcon = null;
        }
    }

    private void UpdatePassiveIcons(PassiveSkillEffect effect, PassiveSkillData data, bool isCharacterSkill)
    {
        bool showEffect = (isCharacterSkill || showWeaponPassiveEffects) && effect.IsEffectShown;
        bool showCooldown = (isCharacterSkill || showWeaponPassiveCooldown) && effect.CooldownRemaining01 > 0f;

        SyncIcon(effectIcons, effect, data, showEffect, EffectColor,
                 effect.EffectRemaining01, effect.EffectUsesTimer, effect.EffectStacks);

        SyncIcon(cooldownIcons, effect, data, showCooldown, CooldownColor,
                 effect.CooldownRemaining01, true, 0);
    }

    // Sinh icon khi cần hiện, trả về kho khi hết. Mỗi frame chỉ cập nhật vòng thời gian và số stack.
    private void SyncIcon(Dictionary<PassiveSkillEffect, BuffIconView> icons, PassiveSkillEffect effect, PassiveSkillData data,
                          bool show, Color border, float remaining01, bool usesTimer, int stacks)
    {
        icons.TryGetValue(effect, out BuffIconView icon);

        if (show)
        {
            if (icon == null)
            {
                icon = TakeIcon();
                SetupSkillIcon(icon, data, border);
                icons[effect] = icon;
            }

            if (usesTimer) icon.SetRemaining(remaining01);
            else icon.SetNoTimer();
            icon.SetStacks(stacks);
        }
        else if (icon != null)
        {
            icons.Remove(effect);
            Release(icon);
        }
    }

    // Icon của skill: dùng hình trong SkillData. Chưa có hình thì hiện 3 chữ cái đầu của tên skill.
    private static void SetupSkillIcon(BuffIconView icon, SkillDataBase data, Color border)
    {
        string label = string.IsNullOrEmpty(data.skillName)
            ? "SKL"
            : (data.skillName.Length > 3 ? data.skillName.Substring(0, 3) : data.skillName).ToUpper();

        icon.SetupFallback(label, border, data.icon);
    }

    // ---------------- Kho icon ----------------

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