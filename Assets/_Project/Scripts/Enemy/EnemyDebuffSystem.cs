using System.Collections.Generic;
using UnityEngine;

// Debuff và DOT của QUÁI. Tự thêm vào quái bởi EnemyBase (không cần sửa prefab).
// Debuff thường: lưu trong BuffController (thời điểm hết hạn, không Update).
// DOT: chỉ khi có DOT thì component này mới BẬT (enabled = true), nên 300 quái không tốn Update.
// Quái KHÔNG có icon buff (hiệu năng).
[RequireComponent(typeof(EnemyBase), typeof(BuffController))]
public class EnemyDebuffSystem : MonoBehaviour
{
    // Một DOT đang dính trên quái
    private class DotState
    {
        public DotData data;
        public int stacks;
        public float endTime;
        public float damagePerTick;      // Sát thương mỗi nhịp (đã chọn từ DotData hoặc từ cấp vũ khí)
        public float nextTickTime;
        public float scale = 1f;      // Hệ số sát thương (ATK người chơi lúc dính đòn)
    }

    // Kho dùng chung cho mọi quái: tái sử dụng bản ghi, không sinh rác
    private static readonly Stack<DotState> freeDots = new Stack<DotState>();
    private readonly List<DotState> dots = new List<DotState>();

    [Header("Debug (chỉ khi Debug.isDebugBuild): chuột phải component khi đang Play")]
    [SerializeField] private DotData debugDot;

    // Thuộc tính lấy lười: an toàn bất kể thứ tự Awake
    private EnemyBase enemy;
    private EnemyBase Enemy { get { if (enemy == null) enemy = GetComponent<EnemyBase>(); return enemy; } }
    private HealthSystem health;
    private HealthSystem Health { get { if (health == null) health = GetComponent<HealthSystem>(); return health; } }
    private BuffController buffs;
    private BuffController Buffs { get { if (buffs == null) buffs = GetComponent<BuffController>(); return buffs; } }
    private StatusEffectHandler status;
    private StatusEffectHandler Status { get { if (status == null) status = GetComponent<StatusEffectHandler>(); return status; } }

    // ---------------- Hệ số cho EnemyBase và BOSS đọc ----------------

    private float Value(BuffType type) { return Mathf.Clamp01(Buffs.GetValue(type)); }

    public float SpeedMultiplier => Mathf.Clamp(1f - Value(BuffType.Slow), 0.1f, 1f);                  // Slow
    public float AttackSpeedMultiplier => Mathf.Clamp(1f - Value(BuffType.Frostbite), 0.1f, 1f);        // Tê cóng
    public float DamageDealtMultiplier => 1f - Value(BuffType.Exhaust);                                // Giảm sát thương gây ra
    public float DamageTakenMultiplier => 1f + Mathf.Max(0f, Buffs.GetValue(BuffType.Vulnerable));      // Tăng sát thương nhận vào
    public bool IsStunned => Buffs.IsActive(BuffType.Stun);

    // ---------------- API ----------------

    // Chỉ các loại dưới đây tác động lên quái. Loại khác bị bỏ qua.
    // Root, Stun là KHỐNG CHẾ CỨNG: quái có statusImmune (BOSS) miễn nhiễm. Các loại còn lại BOSS vẫn dính.
    public bool ApplyDebuff(BuffType type, float value, float duration)
    {
        bool hard = type == BuffType.Root || type == BuffType.Stun;
        if (hard && Enemy.Data != null && Enemy.Data.statusImmune) return false;

        switch (type)
        {
            case BuffType.Root:
                Status.ApplyRoot(duration);               // Dùng lại cơ chế trói có sẵn
                return true;
            case BuffType.Stun:
            case BuffType.Slow:
            case BuffType.Frostbite:
            case BuffType.Vulnerable:
            case BuffType.Exhaust:
                return Buffs.ApplyDebuff(type, value, duration);   // Cùng loại: mạnh nhất thắng, bằng nhau thì reset thời gian
            default:
                return false;
        }
    }

    // Dính một stack DOT. Đã có thì +1 stack (tối đa maxStacksOverride, 0 = theo DotData) và reset thời gian chung.
    // damageOverride = 0: dùng damagePerTick của DotData. Lớn hơn 0: dùng giá trị của cấp vũ khí.
    public void ApplyDot(DotData data, int maxStacksOverride = 0, float scale = 1f, float damageOverride = 0f)
    {
        if (data == null) return;

        float now = Time.time;
        int cap = maxStacksOverride > 0 ? maxStacksOverride : data.maxStacks;
        float perTick = damageOverride > 0f ? damageOverride : data.damagePerTick;

        DotState dot = FindDot(data);
        if (dot == null)
        {
            dot = freeDots.Count > 0 ? freeDots.Pop() : new DotState();
            dot.data = data;
            dot.stacks = 1;
            dot.endTime = now + data.duration;
            dot.nextTickTime = now + data.tickInterval;
            dot.scale = scale;
            dot.damagePerTick = perTick;
            dots.Add(dot);
            enabled = true;
        }
        else
        {
            if (dot.stacks < cap) dot.stacks++;
            dot.endTime = now + data.duration;
            dot.scale = Mathf.Max(dot.scale, scale);
            dot.damagePerTick = Mathf.Max(dot.damagePerTick, perTick);   // Cùng DOT từ hai cấp khác nhau: lấy giá trị mạnh hơn
        }
    }

    // Áp danh sách hiệu ứng của một cấp vũ khí
    public void ApplyWeaponEffects(List<WeaponHitEffect> effects, float atkScale)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            WeaponHitEffect e = effects[i];
            if (e.chance < 1f && Random.value > e.chance) continue;

            if (e.kind == HitEffect.Kind.Dot)
                ApplyDot(e.dot, e.maxStacks, e.scaleWithAtk ? atkScale : 1f, e.damagePerTick);
            else
                ApplyDebuff(e.debuff, e.value, e.duration);
        }
    }

    // EnemyBase.Init gọi: quái lấy lại từ pool phải sạch hiệu ứng cũ
    public void ResetAll()
    {
        Buffs.ClearAll();
        ClearDots();
    }

    private void ClearDots()
    {
        for (int i = dots.Count - 1; i >= 0; i--) ReleaseDot(i);
        enabled = false;
    }

    private DotState FindDot(DotData data)
    {
        for (int i = 0; i < dots.Count; i++)
        {
            DotData d = dots[i].data;
            if (d == data || (!string.IsNullOrEmpty(d.id) && d.id == data.id)) return dots[i];
        }
        return null;
    }

    private void ReleaseDot(int index)
    {
        DotState dot = dots[index];
        dots.RemoveAt(index);
        dot.data = null;
        freeDots.Push(dot);
    }

    // ---------------- Nhịp DOT ----------------

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsEnded) return;

        float now = Time.time;                              // Dừng khi tạm dừng

        for (int i = dots.Count - 1; i >= 0; i--)
        {
            DotState d = dots[i];
            int guard = 0;                                  // Chốt an toàn: tối đa 8 nhịp mỗi frame

            while (now >= d.nextTickTime && d.nextTickTime <= d.endTime + 0.001f && guard++ < 8)
            {
                float damage = d.damagePerTick * d.scale;      // CŨ: d.data.damagePerTick * d.scale

                // Mỗi stack là MỘT đòn riêng: chịu DEF, kháng và miễn nhiễm nguyên tố của quái
                for (int s = 0; s < d.stacks; s++)
                {
                    Health.TakeHit(damage, d.data.element);
                    if (Health.IsDead) return;              // Quái chết: EnemyBase đã trả về pool và OnDisable đã dọn DOT
                }
                d.nextTickTime += d.data.tickInterval;
            }

            if (now >= d.endTime) ReleaseDot(i);
        }

        if (dots.Count == 0) enabled = false;               // Hết DOT: tắt Update
    }

    // Quái bị trả về pool khi còn DOT: dọn sạch để lần sau không dính
    private void OnDisable()
    {
        for (int i = dots.Count - 1; i >= 0; i--) ReleaseDot(i);
    }

    // ---------------- Debug ----------------

    [ContextMenu("Debug: Apply DOT")] private void DebugDot() { if (Debug.isDebugBuild) ApplyDot(debugDot); }
    [ContextMenu("Debug: Slow 50% 5s")] private void DebugSlow() { if (Debug.isDebugBuild) ApplyDebuff(BuffType.Slow, 0.5f, 5f); }
    [ContextMenu("Debug: Stun 3s")] private void DebugStun() { if (Debug.isDebugBuild) ApplyDebuff(BuffType.Stun, 1f, 3f); }
    [ContextMenu("Debug: Frostbite 50% 5s")] private void DebugFrost() { if (Debug.isDebugBuild) ApplyDebuff(BuffType.Frostbite, 0.5f, 5f); }
    [ContextMenu("Debug: Exhaust 50% 5s")] private void DebugExhaust() { if (Debug.isDebugBuild) ApplyDebuff(BuffType.Exhaust, 0.5f, 5f); }
    [ContextMenu("Debug: Vulnerable 50% 5s")] private void DebugVulnerable() { if (Debug.isDebugBuild) ApplyDebuff(BuffType.Vulnerable, 0.5f, 5f); }
}