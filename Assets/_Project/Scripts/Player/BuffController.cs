using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BuffController : MonoBehaviour
{
    private struct Buff
    {
        public float value;      // Mức buff (0.5 = +50%)
        public float endTime;    // Thời điểm hết hạn (theo Time.time)
        public float duration;   // Tổng thời lượng lần nhặt gần nhất (để UI tính tỉ lệ còn lại)
    }

    // Một ô cho mỗi loại buff, theo thứ tự enum BuffType
    private readonly Buff[] buffs = new Buff[Enum.GetValues(typeof(BuffType)).Length];

    // Áp dụng buff. Đang có sẵn thì làm mới thời gian và giữ giá trị cao hơn (không cộng dồn vô hạn)
    public void Apply(BuffType type, float value, float duration)
    {
        Buff b = buffs[(int)type];
        bool wasActive = Time.time < b.endTime;

        b.value = wasActive ? Mathf.Max(b.value, value) : value;
        b.duration = duration;
        b.endTime = Time.time + duration;
        buffs[(int)type] = b;     // Buff là struct nên phải gán ngược lại vào mảng
    }

    // Giá trị buff hiện tại (0 nếu không có hoặc đã hết hạn)
    public float GetValue(BuffType type)
    {
        Buff b = buffs[(int)type];
        return Time.time < b.endTime ? b.value : 0f;
    }

    // Tỉ lệ thời gian còn lại, 0..1 (dùng cho icon trên HUD)
    public float GetRemaining01(BuffType type)
    {
        Buff b = buffs[(int)type];
        float remaining = b.endTime - Time.time;
        return (remaining > 0f && b.duration > 0f) ? remaining / b.duration : 0f;
    }
}