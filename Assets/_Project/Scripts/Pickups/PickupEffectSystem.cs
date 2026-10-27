using UnityEngine;

// Lắng nghe Pickup.Collected và áp dụng hiệu ứng tương ứng lên người chơi
public class PickupEffectSystem : MonoBehaviour
{
    [SerializeField] private PickupEffectData data;
    [SerializeField] private PlayerStats stats;

    private void OnEnable() { Pickup.Collected += HandleCollected; }
    private void OnDisable() { Pickup.Collected -= HandleCollected; }

    private static float Meta(SharedUpgradeType type) => MetaProgress.GetSharedBonus(type);

    private void HandleCollected(PickupType type, int amount)
    {
        switch (type)
        {
            case PickupType.HealthPotion:
                // Bonus nâng cấp chung CỘNG THÊM vào phần trăm hồi máu
                float healPct = Mathf.Clamp01(data.healPercent + Meta(SharedUpgradeType.HealthPotion));
                stats.Health.Heal(stats.Health.MaxHealth * healPct);
                break;
            case PickupType.SpeedPotion:
                stats.Buffs.Apply(BuffType.Speed, data.speedBonusPct + Meta(SharedUpgradeType.SpeedPotion), data.speedDuration);
                break;
            case PickupType.RagePotion:
                stats.Buffs.Apply(BuffType.Rage, data.rageBonusPct + Meta(SharedUpgradeType.RagePotion), data.rageDuration);
                break;
            case PickupType.Magnet:
                stats.Buffs.Apply(BuffType.Magnet, data.magnetBonusPct + Meta(SharedUpgradeType.Magnet), data.magnetDuration);
                break;
            case PickupType.Shield:
                // Bonus nâng cấp chung NHÂN với lá chắn
                float shield = data.shieldBase * (1f + stats.Def * data.shieldDefScale) * (1f + Meta(SharedUpgradeType.Shield));
                stats.AddShield(shield);
                break;
            case PickupType.ScoreBoost:
                stats.Buffs.Apply(BuffType.ScoreBoost, data.scoreBonusPct + Meta(SharedUpgradeType.ScoreBoost), data.scoreDuration);
                break;
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