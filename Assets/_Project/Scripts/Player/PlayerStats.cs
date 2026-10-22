using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float atk = 1f;

    public float Atk => atk;
    private HealthSystem health;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
        health.Init(maxHp);
        health.OnDied += HandleDied;
    }

    // Bonus sát thương theo nguyên tố (nội tại nhân vật). Hiện chưa có nên trả về 0.
    public float GetDamageBonus(ElementType element) => 0f;

    // Nhận sát thương. Sau này công thức DEF sẽ thêm vào đây
    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }

    private void HandleDied()
    {
        Debug.Log("Player đã chết!");   // Màn hình Game Over sẽ làm ở giai đoạn sau
    }
}