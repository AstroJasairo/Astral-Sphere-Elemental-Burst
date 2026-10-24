using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private float damage;
    private float lifeLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Gọi mỗi lần lấy từ pool: đặt lại toàn bộ trạng thái
    public void Init(float damage, Vector2 direction, float speed, float lifetime)
    {
        this.damage = damage;
        lifeLeft = lifetime;
        rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        lifeLeft -= Time.deltaTime;
        if (lifeLeft <= 0f) PoolManager.Instance.Release(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerStats player)) return;   // Chỉ quan tâm người chơi
        player.TakeDamage(damage);
        PoolManager.Instance.Release(gameObject);                     // Pool tự bỏ qua nếu đã trả về rồi
    }
}