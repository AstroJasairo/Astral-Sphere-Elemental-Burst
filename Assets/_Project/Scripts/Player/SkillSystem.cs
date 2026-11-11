using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
public class SkillSystem : MonoBehaviour
{
    [Header("Debug (chỉ khi Debug.isDebugBuild). -1 = dùng cấp thật")]
    [SerializeField] private int debugPassiveLevel = -1;         // Áp dụng cho MỌI passive
    [SerializeField] private int debugActiveLevel = -1;
    [SerializeField] private bool debugKey = true;               // Phím Q kích hoạt skill chủ động

    public ActiveSkillData ActiveData { get; private set; }
    public int ActiveLevel { get; private set; }
    public int PassiveCount => passives.Count;                   // Số passive ĐANG CÓ HIỆU LỰC (cấp > 0)
    public bool PassivesSuppressed { get; private set; }

    public bool HasActive => ActiveData != null && ActiveLevel > 0 && active != null;
    public bool IsEffectActive { get; private set; }
    public bool IsSilenced { get; private set; }
    public event Action<bool> OnSilenceChanged;                  // Nút skill đọc để hiện overlay
    public event Action OnActivated;                             // UI/âm thanh nghe
    public event Action OnEffectEnded;

    private PlayerStats stats;
    private readonly List<PassiveSkillEffect> passives = new List<PassiveSkillEffect>();
    private ActiveSkillEffect active;
    private float effectEndTime, effectTotal;
    private float cooldownStartTime, cooldownEndTime, cooldownTotal;

    // ---- Dữ liệu cho UI (nút skill đọc mỗi frame) ----
    // IsReady: không kích hoạt được khi bị Câm lặng. Hồi chiêu vẫn chạy bình thường.
    public bool IsReady => HasActive && !IsSilenced && Time.time >= cooldownEndTime
                           && GameManager.Instance != null && !GameManager.Instance.IsEnded;
    public bool IsCooldownRunning => HasActive && Time.time >= cooldownStartTime && Time.time < cooldownEndTime;
    public float CooldownRemaining => HasActive ? Mathf.Max(0f, cooldownEndTime - Time.time) : 0f;
    public float CooldownRemaining01 => (HasActive && cooldownTotal > 0f) ? Mathf.Clamp01(CooldownRemaining / cooldownTotal) : 0f;
    public float EffectRemaining => IsEffectActive ? Mathf.Max(0f, effectEndTime - Time.time) : 0f;
    public float EffectRemaining01 => (IsEffectActive && effectTotal > 0f) ? Mathf.Clamp01(EffectRemaining / effectTotal) : 0f;

    private void Awake() { stats = GetComponent<PlayerStats>(); }

    private void OnEnable() { EnemyBase.EnemyKilled += HandleEnemyKilled; }
    private void OnDisable() { EnemyBase.EnemyKilled -= HandleEnemyKilled; }

    private void Start()                                         // Start: PlayerStats.Awake đã nạp nhân vật xong
    {
        CharacterData c = stats.Character;
        if (c == null) return;

        // Mọi passive có cấp > 0 đều được tạo hành vi và bật
        for (int i = 0; i < c.PassiveCount; i++)
        {
            PassiveSkillData data = c.GetPassive(i);
            int level = ResolveLevel(c, SkillKind.Passive, i, debugPassiveLevel);
            if (data == null || level <= 0) continue;

            PassiveSkillEffect effect = data.CreateEffect(stats);
            effect.SetLevel(level);
            effect.Enable();
            passives.Add(effect);
        }

        ActiveData = c.activeSkill;
        ActiveLevel = ResolveLevel(c, SkillKind.Active, 0, debugActiveLevel);
        if (ActiveData != null && ActiveLevel > 0)
        {
            active = ActiveData.CreateEffect(stats);
            // Đầu trận: hồi chiêu chạy từ đầu (theo mô tả)
            cooldownTotal = ActiveData.GetCooldown(ActiveLevel);
            cooldownStartTime = Time.time;
            cooldownEndTime = cooldownStartTime + cooldownTotal;
        }
    }

    private static int ResolveLevel(CharacterData c, SkillKind kind, int index, int debugLevel)
    {
        if (MetaProgress.GetSkillData(c, kind, index) == null) return 0;
        int level = (debugLevel >= 0 && Debug.isDebugBuild) ? debugLevel : MetaProgress.GetSkillLevel(c, kind, index);
        return Mathf.Clamp(level, 0, SkillDataBase.MaxLevel);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (!PassivesSuppressed)
            for (int i = 0; i < passives.Count; i++) passives[i].Tick(dt);

        if (IsEffectActive)
        {
            if (Time.time >= effectEndTime) EndEffect();
            else active.Tick(dt);
        }

        if (debugKey && Debug.isDebugBuild && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            TryActivate();
    }

    // Silence (11C) gọi: tạm tắt / bật lại MỌI passive
    public void SetPassivesSuppressed(bool suppressed)
    {
        if (PassivesSuppressed == suppressed) return;
        PassivesSuppressed = suppressed;
        for (int i = 0; i < passives.Count; i++)
        {
            if (suppressed) passives[i].Disable();
            else passives[i].Enable();
        }
    }

    // PlayerDebuffSystem gọi khi Silence bắt đầu/kết thúc
    public void SetSilenced(bool silenced)
    {
        if (IsSilenced == silenced) return;
        IsSilenced = silenced;

        SetPassivesSuppressed(silenced);                         // Tắt/bật lại MỌI passive
        if (silenced && IsEffectActive) ForceEndEffect();        // Skill chủ động đang chạy: kết thúc ngay
        OnSilenceChanged?.Invoke(silenced);
    }

    // Kết thúc sớm hiệu lực: gỡ buff, kiểu AfterEffectEnds thì hồi chiêu bắt đầu tính ngay
    private void ForceEndEffect()
    {
        IsEffectActive = false;
        active.End();
        stats.Buffs.Clear(BuffType.ActiveSkill);                 // Gỡ buff ATK và icon trên thanh buff

        if (ActiveData.cooldownStart == CooldownStart.AfterEffectEnds)
        {
            cooldownStartTime = Time.time;
            cooldownEndTime = cooldownStartTime + cooldownTotal;
        }
        OnEffectEnded?.Invoke();
    }

    public bool TryActivate()
    {
        if (!IsReady || GameManager.Instance.IsPaused) return false;

        float duration = ActiveData.GetDuration(ActiveLevel);
        float now = Time.time;

        if (IsEffectActive) active.End();                        // Kích hoạt lại khi đang hiệu lực (cooldown ngắn hơn hiệu lực): làm mới

        effectTotal = duration;
        effectEndTime = now + duration;
        IsEffectActive = true;

        // Hai kiểu hồi chiêu: tính từ lúc bấm, hoặc tính từ lúc hết hiệu lực
        cooldownTotal = ActiveData.GetCooldown(ActiveLevel);
        cooldownStartTime = ActiveData.cooldownStart == CooldownStart.OnActivate ? now : effectEndTime;
        cooldownEndTime = cooldownStartTime + cooldownTotal;

        active.Begin(ActiveLevel, duration);
        OnActivated?.Invoke();
        return true;
    }

    private void EndEffect()
    {
        IsEffectActive = false;
        active.End();
        OnEffectEnded?.Invoke();
    }

    // Đồng hồ cát: chỉ giảm khi hồi chiêu ĐÃ BẮT ĐẦU và chưa xong. Trả về true nếu có tác dụng.
    public bool ReduceCooldown(float seconds)
    {
        if (!HasActive || seconds <= 0f) return false;
        float now = Time.time;
        if (now < cooldownStartTime || now >= cooldownEndTime) return false;
        cooldownEndTime = Mathf.Max(now, cooldownEndTime - seconds);
        return true;
    }

    private void HandleEnemyKilled(EnemyBase enemy)
    {
        if (IsEffectActive) active.OnEnemyKilled(enemy);
    }

    [ContextMenu("Debug: Activate")] private void DebugActivate() { TryActivate(); }
    [ContextMenu("Debug: Reduce Cooldown 10s")] private void DebugReduce() { ReduceCooldown(10f); }
    [ContextMenu("Debug: Toggle Passives Suppressed")] private void DebugToggleSuppress() { SetPassivesSuppressed(!PassivesSuppressed); }
    [ContextMenu("Debug: Log State")]
    private void DebugLog()
    {
        Debug.Log($"[Skill] passive đang chạy {passives.Count} (bị tắt: {PassivesSuppressed}), active Lv{ActiveLevel}, " +
                  $"ready={IsReady}, effect={IsEffectActive} ({EffectRemaining:0.0}s), cooldown={CooldownRemaining:0.0}s");
    }
}