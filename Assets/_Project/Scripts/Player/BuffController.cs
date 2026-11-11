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

    // Số loại buff/debuff (tự tăng khi thêm giá trị vào enum BuffType)
    public static readonly int TypeCount = Enum.GetValues(typeof(BuffType)).Length;

    // Cũ:  private readonly Buff[] buffs = new Buff[Enum.GetValues(typeof(BuffType)).Length];
    private readonly Buff[] buffs = new Buff[TypeCount];

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

    // Loại nào là debuff (dùng cho kiểm tra, HUD viền đỏ, "tẩy hiệu ứng")
    public static bool IsDebuff(BuffType type)
    {
        switch (type)
        {
            case BuffType.Slow:
            case BuffType.Root:
            case BuffType.Stun:
            case BuffType.Frostbite:
            case BuffType.Silence:
            case BuffType.Vulnerable:
            case BuffType.Exhaust:
            case BuffType.Blind:
            case BuffType.Weakness:
            case BuffType.ManaDrain:
                return true;
            default:
                return false;
        }
    }

    // Khống chế cùng loại KHÔNG cộng dồn: mạnh hơn thì thay, bằng nhau thì reset thời gian, yếu hơn thì bỏ qua.
    // Root/Stun/Silence: "mạnh" = còn hiệu lực lâu hơn. Các loại còn lại: "mạnh" = giá trị % lớn hơn.
    public bool ApplyDebuff(BuffType type, float value, float duration)
    {
        if (duration <= 0f) return false;

        Buff b = buffs[(int)type];
        float now = Time.time;
        float newEnd = now + duration;

        if (now < b.endTime)                                       // đang dính cùng loại
        {
            bool byDuration = type == BuffType.Root || type == BuffType.Stun || type == BuffType.Silence;
            float current = byDuration ? b.endTime : b.value;
            float incoming = byDuration ? newEnd : value;
            if (incoming < current && !Mathf.Approximately(incoming, current)) return false;   // yếu hơn: bỏ qua
        }

        b.value = value;                                           // mạnh hơn: thay thế. Bằng nhau: làm mới thời gian
        b.duration = duration;
        b.endTime = newEnd;
        buffs[(int)type] = b;
        return true;
    }

    // Gỡ ngay một buff/debuff (Silence kết thúc buff của skill chủ động, tẩy hiệu ứng...)
    public void Clear(BuffType type)
    {
        buffs[(int)type] = default(Buff);
    }
}