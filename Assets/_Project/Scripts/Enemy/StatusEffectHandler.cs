using UnityEngine;

// Quản lý hiệu ứng trạng thái trên quái: đẩy lùi (Phong) và trói chân (Mộc)
public class StatusEffectHandler : MonoBehaviour
{
    private EnemyBase enemy;
    private float rootUntil;
    private float knockbackUntil;
    private float knockbackDuration = 0.25f;
    private Vector2 knockbackStartVelocity;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    // EnemyBase.Init gọi để quái tái sử dụng từ pool không còn dính hiệu ứng cũ
    public void ResetEffects()
    {
        rootUntil = 0f;
        knockbackUntil = 0f;
    }

    private bool IsImmune => enemy != null && enemy.Data != null && enemy.Data.statusImmune;
    public bool IsRooted => Time.time < rootUntil;

    // Trói chân: quái không thể di chuyển trong duration giây (vẫn đánh được nếu đang chạm người chơi)
    public void ApplyRoot(float duration)
    {
        if (IsImmune) return;
        rootUntil = Mathf.Max(rootUntil, Time.time + duration);   // Trói lại thì kéo dài, không rút ngắn
    }

    // Đẩy lùi theo hướng direction, tổng quãng đường distance (đơn vị Unity) trong duration giây
    public void ApplyKnockback(Vector2 direction, float distance, float duration = 0.25f)
    {
        if (IsImmune || direction == Vector2.zero) return;

        knockbackDuration = duration;
        knockbackUntil = Time.time + duration;
        // Vận tốc giảm đều từ v0 về 0 trong duration => quãng đường = v0 x duration / 2
        // => v0 = 2 x distance / duration
        knockbackStartVelocity = direction.normalized * (2f * distance / duration);
    }

    // EnemyBase hỏi mỗi FixedUpdate. true = đang bị khống chế, hãy dùng "velocity" thay cho việc đuổi theo
    public bool TryGetForcedVelocity(out Vector2 velocity)
    {
        if (IsRooted)                                  // Bị trói thì đứng yên (ưu tiên hơn đẩy lùi)
        {
            velocity = Vector2.zero;
            return true;
        }

        float remaining = knockbackUntil - Time.time;
        if (remaining > 0f)                            // Đang bị đẩy: vận tốc yếu dần về 0
        {
            velocity = knockbackStartVelocity * (remaining / knockbackDuration);
            return true;
        }

        velocity = Vector2.zero;
        return false;
    }
}