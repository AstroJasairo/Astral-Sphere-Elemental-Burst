using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private float damage;
    private float lifeLeft;
    private int hitsLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Gọi mỗi lần lấy đạn từ pool: đặt lại toàn bộ trạng thái
    public void Init(float damage, Vector2 direction, float speed, float lifetime, int pierce)
    {
        this.damage = damage;
        lifeLeft = lifetime;
        hitsLeft = pierce + 1;                 // pierce = 0 nghĩa là trúng 1 mục tiêu là hết
        rb.linearVelocity = direction * speed;
        transform.up = direction;              // Xoay đầu sprite theo hướng bay (sprite vẽ hướng lên trên)
    }

    private void Update()
    {
        lifeLeft -= Time.deltaTime;
        if (lifeLeft <= 0f) Despawn();         // Hết thời gian sống thì thu hồi
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitsLeft <= 0) return;
        if (!other.TryGetComponent(out HealthSystem hp)) return;

        hp.TakeDamage(damage);
        hitsLeft--;
        if (hitsLeft <= 0) Despawn();
    }

    private void Despawn()
    {
        hitsLeft = 0;
        PoolManager.Instance.Release(gameObject);
    }
}