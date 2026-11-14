using System;
using UnityEngine;

// Hiệu ứng vũ khí gây lên QUÁI khi đòn trúng. Khai báo theo từng cấp vũ khí (WeaponLevel.hitEffects).
// Ví dụ: Lửa Lv4 = DOT Bỏng, maxStacks 2. Lửa Lv5 = DOT Bỏng, maxStacks 5.
[Serializable]
public class WeaponHitEffect
{
    public HitEffect.Kind kind = HitEffect.Kind.Dot;

    [Range(0f, 1f)] public float chance = 1f;      // Xác suất gây hiệu ứng mỗi đòn trúng

    [Header("Kind = Debuff (Slow, Root, Stun, Frostbite, Vulnerable, Exhaust)")]
    public BuffType debuff = BuffType.Slow;
    public float value = 0.3f;                      // 0.3 = 30%. Root/Stun bỏ qua
    public float duration = 3f;

    [Header("Kind = Dot")]
    public DotData dot;
    [Tooltip("Số stack tối đa của DOT này ở cấp vũ khí này. 0 = dùng maxStacks trong DotData")]
    public int maxStacks = 0;
    [Tooltip("Bật: sát thương mỗi nhịp nhân với ATK của người chơi lúc dính đòn")]
    public bool scaleWithAtk = true;
}