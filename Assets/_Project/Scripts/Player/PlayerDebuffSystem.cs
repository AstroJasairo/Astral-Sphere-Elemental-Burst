using System;
using System.Collections.Generic;
using UnityEngine;

// Một DOT đang dính trên người chơi: một DotData = MỘT mục, nhiều stack.
// (Lớp thường, được tái sử dụng qua kho nên không sinh rác mỗi lần dính.)
public class DotInstance
{
    public DotData data;
    public int stacks;
    public float endTime;
    public float nextTickTime;

    public float Remaining01 => (data != null && data.duration > 0f) ? Mathf.Clamp01((endTime - Time.time) / data.duration) : 0f;
}

// Điều phối debuff của người chơi: DOT, và đồng bộ các trạng thái cần "bước chuyển" (Silence, Yếu đuối).
// Các debuff chỉ đổi chỉ số (Slow, Blind...) không cần ở đây: PlayerStats đọc thẳng từ BuffController.
[RequireComponent(typeof(PlayerStats))]
public class PlayerDebuffSystem : MonoBehaviour
{
    [Header("Debug (chỉ khi Debug.isDebugBuild): chuột phải component để thử")]
    [SerializeField] private BuffType debugDebuff = BuffType.Slow;
    [SerializeField] private float debugValue = 0.5f;              // Root/Stun/Silence bỏ qua giá trị này
    [SerializeField] private float debugDuration = 5f;
    [SerializeField] private DotData debugDot;

    private readonly List<DotInstance> dots = new List<DotInstance>();
    private readonly Stack<DotInstance> freeDots = new Stack<DotInstance>();

    public IReadOnlyList<DotInstance> ActiveDots => dots;
    public event Action<DotInstance> OnDotAdded;                   // BuffHudView sinh icon
    public event Action<DotInstance> OnDotRemoved;                 // BuffHudView trả icon

    private PlayerStats stats;
    private bool wasSilenced;
    private float shownWeakness;

    private void Awake() { stats = GetComponent<PlayerStats>(); }

    // ---------------- API cho nguồn gây hiệu ứng (quái, BOSS, bẫy...) ----------------

    // value: phần trăm (0.5 = 50%) cho Slow, Tê cóng, Vulnerable, Exhaust, Blind, Yếu đuối, Suy giảm mana.
    // Root/Stun/Silence chỉ dùng duration. Trả về true nếu hiệu ứng được áp dụng (không bị hiệu ứng mạnh hơn chặn).
    public bool ApplyDebuff(BuffType type, float value, float duration)
    {
        if (!BuffController.IsDebuff(type))
        {
            Debug.LogWarning($"PlayerDebuffSystem: {type} không phải debuff.", this);
            return false;
        }
        if (GameManager.Instance != null && GameManager.Instance.IsEnded) return false;
        return stats.Buffs.ApplyDebuff(type, value, duration);
    }

    // Dính một stack DOT. Đã có thì +1 stack (tối đa maxStacks) và reset thời gian chung.
    public void ApplyDot(DotData data)
    {
        if (data == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsEnded) return;

        float now = Time.time;
        DotInstance inst = Find(data);
        if (inst == null)
        {
            inst = freeDots.Count > 0 ? freeDots.Pop() : new DotInstance();
            inst.data = data;
            inst.stacks = 1;
            inst.endTime = now + data.duration;
            inst.nextTickTime = now + data.tickInterval;
            dots.Add(inst);
            OnDotAdded?.Invoke(inst);
        }
        else
        {
            inst.stacks = Mathf.Min(inst.stacks + 1, data.maxStacks);
            inst.endTime = now + data.duration;                    // reset thời gian, giữ nguyên nhịp đang chạy
        }
    }

    // Gỡ mọi debuff và DOT (dùng cho "tẩy hiệu ứng" sau này và để test)
    public void ClearAll()
    {
        foreach (BuffType t in Enum.GetValues(typeof(BuffType)))
            if (BuffController.IsDebuff(t)) stats.Buffs.Clear(t);
        for (int i = dots.Count - 1; i >= 0; i--) RemoveDot(i);
    }

    private DotInstance Find(DotData data)
    {
        for (int i = 0; i < dots.Count; i++)
        {
            DotData d = dots[i].data;
            if (d == data || (!string.IsNullOrEmpty(d.id) && d.id == data.id)) return dots[i];
        }
        return null;
    }

    // ---------------- Vòng cập nhật ----------------

    private void Update()
    {
        SyncSilence();
        SyncWeakness();

        if (dots.Count == 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsEnded) return;

        float now = Time.time;                                     // Dừng khi tạm dừng
        for (int i = dots.Count - 1; i >= 0; i--)
        {
            DotInstance d = dots[i];
            int guard = 0;                                         // Chốt an toàn: tối đa 8 nhịp mỗi frame
            while (now >= d.nextTickTime && d.nextTickTime <= d.endTime + 0.001f && guard++ < 8)
            {
                Tick(d);
                d.nextTickTime += d.data.tickInterval;
                if (stats.Health.IsDead) return;
            }
            if (now >= d.endTime) RemoveDot(i);
        }
    }

    // Mỗi stack là MỘT đòn riêng: sau này hiệu ứng giảm sát thương theo từng đòn sẽ áp dụng cho từng stack
    private void Tick(DotInstance d)
    {
        for (int s = 0; s < d.stacks; s++)
        {
            stats.TakeDamage(d.data.damagePerTick, d.data.element);   // chịu DEF và giảm theo nguyên tố (Pure thì không)
            if (stats.Health.IsDead) return;
        }
    }

    private void RemoveDot(int index)
    {
        DotInstance d = dots[index];
        OnDotRemoved?.Invoke(d);                                   // Gửi TRƯỚC khi xóa dữ liệu
        dots.RemoveAt(index);
        d.data = null;
        freeDots.Push(d);
    }

    // Silence: chỉ báo cho SkillSystem khi trạng thái ĐỔI
    private void SyncSilence()
    {
        bool silenced = stats.IsSilenced;
        if (silenced == wasSilenced) return;
        wasSilenced = silenced;
        SkillSystem skills = stats.Skills;
        if (skills != null) skills.SetSilenced(silenced);
    }

    // Yếu đuối: chỉ báo cho HealthSystem khi giá trị ĐỔI (hết hiệu ứng: về 0, máu KHÔNG hồi)
    private void SyncWeakness()
    {
        float weakness = Mathf.Clamp01(stats.Buffs.GetValue(BuffType.Weakness));
        if (Mathf.Approximately(weakness, shownWeakness)) return;
        shownWeakness = weakness;
        stats.Health.SetMaxReduction(weakness);
    }

    // ---------------- Debug ----------------

    [ContextMenu("Debug: Apply Debuff")]
    private void DebugApplyDebuff()
    {
        if (!Debug.isDebugBuild) return;
        bool applied = ApplyDebuff(debugDebuff, debugValue, debugDuration);
        Debug.Log($"[Debuff] {debugDebuff} {debugValue:0.##} trong {debugDuration}s: {(applied ? "ÁP DỤNG" : "BỊ BỎ QUA (có hiệu ứng mạnh hơn)")}");
    }

    [ContextMenu("Debug: Apply DOT")]
    private void DebugApplyDot() { if (Debug.isDebugBuild) ApplyDot(debugDot); }

    [ContextMenu("Debug: Clear All")]
    private void DebugClear() { ClearAll(); }

    [ContextMenu("Debug: Log Stats")]
    private void DebugLogStats()
    {
        Debug.Log($"[Debuff] Tốc độ {stats.MoveSpeed:0.##} | ATK {stats.Atk:0.##} | DEF {stats.Def:0.##} | Nam châm {stats.MagnetRadius:0.##} | " +
                  $"Tốc đánh x{stats.AttackSpeedMultiplier:0.##} | Hồi mana x{stats.ManaGainMultiplier:0.##} | " +
                  $"Máu {stats.Health.CurrentHealth:0.#}/{stats.Health.MaxHealth:0.#} (gốc {stats.Health.BaseMaxHealth:0.#}) | " +
                  $"Choáng {stats.IsStunned}, Trói {stats.IsRooted}, Câm lặng {stats.IsSilenced} | DOT {dots.Count}");
    }
}