using System;
using System.Collections.Generic;
using UnityEngine;

// Một hiệu ứng gây ra khi đòn đánh trúng người chơi. Khai báo trong asset (EnemyData, dữ liệu BOSS...).
[Serializable]
public class HitEffect
{
    public enum Kind { Debuff, Dot }

    public Kind kind = Kind.Debuff;

    [Tooltip("Dùng khi Kind = Debuff")]
    public BuffType debuff = BuffType.Slow;
    [Tooltip("Phần trăm (0.5 = 50%). Root, Stun, Silence bỏ qua giá trị này")]
    public float value = 0.5f;
    [Tooltip("Giây hiệu lực (Debuff). DOT lấy thời gian từ chính DotData")]
    public float duration = 5f;

    [Tooltip("Dùng khi Kind = Dot")]
    public DotData dot;

    public void Apply(PlayerStats target)
    {
        PlayerDebuffSystem debuffs = target.Debuffs;
        if (kind == Kind.Dot) debuffs.ApplyDot(dot);
        else debuffs.ApplyDebuff(debuff, value, duration);
    }

    public static void ApplyAll(List<HitEffect> effects, PlayerStats target)
    {
        if (effects == null) return;
        for (int i = 0; i < effects.Count; i++) effects[i].Apply(target);   // for thay vì foreach: không sinh rác
    }
}