using UnityEngine;

// Lắng nghe Pickup.Collected và áp dụng hiệu ứng tương ứng lên người chơi
public class PickupEffectSystem : MonoBehaviour
{
    [SerializeField] private PickupEffectData data;
    [SerializeField] private PlayerStats stats;

    private void OnEnable() { Pickup.Collected += HandleCollected; }
    private void OnDisable() { Pickup.Collected -= HandleCollected; }

    private void HandleCollected(PickupType type, int amount)
    {
        switch (type)
        {
            case PickupType.HealthPotion:
                stats.Health.Heal(stats.Health.MaxHealth * data.healPercent);
                break;
            case PickupType.SpeedPotion:
                stats.Buffs.Apply(BuffType.Speed, data.speedBonusPct, data.speedDuration);
                break;
            case PickupType.RagePotion:
                stats.Buffs.Apply(BuffType.Rage, data.rageBonusPct, data.rageDuration);
                break;
            case PickupType.Magnet:
                stats.Buffs.Apply(BuffType.Magnet, data.magnetBonusPct, data.magnetDuration);
                break;
            case PickupType.Shield:
                // Lá chắn dựa theo DEF: DEF càng cao, khiên càng dày
                stats.AddShield(data.shieldBase * (1f + stats.Def * data.shieldDefScale));
                break;
            case PickupType.ScoreBoost:
                stats.Buffs.Apply(BuffType.ScoreBoost, data.scoreBonusPct, data.scoreDuration);
                break;
                // Gold, Crystal, ManaPotion do các hệ thống khác xử lý
        }
    }

    // Chuột phải vào component (khi đang Play) để thử nhanh toàn bộ hiệu ứng
    [ContextMenu("Debug: Apply All Effects")]
    private void DebugApplyAll()
    {
        foreach (PickupType t in new[] { PickupType.HealthPotion, PickupType.SpeedPotion, PickupType.RagePotion,
                                         PickupType.Magnet, PickupType.Shield, PickupType.ScoreBoost })
            HandleCollected(t, 1);
    }
}