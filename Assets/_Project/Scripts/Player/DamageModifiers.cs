using System.Collections.Generic;
using UnityEngine;

// Tập các nguồn GIẢM SÁT THƯƠNG đang hiệu lực. Mỗi nguồn tự đăng ký (Set) và tự gỡ (Remove).
public class DamageReductionSet
{
    public const int AllScope = -1;                 // Nguồn giảm MỌI sát thương (trừ Pure). Nguồn theo nguyên tố dùng (int)ElementType

    private struct Entry { public object source; public int scope; public float value; }
    private readonly List<Entry> entries = new List<Entry>();

    // Hệ số tương đối lên mọi hiệu ứng giảm (giai đoạn 11C: Vulnerable). 1 = nguyên giá trị.
    public float EffectScale { get; set; } = 1f;

    // Đặt giá trị của một nguồn (đã có thì cập nhật; value <= 0 thì gỡ)
    public void Set(object source, int scope, float value)
    {
        value = Mathf.Clamp01(value);
        for (int i = 0; i < entries.Count; i++)
        {
            if (!Equals(entries[i].source, source) || entries[i].scope != scope) continue;
            if (value <= 0f) entries.RemoveAt(i);
            else entries[i] = new Entry { source = source, scope = scope, value = value };
            return;
        }
        if (value > 0f) entries.Add(new Entry { source = source, scope = scope, value = value });
    }

    // Gỡ mọi mục của một nguồn
    public void Remove(object source)
    {
        for (int i = entries.Count - 1; i >= 0; i--)
            if (Equals(entries[i].source, source)) entries.RemoveAt(i);
    }

    // Tổng giảm cho một nguyên tố, 0..maxTotalReduction. Pure luôn là 0.
    public float GetTotalReduction(ElementType element)
    {
        if (element == ElementType.Pure) return 0f;

        CombatRulesConfig rules = CombatRules.Config;
        bool multiply = rules.reductionStacking == StackMode.Multiplicative;
        int elementScope = (int)element;

        float allKeep = 1f, allSum = 0f;            // nhóm "chung"
        float elKeep = 1f, elSum = 0f;              // nhóm "nguyên tố này"
        for (int i = 0; i < entries.Count; i++)
        {
            Entry e = entries[i];
            if (e.scope == AllScope) { allKeep *= 1f - e.value; allSum += e.value; }
            else if (e.scope == elementScope) { elKeep *= 1f - e.value; elSum += e.value; }
        }

        float rAll = multiply ? 1f - allKeep : allSum;       // cùng nhóm: NHÂN tích lũy (hoặc cộng nếu đổi luật)
        float rElement = multiply ? 1f - elKeep : elSum;
        float total = (rAll + rElement) * EffectScale;       // khác nhóm: CỘNG
        return Mathf.Clamp(total, 0f, rules.maxTotalReduction);
    }
}

// Tập các nguồn HIỆU QUẢ HỒI PHỤC (0.15 = +15% lượng hồi)
public class HealingBonusSet
{
    private struct Entry { public object source; public float value; }
    private readonly List<Entry> entries = new List<Entry>();

    public void Set(object source, float value)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (!Equals(entries[i].source, source)) continue;
            if (value <= 0f) entries.RemoveAt(i);
            else entries[i] = new Entry { source = source, value = value };
            return;
        }
        if (value > 0f) entries.Add(new Entry { source = source, value = value });
    }

    public void Remove(object source)
    {
        for (int i = entries.Count - 1; i >= 0; i--)
            if (Equals(entries[i].source, source)) entries.RemoveAt(i);
    }

    // Hệ số nhân lượng hồi: 1.265 nghĩa là hồi nhiều hơn 26.5%
    public float Multiplier
    {
        get
        {
            bool multiply = CombatRules.Config.healingStacking == StackMode.Multiplicative;
            float product = 1f, sum = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                product *= 1f + entries[i].value;
                sum += entries[i].value;
            }
            return multiply ? product : 1f + sum;
        }
    }
}