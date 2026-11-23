using System;
using UnityEngine;

// Thứ sở hữu một kỹ năng dùng chung: người chơi (PlayerStats) hoặc quái/BOSS (EnemyBase).
// Skill dùng chung chỉ được dùng những thứ này. Skill cần đồ riêng của người chơi (vũ khí, mana...) dùng PlayerStats.
public interface ISkillOwner
{
    Transform Transform { get; }
    HealthSystem Health { get; }
    DamageReductionSet Reductions { get; }

    // Phát sau khi một đòn từ bên ngoài đã trừ máu (KHÔNG gồm tự trừ máu như Huyết nộ)
    event Action<float> OnDamaged;

    // Đặt các miễn nhiễm của một nguồn (None = gỡ). Có miễn nhiễm thì hiệu ứng tương ứng đang dính bị xóa
    void SetImmunity(object source, ImmunityFlags flags);

    // Hồi máu (người chơi nhân hiệu quả hồi phục)
    void ReceiveHealing(float amount);
}