using UnityEngine;
using UnityEngine.InputSystem;

// CÔNG CỤ TEST: gắn vào Player, chỉ hoạt động trong Editor. XÓA khi đã có vũ khí Phong/Mộc.
// Phím K: đẩy lùi quái xung quanh. Phím R: trói quái xung quanh 3 giây.
public class StatusEffectTester : MonoBehaviour
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float range = 6f;

    private void Update()
    {
#if UNITY_EDITOR
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.kKey.wasPressedThisFrame) Apply(true);
        if (kb.rKey.wasPressedThisFrame) Apply(false);
#endif
    }

    private void Apply(bool knockback)
    {
        int count = TargetFinder.FindAll(transform.position, range, enemyMask);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = TargetFinder.Results[i];
            if (!col.TryGetComponent(out StatusEffectHandler status)) continue;

            if (knockback)
            {
                // Hướng đẩy: từ người chơi ra phía quái
                Vector2 dir = col.transform.position - transform.position;
                status.ApplyKnockback(dir, 2f);
            }
            else
            {
                status.ApplyRoot(3f);
            }
        }
    }
}