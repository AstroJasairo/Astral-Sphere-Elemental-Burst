using System.Collections.Generic;
using UnityEngine;

// Giảm sát thương TẠM THỜI của BOSS. Mỗi lần Add là một mục có giá trị và thời hạn riêng.
// Nhiều mục cùng lúc: NHÂN tích lũy (50% và 50% => nhận 25%), hoặc không cộng dồn nếu tắt trong CombatRules.
[RequireComponent(typeof(HealthSystem))]
public class BossDamageReduction : MonoBehaviour
{
    private struct Entry { public float value; public float endTime; public float duration; }

    private readonly List<Entry> entries = new List<Entry>();
    private HealthSystem health;

    public int Stacks => entries.Count;                         // Số lớp đang hiệu lực (HUD hiện số khi từ 2)

    // Tỉ lệ thời gian còn lại của lớp còn lâu nhất (cho vòng đồng hồ trên icon)
    public float Remaining01
    {
        get
        {
            float now = Time.time, best = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                Entry e = entries[i];
                float r = e.duration > 0f ? (e.endTime - now) / e.duration : 0f;
                if (r > best) best = r;
            }
            return Mathf.Clamp01(best);
        }
    }

    private void Awake() { health = GetComponent<HealthSystem>(); }
    private void OnEnable() { Clear(); }                        // BOSS lấy lại từ pool luôn bắt đầu sạch

    public void Clear()
    {
        entries.Clear();
        Apply();
    }

    public void Add(float value, float duration)
    {
        value = Mathf.Clamp01(value);
        if (value <= 0f || duration <= 0f) return;

        float now = Time.time;
        if (!CombatRules.Config.overlappingBossReductionsStack && entries.Count > 0)
        {
            // Chế độ KHÔNG cộng dồn: chỉ giữ một mục, lấy giá trị lớn hơn, làm mới thời gian
            Entry e = entries[0];
            e.value = Mathf.Max(e.value, value);
            e.endTime = now + duration;
            e.duration = duration;
            entries[0] = e;
        }
        else
        {
            entries.Add(new Entry { value = value, endTime = now + duration, duration = duration });
        }
        Apply();
    }

    private void Update()                                       // Chỉ làm việc khi có lớp đang chạy
    {
        if (entries.Count == 0) return;

        float now = Time.time;                                  // Dừng khi tạm dừng
        bool changed = false;
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (now < entries[i].endTime) continue;
            entries.RemoveAt(i);
            changed = true;
        }
        if (changed) Apply();
    }

    private void Apply()
    {
        float multiplier = 1f;
        for (int i = 0; i < entries.Count; i++) multiplier *= 1f - entries[i].value;   // cùng loại: nhân tích lũy
        health.DamageTakenMultiplier = multiplier;
    }
}