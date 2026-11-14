using System.Collections.Generic;
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
    private ElementType element;                           // MỚI: nguyên tố của đạn (cho DEF/kháng của quái)
    private List<WeaponHitEffect> effects;                 // MỚI: hiệu ứng theo cấp vũ khí
    private float atkScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Tham số cuối đều TÙY CHỌN. Nguyên tố mặc định Pure = bỏ qua DEF, nên luôn truyền nguyên tố của vũ khí.
    public void Init(float damage, Vector2 direction, float speed, float lifetime, int pierce,
                     float explosionRadius = 0f, LayerMask enemyMask = default,
                     ElementType element = ElementType.Pure, List<WeaponHitEffect> effects = null, float atkScale = 1f)
    {
        this.damage = damage;
        this.explosionRadius = explosionRadius;
        this.enemyMask = enemyMask;
        this.element = element;
        this.effects = effects;
        this.atkScale = atkScale;

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

        CombatHit.HitEnemy(other, damage, element, effects, atkScale);
        hitsLeft--;
        if (hitsLeft <= 0) Despawn();
    }

    private void Explode()
    {
        int count = TargetFinder.FindAll(transform.position, explosionRadius, enemyMask);
        for (int i = 0; i < count; i++)
            CombatHit.HitEnemy(TargetFinder.Results[i], damage, element, effects, atkScale);

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