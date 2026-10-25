using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(HealthSystem))]
public class PlayerStats : MonoBehaviour
{
    [Header("Nhân vật (tạm gán tay để test; sau này lấy từ menu chọn nhân vật)")]
    [SerializeField] private CharacterData character;
    [SerializeField, Range(0, 5)] private int starLevel = 0;

    // Chỉ số gốc: nạp từ CharacterData (giá trị dưới đây chỉ là dự phòng khi chưa gán)
    private float baseMaxHp = 100f;
    private float baseAtk = 1f;
    private float baseDef = 0f;
    private float baseMoveSpeed = 5f;
    private float baseMagnetRadius = 1.5f;

    // Bonus nhận được TRONG trận
    private float hpBonus, defBonus;
    private float atkBonusPct, speedBonusPct, magnetBonusPct;
    private float manaCostReductionPct;
    private float aoaAtkPct, aoaSpeedPct;        // Buff All Out Assault (tạm thời, kênh riêng)
    private float aoaCooldownMultiplier = 1f;    // 1 = bình thường, 0.25 = hồi chiêu còn 25%

    private readonly List<StarPerk> activePerks = new List<StarPerk>();

    public float MaxHp => baseMaxHp + hpBonus;
    public float Atk => baseAtk * (1f + atkBonusPct + aoaAtkPct + Buffs.GetValue(BuffType.Rage));
    public float Def => baseDef + defBonus;
    public float MoveSpeed => baseMoveSpeed * (1f + speedBonusPct + aoaSpeedPct + Buffs.GetValue(BuffType.Speed));
    public float MagnetRadius => baseMagnetRadius * (1f + magnetBonusPct + Buffs.GetValue(BuffType.Magnet));
    public float ManaCostMultiplier => Mathf.Max(0.2f, 1f - manaCostReductionPct);
    public float CooldownMultiplier => aoaCooldownMultiplier;
    public float ScoreMultiplier => 1f + Buffs.GetValue(BuffType.ScoreBoost);

    public float Shield { get; private set; }
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
        if (character == null)
        {
            Debug.LogWarning("PlayerStats: chưa gán CharacterData, đang dùng chỉ số dự phòng.", this);
            return;
        }

        baseMaxHp = character.baseMaxHp;
        baseAtk = character.baseAtk;
        baseDef = character.baseDef;
        baseMoveSpeed = character.baseMoveSpeed;
        baseMagnetRadius = character.baseMagnetRadius;

        activePerks.Clear();
        activePerks.AddRange(character.GetActivePerks(starLevel));   // Chỉ tính một lần lúc vào trận
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

    public void TakeDamage(float amount)
    {
        //Health.TakeDamage(amount * 100f / (100f + Def));
        float damage = amount * 100f / (100f + Def);       // DEF giảm sát thương trước

        if (Shield > 0f)
        {
            float absorbed = Mathf.Min(Shield, damage);    // Khiên chịu được bao nhiêu thì chịu bấy nhiêu
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
}