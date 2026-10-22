using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected WeaponData data;
    [SerializeField, Range(1, 5)] protected int level = 1;

    protected PlayerStats owner;
    private float timer;

    // Thông số của cấp hiện tại (level 1 -> phần tử 0)
    protected WeaponLevel CurrentLevel => data.levels[Mathf.Clamp(level, 1, data.levels.Length) - 1];

    protected virtual void Awake()
    {
        owner = GetComponentInParent<PlayerStats>();   // Vũ khí là object con của Player
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < data.cooldown) return;

        // Fire() trả về true nếu bắn được. Nếu không có mục tiêu, thử lại sau 0.2s thay vì mỗi frame
        timer = Fire() ? 0f : data.cooldown - 0.2f;
    }

    // Mỗi loại vũ khí tự cài đặt cách tấn công
    protected abstract bool Fire();

    public void LevelUp()
    {
        if (level < data.levels.Length) level++;
    }

    protected float CalculateDamage()
    {
        return DamageCalculator.Calculate(data.baseDamage, owner.Atk, owner.GetDamageBonus(data.element));
    }
}