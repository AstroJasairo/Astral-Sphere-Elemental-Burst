using UnityEngine;

// Vũ khí dạng "vùng quanh người": Lửa. WeaponBase gọi Fire() theo cooldown, nên cooldown là chu kỳ gây sát thương liên tục.
public class AuraWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;   // Chọn layer Enemy
    [SerializeField] private Transform visual;      // Hình tròn minh họa Aura (con của object này)

    private float shownRadius = -1f;

    // Bán kính thực tế = bán kính của cấp x hệ số nội tại (Claire 5 sao khi All Out Assault)
    private float Radius => CurrentLevel.radius * AreaScale;

    protected override void Update()
    {
        base.Update();
        if (data == null) return;

        // Cập nhật hình minh họa khi bán kính đổi (lên cấp hoặc bật/tắt bonus All Out Assault)
        float r = Radius;
        if (Mathf.Approximately(r, shownRadius)) return;
        shownRadius = r;

        if (visual != null)
        {
            float diameter = r * 2f;            // Sprite Circle mặc định có đường kính 1 đơn vị
            visual.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    protected override bool Fire()
    {
        int count = TargetFinder.FindAll(transform.position, Radius, enemyMask);
        float damage = CalculateDamage();

        // Gây sát thương cho mọi kẻ địch trong vòng tròn
        for (int i = 0; i < count; i++)
        {
            if (TargetFinder.Results[i].TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }
        return true;    // Aura luôn "bắn" dù không có ai trong vùng
    }
}