using System.Collections.Generic;
using UnityEngine;

// Cổng DUY NHẤT để vũ khí và đạn của người chơi đánh QUÁI.
// Làm hai việc: áp hiệu ứng (debuff/DOT) rồi gây sát thương có nguyên tố (qua DEF và kháng của quái).
public static class CombatHit
{
    // target: Collider2D hoặc Transform của quái. atkScale: ATK của người chơi (dùng cho DOT có scaleWithAtk)
    public static void HitEnemy(Component target, float damage, ElementType element,
                                List<WeaponHitEffect> effects = null, float atkScale = 1f)
    {
        if (!target.TryGetComponent(out HealthSystem hp)) return;
        if (hp.IsDead || hp.Invulnerable) return;               // BOSS đang miễn nhiễm: không dính gì

        // Debuff TRƯỚC, sát thương SAU: quái chết sẽ bị trả về pool nên không áp debuff lên xác nữa
        if (effects != null && effects.Count > 0 && target.TryGetComponent(out EnemyBase enemy))
            enemy.Debuffs.ApplyWeaponEffects(effects, atkScale);

        hp.TakeHit(damage, element);
    }
}