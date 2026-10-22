using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PickupCollector : MonoBehaviour
{
    [SerializeField] private LayerMask pickupMask;          // Chọn layer Pickup
    [SerializeField] private float scanInterval = 0.1f;

    private PlayerStats stats;
    private ContactFilter2D filter;
    private readonly Collider2D[] buffer = new Collider2D[64];   // Bộ đệm dùng lại, không sinh rác
    private float timer;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        filter.SetLayerMask(pickupMask);
        filter.useTriggers = true;   // Collider vật phẩm là Trigger
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < scanInterval) return;
        timer = 0f;

        // Bán kính lấy từ chỉ số MAG nên nâng MAG là hút xa hơn
        int count = Physics2D.OverlapCircle(transform.position, stats.MagnetRadius, filter, buffer);
        for (int i = 0; i < count; i++)
        {
            if (buffer[i].TryGetComponent(out Pickup pickup))
                pickup.StartAttract(transform);
        }
    }
}