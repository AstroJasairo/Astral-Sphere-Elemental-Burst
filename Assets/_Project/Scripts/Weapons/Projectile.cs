using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject explosionEffect;   // Tùy chọn: hình vụ nổ (prefab có TimedEffect)

    private Rigidbody2D rb;
    private float damage;
    private float lifeLeft;
    private int hitsLeft;
    private float explosionRadius;
    private LayerMask enemyMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // explosionRadius = 0: đạn thường. Lớn hơn 0: đạn nổ vùng.
    public void Init(float damage, Vector2 direction, float speed, float lifetime, int pierce,
                     float explosionRadius = 0f, LayerMask enemyMask = default)
    {
        this.damage = damage;
        this.explosionRadius = explosionRadius;
        this.enemyMask = enemyMask;
        lifeLeft = lifetime;
        hitsLeft = pierce + 1;
        rb.linearVelocity = direction * speed;
        transform.up = direction;
    }

    private void Update()
    {
        lifeLeft -= Time.deltaTime;
        if (lifeLeft > 0f) return;

        // Hết thời gian: đạn nổ vẫn nổ tại chỗ (nếu chưa nổ)
        if (explosionRadius > 0f && hitsLeft > 0) Explode();
        Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitsLeft <= 0) return;
        if (!other.TryGetComponent(out HealthSystem hp)) return;

        if (explosionRadius > 0f)
        {
            Explode();     // Nổ gây sát thương mọi kẻ địch trong bán kính (gồm cả con vừa trúng)
            Despawn();
            return;
        }

        hp.TakeDamage(damage);
        hitsLeft--;
        if (hitsLeft <= 0) Despawn();
    }

    private void Explode()
    {
        int count = TargetFinder.FindAll(transform.position, explosionRadius, enemyMask);
        for (int i = 0; i < count; i++)
        {
            if (TargetFinder.Results[i].TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }

        if (explosionEffect != null)
        {
            GameObject fx = PoolManager.Instance.Get(explosionEffect, transform.position, Quaternion.identity);
            fx.GetComponent<TimedEffect>().Play(explosionRadius * 2f);   // Đường kính = 2 x bán kính
        }
    }

    private void Despawn()
    {
        hitsLeft = 0;   // Chặn nổ hai lần
        PoolManager.Instance.Release(gameObject);
    }
}