using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class PlayerStats : MonoBehaviour
{
    [Header("Chỉ số gốc (sau này lấy từ CharacterData + nâng cấp menu)")]
    [SerializeField] private float baseMaxHp = 100f;
    [SerializeField] private float baseAtk = 1f;
    [SerializeField] private float baseDef = 0f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMagnetRadius = 1.5f;

    // Bonus nhận được TRONG trận (từ màn chọn nâng cấp)
    private float hpBonus, defBonus;                          // Cộng thẳng
    private float atkBonusPct, speedBonusPct, magnetBonusPct; // Phần trăm (0.1 = 10%)
    private float manaCostReductionPct;                       // Giảm mana cần cho All Out Assault (làm ở giai đoạn sau)

    // Giá trị cuối cùng mà các hệ thống khác đọc
    public float MaxHp => baseMaxHp + hpBonus;
    public float Atk => baseAtk * (1f + atkBonusPct);
    public float Def => baseDef + defBonus;
    public float MoveSpeed => baseMoveSpeed * (1f + speedBonusPct);
    public float MagnetRadius => baseMagnetRadius * (1f + magnetBonusPct);
    public float ManaCostMultiplier => Mathf.Max(0.2f, 1f - manaCostReductionPct);

    //public HealthSystem Health { get; private set; }
    private HealthSystem health;

    private void Awake()
    {
        //Health = GetComponent<HealthSystem>();
        Health.Init(MaxHp);
        Health.OnDied += HandleDied;
    }

    // Áp dụng một lần nâng cấp chỉ số
    public void ApplyStat(StatType stat, float amount)
    {
        switch (stat)
        {
            case StatType.Hp:
                hpBonus += amount;
                Health.IncreaseMax(amount);   // Tăng máu tối đa và hồi luôn phần tăng thêm
                break;
            case StatType.Mp: manaCostReductionPct += amount; break;
            case StatType.Atk: atkBonusPct += amount; break;
            case StatType.Def: defBonus += amount; break;
            case StatType.Spd: speedBonusPct += amount; break;
            case StatType.Mag: magnetBonusPct += amount; break;
        }
    }

    // Bonus sát thương theo nguyên tố (nội tại nhân vật). Hiện chưa có nên trả về 0.
    public float GetDamageBonus(ElementType element) => 0f;

    // DEF giảm sát thương theo công thức 100/(100+DEF): DEF càng cao càng giảm nhiều
    // nhưng không bao giờ miễn nhiễm hoàn toàn (DEF 100 giảm 50%, DEF 300 giảm 75%).
    public void TakeDamage(float amount)
    {
        Health.TakeDamage(amount * 100f / (100f + Def));
    }

    private void HandleDied()
    {
        Debug.Log("Player đã chết!");   // Game Over làm ở giai đoạn sau
    }

    public HealthSystem Health
    {
        get
        {
            if (health == null) health = GetComponent<HealthSystem>();   // Lấy lần đầu khi có người cần
            return health;
        }
    }
}