using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(HealthSystem))]
public class PlayerStats : MonoBehaviour, ISkillOwner
{
    [Header("Nhân vật (Chỉ dùng khi chạy thẳng scene Game để test)")]
    [SerializeField] private CharacterData character;      // Dự phòng khi chưa có MetaConfig
    [SerializeField] private bool useDebugStarLevel;       // Bật để dùng "Star Level" bên dưới thay cho sao đã lưu
    [SerializeField] private bool debugInvincible;         // CHỈ ĐỂ TEST HIỆU NĂNG: người chơi không bao giờ chết
    [SerializeField, Range(0, 5)] private int starLevel = 0;
    [SerializeField] private bool logMeta;                 // Bật để in bonus meta ra Console

    // Chỉ số gốc: nạp từ CharacterData (giá trị dưới đây chỉ là dự phòng khi chưa gán)
    private float baseMaxHp = 100f;
    private float baseAtk = 1f;
    private float baseDef = 0f;
    private float baseMoveSpeed = 5f;
    private float baseMagnetRadius = 1.5f;
    public const float MinManaCostMultiplier = 0.2f;      // Mana cần không bao giờ giảm quá 80%
    private float orbitSpeedMultiplier = 1f;
    private float orbitHitIntervalMultiplier = 1f;

    // ----- Màu sắc của nhân vật đang dùng -----
    public Color PrimaryColor => character != null ? character.primaryColor : new Color(1f, 0.75f, 0.1f);
    public Color SecondaryColor => character != null ? character.secondaryColor : Color.white;

    // Bonus nhận được TRONG trận
    private float hpBonus;                 // TỔNG HP cộng thêm đã nhận (kể cả khi đang bị chuyển đổi sang hồi mana)
    private float appliedHpBonus;          // MỚI 12D: phần HP cộng thêm đang THỰC SỰ nằm trong máu tối đa
    private float defBonus;
    private float atkBonusPct, speedBonusPct, magnetBonusPct;
    private float atkFlatBonus;            // MỚI 12D: ATK cộng THẲNG (hiện chưa có nguồn nào gọi AddAtkFlat)
    private float manaCostReductionPct;
    private ElementMask elementImmunity;

    public float BaseMaxMana { get; private set; } = 100f;     // Mana gốc của nhân vật (CharacterData.maxMana)
    public bool IsImmuneTo(ElementType element) { return ElementMaskUtil.Contains(elementImmunity, element); }
    private float aoaAtkPct, aoaSpeedPct;        // Buff All Out Assault (tạm thời, kênh riêng)
    private float aoaCooldownMultiplier = 1f;    // 1 = bình thường, 0.25 = hồi chiêu còn 25%

    // ----- MỚI 12D: Brave -----
    private int braveStacks;
    private float braveAtkPct, braveDefPct;      // Đã nhân sẵn với số stack

    // ----- MỚI 12D: Liên kết linh hồn -----
    private bool soulLinkOn;
    private float soulLinkAtkEff, soulLinkHpRate, soulLinkDefRate, soulLinkSpdRate;

    private float Debuff(BuffType type) { return Mathf.Clamp01(Buffs.GetValue(type)); }
    private readonly List<StarPerk> activePerks = new List<StarPerk>();

    public CharacterData Character => character;
    public Transform Transform => transform;                  // ISkillOwner

    // ATK/tốc độ "cộng thêm" dạng phần trăm: gồm nâng cấp trong trận, AOA, thuốc, Huyết nộ, Brave
    public float AddedAtkPct => atkBonusPct + aoaAtkPct + Buffs.GetValue(BuffType.Rage)
                                + Buffs.GetValue(BuffType.ActiveSkill) + braveAtkPct;
    public float AddedSpeedPct => speedBonusPct + aoaSpeedPct + Buffs.GetValue(BuffType.Speed);

    public bool SoulLinkActive => soulLinkOn;

    // Máu tối đa vĩnh viễn. Khi Liên kết linh hồn bật, HP cộng thêm KHÔNG nằm trong đây
    public float MaxHp => baseMaxHp + appliedHpBonus;

    // Liên kết linh hồn bật: nhân vật không nhận ATK cộng thêm (chuyển sang vũ khí, xem WeaponAtkMultiplier)
    public float Atk => (soulLinkOn ? baseAtk : (baseAtk + atkFlatBonus) * (1f + AddedAtkPct))
                        * (1f - Debuff(BuffType.Exhaust));

    // Hai hệ số WeaponBase dùng khi tính sát thương cơ bản của vũ khí
    public float WeaponAtkMultiplier => soulLinkOn ? 1f + AddedAtkPct * soulLinkAtkEff : 1f;
    public float WeaponFlatDamage => soulLinkOn ? atkFlatBonus * soulLinkAtkEff : 0f;

    // MỚI 12E: DEF cộng thêm của Brave = % của (DEF gốc + DEF nâng cấp trong trận)
    private float BraveDefPoints => (baseDef + defBonus) * braveDefPct;

    // DEF cộng thêm = defBonus (nâng cấp) + Brave. Liên kết linh hồn bật: chuyển sang hồi mana
    public float Def => (baseDef + (soulLinkOn ? 0f : defBonus + BraveDefPoints)) * (1f - Debuff(BuffType.Vulnerable));

    public float MoveSpeed
    {
        get
        {
            if (IsRooted || IsStunned) return 0f;                              // Trói và Choáng: không di chuyển
            return baseMoveSpeed * (1f + (soulLinkOn ? 0f : AddedSpeedPct))
                   * (1f - Debuff(BuffType.Slow));
        }
    }
    public float MagnetRadius => baseMagnetRadius * (1f + magnetBonusPct + Buffs.GetValue(BuffType.Magnet))
                                 * (1f - Debuff(BuffType.Blind));
    public float ManaCostMultiplier => Mathf.Max(MinManaCostMultiplier, 1f - manaCostReductionPct);
    public float CooldownMultiplier => aoaCooldownMultiplier;
    public float ScoreMultiplier => 1f + Buffs.GetValue(BuffType.ScoreBoost);
    public float OrbitSpeedMultiplier => orbitSpeedMultiplier;
    public float OrbitHitIntervalMultiplier => orbitHitIntervalMultiplier;

    public bool IsStunned => Buffs.IsActive(BuffType.Stun);
    public bool IsRooted => Buffs.IsActive(BuffType.Root);
    public bool IsSilenced => Buffs.IsActive(BuffType.Silence);
    public float AttackSpeedMultiplier => Mathf.Max(0.1f, 1f - Debuff(BuffType.Frostbite));   // Tê cóng: 0.5 = tốc đánh còn một nửa

    // Hiệu quả hồi mana = (nguồn thường + Liên kết linh hồn, gộp theo CombatRules.manaGainStacking) x (1 - Suy giảm mana)
    public float ManaGainMultiplier => ManaGainBase * (1f - Debuff(BuffType.ManaDrain));

    private float ManaGainBase
    {
        get
        {
            float m = ManaGainBonuses.Multiplier;
            float bonus = SoulLinkManaBonus;
            if (bonus <= 0f) return m;
            return CombatRules.Config.manaGainStacking == StackMode.Additive ? m + bonus : m * (1f + bonus);
        }
    }

    // Hiệu quả hồi mana nhận từ HP/DEF/SPD cộng thêm (0.2 = +20%). Tính lại mỗi lần đọc vì buff tạm thời hết hạn.
    public float SoulLinkManaBonus
    {
        get
        {
            if (!soulLinkOn) return 0f;
            float defPoints = defBonus + BraveDefPoints;                      // DEF % của Brave tính theo tổng DEF
            float speedPoints = baseMoveSpeed * AddedSpeedPct;            // SPD % tính theo tốc độ gốc
            return hpBonus * soulLinkHpRate + defPoints * soulLinkDefRate + speedPoints * soulLinkSpdRate;
        }
    }

    public float Shield { get; private set; }

    // Các nguồn giảm sát thương và hiệu quả hồi phục (passive, buff... tự đăng ký vào đây)
    public DamageBonusSet DamageBonuses { get; } = new DamageBonusSet();
    public ManaGainBonusSet ManaGainBonuses { get; } = new ManaGainBonusSet();
    public DamageReductionSet Reductions { get; } = new DamageReductionSet();
    public HealingBonusSet HealingBonuses { get; } = new HealingBonusSet();
    public event Action<float, float> OnShieldChanged;   // (lá chắn hiện tại, tối đa) -> HUD lắng nghe
    public event Action<float> OnDamaged;     // MỚI 12E: phát SAU khi một đòn đã trừ máu/khiên (tham số = sát thương đã tính DEF và giảm)

    // MỚI 13C: Đoạt mệnh nghe. CombatHit báo sau mỗi đòn trúng quái
    public event Action<EnemyBase> OnEnemyHit;
    public bool HasEnemyHitListeners => OnEnemyHit != null;
    public void NotifyEnemyHit(EnemyBase enemy) { OnEnemyHit?.Invoke(enemy); }

    // ISkillOwner: đặt miễn nhiễm của một nguồn rồi xóa các hiệu ứng đang dính mà giờ được miễn nhiễm
    public void SetImmunity(object source, ImmunityFlags flags)
    {
        Health.Immunities.Set(source, flags);
        Debuffs.PurgeImmune(Health.Immunities.Flags);
    }

    public IReadOnlyList<StarPerk> ActivePerks => activePerks;

    // AllOutAssaultSystem (giai đoạn sau) sẽ bật/tắt biến này để kích hoạt perk 5 sao
    public bool AllOutAssaultActive { get; private set; }

    // Thuộc tính lấy lười: dùng lúc nào cũng an toàn, bất kể thứ tự Awake/OnEnable
    private HealthSystem health;
    public HealthSystem Health
    {
        get
        {
            if (health == null && this != null) health = GetComponent<HealthSystem>();
            return health;
        }
    }

    // Kho vũ khí (lấy lười: null nếu chưa gắn)
    private WeaponInventory weaponInventory;
    public WeaponInventory Weapons
    {
        get
        {
            if (weaponInventory == null) TryGetComponent(out weaponInventory);
            return weaponInventory;
        }
    }

    private SkillSystem skills;
    public SkillSystem Skills
    {
        get
        {
            if (skills == null) TryGetComponent(out skills);
            return skills;
        }
    }

    private BuffController buffs;
    public BuffController Buffs
    {
        get
        {
            if (buffs == null && !TryGetComponent(out buffs))
                buffs = gameObject.AddComponent<BuffController>();
            return buffs;
        }
    }

    private PlayerDebuffSystem debuffs;
    public PlayerDebuffSystem Debuffs
    {
        get
        {
            if (debuffs == null && !TryGetComponent(out debuffs))
                debuffs = gameObject.AddComponent<PlayerDebuffSystem>();
            return debuffs;
        }
    }

    private void Awake()
    {
        LoadCharacter();
        Health.Init(MaxHp);
        Health.OnDied += HandleDied;
    }

    private void LoadCharacter()
    {
        // Nhân vật được chọn ở Home (lưu trong file). Chạy thẳng scene Game mà chưa có MetaConfig thì dùng nhân vật gán tay.
        CharacterData selected = MetaProgress.GetSelectedCharacter();
        if (selected != null) character = selected;

        if (character == null)
        {
            Debug.LogWarning("PlayerStats: chưa có CharacterData, đang dùng chỉ số dự phòng.", this);
            return;
        }

        bool useDebug = useDebugStarLevel && Debug.isDebugBuild;
        int stars = useDebug ? starLevel : MetaProgress.GetStars(character.id);

        CharacterStatSheet sheet = CharacterStatSheet.Build(character);
        baseMaxHp = sheet.maxHp;
        baseAtk = sheet.atk;
        baseDef = sheet.def;
        baseMoveSpeed = sheet.moveSpeed;
        baseMagnetRadius = sheet.magnetRadius;
        manaCostReductionPct = sheet.manaCostReduction;    // Nâng MP trong trận sẽ cộng dồn thêm vào đây

        BaseMaxMana = character.maxMana;

        // Kháng nguyên tố cơ bản = một nguồn giảm sát thương theo nguyên tố (cùng hệ thống với Kháng hỏa của skill)
        Reductions.Remove(this);
        foreach (ElementType e in ElementMaskUtil.Basic)
        {
            float resist = character.elementResist.GetClamped(e);
            if (!Mathf.Approximately(resist, 0f)) Reductions.Set(this, (int)e, resist);
        }
        elementImmunity = character.elementImmunity;

        activePerks.Clear();
        activePerks.AddRange(character.GetActivePerks(stars));

        if (logMeta)
            Debug.Log($"[Meta] {character.id} sao {stars} | HP {baseMaxHp}, ATK {baseAtk:0.00}, DEF {baseDef}, " +
                      $"SPD {baseMoveSpeed:0.00}, MAG {baseMagnetRadius:0.00}, giảm mana {manaCostReductionPct:P0}");
    }

    public void ApplyStat(StatType stat, float amount)
    {
        switch (stat)
        {
            case StatType.Hp:
                hpBonus += amount;                                  // Luôn ghi nhận (để chuyển đổi sang hồi mana nếu cần)
                if (!soulLinkOn)                                    // Liên kết linh hồn bật: không tăng máu tối đa
                {
                    appliedHpBonus += amount;
                    Health.IncreaseMax(amount);
                }
                break;
            case StatType.Mp: manaCostReductionPct += amount; break;
            case StatType.Atk: atkBonusPct += amount; break;
            case StatType.Def: defBonus += amount; break;
            case StatType.Spd: speedBonusPct += amount; break;
            case StatType.Mag: magnetBonusPct += amount; break;
        }
    }

    // MỚI 12D: ATK cộng THẲNG (ví dụ +1 ATK). Liên kết linh hồn bật thì chuyển sang sát thương cơ bản vũ khí x hiệu quả
    public void AddAtkFlat(float amount) { atkFlatBonus += amount; }

    // MỚI 12D: BravePassiveEffect gọi. stacks = số kẻ địch; atkPct, defPct đã nhân sẵn với số stack
    public void SetBrave(int stacks, float atkPct, float defPct)
    {
        braveStacks = stacks;
        braveAtkPct = atkPct;
        braveDefPct = defPct;
    }
    public int BraveStacks => braveStacks;

    // MỚI 12D: SoulLinkPassiveEffect gọi (Enable/Disable). Gọi nhiều lần vẫn an toàn.
    public void SetSoulLink(bool on, float atkEfficiency, float hpRate, float defRate, float spdRate)
    {
        soulLinkAtkEff = atkEfficiency;
        soulLinkHpRate = hpRate;
        soulLinkDefRate = defRate;
        soulLinkSpdRate = spdRate;

        bool active = on && CombatRules.Config.soulLinkEnabled;     // Công tắc trong CombatRules
        if (active == soulLinkOn) return;                           // Trạng thái không đổi: chỉ cập nhật hệ số ở trên
        soulLinkOn = active;

        // Đồng bộ máu tối đa: bật thì thu HP cộng thêm về 0, tắt thì trả lại toàn bộ
        float target = active ? 0f : hpBonus;
        float delta = target - appliedHpBonus;
        appliedHpBonus = target;
        if (!Mathf.Approximately(delta, 0f)) Health.ShiftMax(delta);

        OnShieldChanged?.Invoke(Shield, MaxHp);                     // HUD vẽ lại thanh khiên theo máu tối đa mới
    }

    // Cộng dồn bonus sát thương của nguyên tố từ các perk đang hoạt động
    public float GetDamageBonus(ElementType element)
    {
        float bonus = 0f;
        foreach (StarPerk perk in activePerks)   // foreach trên List không sinh rác
        {
            if (perk.element != element) continue;

            if (perk.type == PerkType.ElementDamageBonus)
                bonus += perk.bonus;
            else if (perk.type == PerkType.AllOutAssaultDamageBonus && AllOutAssaultActive)
                bonus += perk.bonus;   // Chỉ tính khi đang trong All Out Assault
        }
        // Bonus nguyên tố từ perk + bonus từ passive (nhóm "tất cả" và nhóm nguyên tố). Exhaust giảm tất cả
        return (bonus + DamageBonuses.GetTotalBonus(element)) * (1f - Debuff(BuffType.Exhaust));
    }

    // Sát thương thực nhận = số gốc x (1 - giảm sát thương) x 100/(100+DEF). Pure bỏ qua cả hai.
    public float CalculateIncomingDamage(float amount, ElementType element)
    {
        if (element == ElementType.Pure) return amount;
        if (IsImmuneTo(element)) return 0f;                       // Miễn nhiễm hoàn toàn (khiên cũng không bị tiêu)
        Reductions.EffectScale = 1f - Debuff(BuffType.Vulnerable);
        float reduction = Reductions.GetTotalReduction(element);
        return amount * (1f - reduction) * 100f / (100f + Def);
    }

    public void TakeDamage(float amount, ElementType element, DamageKind kind = DamageKind.Normal)
    {
        if (debugInvincible && Debug.isDebugBuild) return;
        if (Health.Invulnerable) return;                                                    // Bất tử: khiên cũng không bị tiêu
        if (kind == DamageKind.Normal && Health.Immunities.Has(ImmunityFlags.Damage)) return;   // Miễn nhiễm sát thương (DOT cũng đi qua đây)

        float damage = CalculateIncomingDamage(amount, element);
        float incoming = damage;                           // Nhớ lại để phát sự kiện

        if (Shield > 0f)                                   // Khiên hấp thụ trước, kể cả Pure
        {
            float absorbed = Mathf.Min(Shield, damage);
            Shield -= absorbed;
            damage -= absorbed;
            OnShieldChanged?.Invoke(Shield, MaxHp);
        }

        if (damage > 0f) Health.TakeDamage(damage);        // Phần dư mới trừ vào máu

        if (incoming > 0f) OnDamaged?.Invoke(incoming);
    }

    private void HandleDied()
    {
        GameManager.Instance.TriggerGameOver();
    }

    // AllOutAssaultSystem gọi khi bật/tắt
    public void SetAllOutAssault(bool active, float atkPct, float speedPct, float cooldownMultiplier)
    {
        AllOutAssaultActive = active;     // Bật perk 5 sao trong GetDamageBonus
        aoaAtkPct = active ? atkPct : 0f;
        aoaSpeedPct = active ? speedPct : 0f;
        aoaCooldownMultiplier = active ? cooldownMultiplier : 1f;   // Tắt thì trả về đúng 1, không "trừ ngược"
    }

    public void AddShield(float amount)
    {
        Shield = Mathf.Min(Shield + amount, Health.MaxHealth);      // Giới hạn theo máu tối đa hiệu lực
        OnShieldChanged?.Invoke(Shield, MaxHp);
    }

    // MỌI nguồn hồi máu phải đi qua hàm này để được nhân "hiệu quả hồi phục"
    public void ReceiveHealing(float amount)
    {
        if (amount <= 0f) return;
        Health.Heal(amount * HealingBonuses.Multiplier);
    }

    public void SetOrbitModifiers(float speedMultiplier, float hitIntervalMultiplier)
    {
        orbitSpeedMultiplier = speedMultiplier;
        orbitHitIntervalMultiplier = hitIntervalMultiplier;
    }

    // ----- Hệ số bán kính từ nội tại (Claire 5 sao: bán kính hỏa +100% khi All Out Assault) -----
    public float GetAreaMultiplier(ElementType element)
    {
        float bonus = 0f;
        if (AllOutAssaultActive)
        {
            foreach (StarPerk perk in activePerks)     // foreach trên List không sinh rác
                if (perk.type == PerkType.AllOutAssaultRadiusBonus && perk.element == element)
                    bonus += perk.bonus;
        }
        return 1f + bonus;
    }

    // ----- Debug: kiểm tra công thức mà không cần chờ quái đánh -----
    [ContextMenu("Debug: Add Test Reductions")]
    private void DebugAddReductions()
    {
        Reductions.Set("test_all", DamageReductionSet.AllScope, 0.15f);      // chung 15%
        Reductions.Set("test_fire_a", (int)ElementType.Fire, 0.20f);         // hỏa 20%
        Reductions.Set("test_fire_b", (int)ElementType.Fire, 0.10f);         // hỏa 10% (cùng nhóm với dòng trên)
        DebugLogDamageTable();
    }

    [ContextMenu("Debug: Add Test Healing")]
    private void DebugAddHealing()
    {
        HealingBonuses.Set("test_heal_a", 0.15f);
        HealingBonuses.Set("test_heal_b", 0.10f);
        Debug.Log($"[Combat] Hệ số hồi phục = x{HealingBonuses.Multiplier:0.###}");
    }

    [ContextMenu("Debug: Clear Test Modifiers")]
    private void DebugClearModifiers()
    {
        Reductions.Remove("test_all"); Reductions.Remove("test_fire_a"); Reductions.Remove("test_fire_b");
        HealingBonuses.Remove("test_heal_a"); HealingBonuses.Remove("test_heal_b");
    }

    [ContextMenu("Debug: Log Damage Table")]
    private void DebugLogDamageTable()
    {
        var sb = new System.Text.StringBuilder("[Combat] Đánh 100 sát thương, DEF = " + Def + "\n");
        foreach (ElementType e in System.Enum.GetValues(typeof(ElementType)))
        {
            sb.Append(e).Append(": giảm ").Append((Reductions.GetTotalReduction(e) * 100f).ToString("0.#"))
              .Append("%, nhận ").Append(CalculateIncomingDamage(100f, e).ToString("0.##")).Append('\n');
        }
        sb.Append("Hồi phục: x").Append(HealingBonuses.Multiplier.ToString("0.###")).Append('\n');
        Debug.Log(sb.ToString());
    }

    // MỚI 12D: xem nhanh Liên kết linh hồn và Brave
    [ContextMenu("Debug: Log Soul Link")]
    private void DebugLogSoulLink()
    {
        Debug.Log($"[SoulLink] bật={soulLinkOn} | ATK nhân vật {Atk:0.##}, hệ số vũ khí x{WeaponAtkMultiplier:0.###}, " +
                  $"ATK cộng thêm {AddedAtkPct * 100f:0.#}% | HP tối đa {MaxHp:0.#} (cộng thêm đã nhận {hpBonus:0.#}, đang áp {appliedHpBonus:0.#}) | " +
                  $"DEF {Def:0.##} | SPD {MoveSpeed:0.##} | hiệu quả hồi mana x{ManaGainMultiplier:0.###} (từ chuyển đổi +{SoulLinkManaBonus * 100f:0.#}%) | Brave {braveStacks} stack");
    }

    [ContextMenu("Debug: Take 20 Fire")] private void DebugTakeFire() { TakeDamage(20f, ElementType.Fire); }
    [ContextMenu("Debug: Take 20 Pure")] private void DebugTakePure() { TakeDamage(20f, ElementType.Pure); }
}