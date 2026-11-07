using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
public class SkillSystem : MonoBehaviour
{
    [Header("Debug (chỉ khi Debug.isDebugBuild). -1 = dùng cấp thật")]
    [SerializeField] private int debugPassiveLevel = -1;
    [SerializeField] private int debugActiveLevel = -1;
    [SerializeField] private bool debugKey = true;               // Phím Q kích hoạt skill chủ động

    public PassiveSkillData PassiveData { get; private set; }
    public ActiveSkillData ActiveData { get; private set; }
    public int PassiveLevel { get; private set; }
    public int ActiveLevel { get; private set; }

    public bool HasActive => ActiveData != null && ActiveLevel > 0 && active != null;
    public bool IsEffectActive { get; private set; }
    public event Action OnActivated;                             // UI/âm thanh nghe
    public event Action OnEffectEnded;

    private PlayerStats stats;
    private PassiveSkillEffect passive;
    private ActiveSkillEffect active;
    private float effectEndTime, effectTotal;
    private float cooldownStartTime, cooldownEndTime, cooldownTotal;

    // ---- Dữ liệu cho UI (nút skill đọc mỗi frame) ----
    public bool IsReady => HasActive && Time.time >= cooldownEndTime
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

        PassiveData = c.passiveSkill;
        ActiveData = c.activeSkill;
        PassiveLevel = ResolveLevel(PassiveData, SkillKind.Passive, c, debugPassiveLevel);
        ActiveLevel = ResolveLevel(ActiveData, SkillKind.Active, c, debugActiveLevel);

        if (PassiveData != null && PassiveLevel > 0)
        {
            passive = PassiveData.CreateEffect(stats);
            passive.SetLevel(PassiveLevel);
        }
        if (ActiveData != null && ActiveLevel > 0)
        {
            active = ActiveData.CreateEffect(stats);
            // Đầu trận: hồi chiêu chạy từ đầu (theo mô tả)
            cooldownTotal = ActiveData.GetCooldown(ActiveLevel);
            cooldownStartTime = Time.time;
            cooldownEndTime = cooldownStartTime + cooldownTotal;
        }
    }

    private static int ResolveLevel(SkillDataBase data, SkillKind kind, CharacterData c, int debugLevel)
    {
        if (data == null) return 0;
        int level = (debugLevel >= 0 && Debug.isDebugBuild) ? debugLevel : MetaProgress.GetSkillLevel(c.id, kind);
        return Mathf.Clamp(level, 0, SkillDataBase.MaxLevel);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (passive != null) passive.Tick(dt);

        if (IsEffectActive)
        {
            if (Time.time >= effectEndTime) EndEffect();
            else active.Tick(dt);
        }

        if (debugKey && Debug.isDebugBuild && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            TryActivate();
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
    [ContextMenu("Debug: Log State")]
    private void DebugLog()
    {
        Debug.Log($"[Skill] passive Lv{PassiveLevel}, active Lv{ActiveLevel}, ready={IsReady}, effect={IsEffectActive} ({EffectRemaining:0.0}s), cooldown={CooldownRemaining:0.0}s");
    }
}