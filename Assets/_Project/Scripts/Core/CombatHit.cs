using System.Collections.Generic;
using UnityEngine;

// Cổng DUY NHẤT để vũ khí và đạn của người chơi đánh QUÁI.
// Áp hiệu ứng (debuff/DOT), gây sát thương có nguyên tố (qua DEF, kháng, miễn nhiễm của quái),
// rồi báo cho người chơi biết "vừa trúng một con quái" (Đoạt mệnh nghe).
public static class CombatHit
{
    private static PlayerStats attacker;

    // Người chơi (lấy lười). Object bị hủy khi đóng scene thì tự lấy lại ở lần sau
    private static PlayerStats GetAttacker()
    {
        if (attacker == null && GameManager.Instance != null && GameManager.Instance.Player != null)
            GameManager.Instance.Player.TryGetComponent(out attacker);
        return attacker;
    }

    // target: Collider2D hoặc Transform của quái. atkScale: ATK của người chơi (dùng cho DOT có scaleWithAtk)
    public static void HitEnemy(Component target, float damage, ElementType element,
                                List<WeaponHitEffect> effects = null, float atkScale = 1f)
    {
        if (!target.TryGetComponent(out HealthSystem hp)) return;
        if (hp.IsDead || hp.Invulnerable) return;                // BOSS đang bất tử: không dính gì

        bool hasEffects = effects != null && effects.Count > 0;
        PlayerStats player = GetAttacker();
        bool needEnemy = hasEffects || (player != null && player.HasHitListeners);

        EnemyBase enemy = null;
        if (needEnemy) target.TryGetComponent(out enemy);        // Chỉ lấy khi thật sự cần

        // Debuff TRƯỚC, sát thương SAU: quái chết sẽ bị trả về pool nên không áp debuff lên xác nữa
        if (hasEffects && enemy != null) enemy.Debuffs.ApplyWeaponEffects(effects, atkScale);

        hp.TakeHit(damage, element);

        // Quái đã chết trong đòn này thì không báo
        if (enemy != null && player != null && !hp.IsDead) player.NotifyHit(hp, enemy.Tier);
    }
}