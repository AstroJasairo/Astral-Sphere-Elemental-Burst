using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(HealthSystem))]
public class PlayerStats : MonoBehaviour
{
    //[Header("Nhân vật (tạm gán tay để test; sau này lấy từ menu chọn nhân vật)")]
    //[SerializeField] private CharacterData character;
    //[SerializeField, Range(0, 5)] private int starLevel = 0;
    [Header("Nhân vật (Chỉ dùng khi chạy thẳng scene Game để test)")]
    [SerializeField] private CharacterData character;      // Dự phòng khi chưa có MetaConfig
    [SerializeField] private bool useDebugStarLevel;       // Bật để dùng "Star Level" bên dưới thay cho sao đã lưu
    [SerializeField] private bool debugInvincible;       // CHỈ ĐỂ TEST HIỆU NĂNG: người chơi không bao giờ chết
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
    private float hpBonus, defBonus;
    private float atkBonusPct, speedBonusPct, magnetBonusPct;
    private float manaCostReductionPct;
    private float aoaAtkPct, aoaSpeedPct;        // Buff All Out Assault (tạm thời, kênh riêng)
    private float aoaCooldownMultiplier = 1f;    // 1 = bình thường, 0.25 = hồi chiêu còn 25%

    private readonly List<StarPerk> activePerks = new List<StarPerk>();

    public CharacterData Character => character;
    public float MaxHp => baseMaxHp + hpBonus;
    public float Atk => baseAtk * (1f + atkBonusPct + aoaAtkPct + Buffs.GetValue(BuffType.Rage) + Buffs.GetValue(BuffType.ActiveSkill));
    public float Def => baseDef + defBonus;
    public float MoveSpeed => baseMoveSpeed * (1f + speedBonusPct + aoaSpeedPct + Buffs.GetValue(BuffType.Speed));
    public float MagnetRadius => baseMagnetRadius * (1f + magnetBonusPct + Buffs.GetValue(BuffType.Magnet));
    public float ManaCostMultiplier => Mathf.Max(MinManaCostMultiplier, 1f - manaCostReductionPct);
    public float CooldownMultiplier => aoaCooldownMultiplier;
    public float ScoreMultiplier => 1f + Buffs.GetValue(BuffType.ScoreBoost);
    public float OrbitSpeedMultiplier => orbitSpeedMultiplier;
    public float OrbitHitIntervalMultiplier => orbitHitIntervalMultiplier;

    public float Shield { get; private set; }

    // Các nguồn giảm sát thương và hiệu quả hồi phục (passive, buff... tự đăng ký vào đây)
    public DamageReductionSet Reductions { get; } = new DamageReductionSet();
    public HealingBonusSet HealingBonuses { get; } = new HealingBonusSet();
    //public float IncomingDamageMultiplier { get; set; } = 1f;
    public event Action<float, float> OnShieldChanged;   // (lá chắn hiện tại, tối đa) -> HUD lắng nghe

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
                hpBonus += amount;
                Health.IncreaseMax(amount);
                break;
            case StatType.Mp: manaCostReductionPct += amount; break;
            case StatType.Atk: atkBonusPct += amount; break;
            case StatType.Def: defBonus += amount; break;
            case StatType.Spd: speedBonusPct += amount; break;
            case StatType.Mag: magnetBonusPct += amount; break;
        }
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
        return bonus;
    }

    // Sát thương thực nhận = số gốc x (1 - giảm sát thương) x 100/(100+DEF). Pure bỏ qua cả hai.
    public float CalculateIncomingDamage(float amount, ElementType element)
    {
        if (element == ElementType.Pure) return amount;
        float reduction = Reductions.GetTotalReduction(element);
        return amount * (1f - reduction) * 100f / (100f + Def);
    }

    public void TakeDamage(float amount, ElementType element)
    {
        if (debugInvincible && Debug.isDebugBuild) return;
        float damage = CalculateIncomingDamage(amount, element);

        if (Shield > 0f)                                   // Khiên hấp thụ trước, kể cả Pure
        {
            float absorbed = Mathf.Min(Shield, damage);
            Shield -= absorbed;
            damage -= absorbed;
            OnShieldChanged?.Invoke(Shield, MaxHp);
        }

        if (damage > 0f) Health.TakeDamage(damage);        // Phần dư mới trừ vào máu
    }

    private void HandleDied()
    {
        GameManager.Instance.TriggerGameOver();
    }

    // Thêm hàm: AllOutAssaultSystem gọi khi bật/tắt
    public void SetAllOutAssault(bool active, float atkPct, float speedPct, float cooldownMultiplier)
    {
        AllOutAssaultActive = active;     // Bật perk 5 sao trong GetDamageBonus
        aoaAtkPct = active ? atkPct : 0f;
        aoaSpeedPct = active ? speedPct : 0f;
        aoaCooldownMultiplier = active ? cooldownMultiplier : 1f;   // Tắt thì trả về đúng 1, không "trừ ngược"
    }

    public void AddShield(float amount)
    {
        Shield = Mathf.Min(Shield + amount, MaxHp);       // Lá chắn tối đa bằng máu tối đa
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
            sb.Append(e).Append(": giảm ").Append((Reductions.GetTotalReduction(e) * 100f).ToString("0.#"))
              .Append("%, nhận ").Append(CalculateIncomingDamage(100f, e).ToString("0.##")).Append('\n');
        Debug.Log(sb.ToString());
    }

    [ContextMenu("Debug: Take 20 Fire")] private void DebugTakeFire() { TakeDamage(20f, ElementType.Fire); }
    [ContextMenu("Debug: Take 20 Pure")] private void DebugTakePure() { TakeDamage(20f, ElementType.Pure); }
}