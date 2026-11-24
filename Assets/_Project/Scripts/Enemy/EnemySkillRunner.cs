using System.Collections.Generic;
using UnityEngine;

// Chạy các kỹ năng module của MỘT quái (mọi bậc: Normal, Elite, BOSS...).
// EnemyBase.Init tự thêm component này khi EnemyData có module (quái không có module thì không tốn gì).
[RequireComponent(typeof(EnemyBase))]
public class EnemySkillRunner : MonoBehaviour
{
    private EnemyBase enemy;
    private readonly List<PassiveSkillEffect> passives = new List<PassiveSkillEffect>();
    private readonly List<BossSkillRuntime> casts = new List<BossSkillRuntime>();

    private void Awake() { enemy = GetComponent<EnemyBase>(); }

    // EnemyBase.Init gọi mỗi lần lấy quái từ pool
    public void Setup(EnemyData data)
    {
        Cleanup();
        if (enemy == null) enemy = GetComponent<EnemyBase>();

        int level = GameDifficulty.Current;                    // Cấp kỹ năng theo độ khó (kẹp trong số cấp mỗi kỹ năng có)

        if (data.passiveModules != null)
        {
            foreach (PassiveSkillData p in data.passiveModules)
            {
                if (p == null) continue;

                PassiveSkillEffect effect = p.CreateEffect(enemy);
                if (effect == null)
                {
                    Debug.LogWarning($"EnemySkillRunner: '{p.name}' chỉ dành cho người chơi, không gắn được lên quái.", this);
                    continue;
                }

                effect.SetLevel(level);
                effect.Enable();
                passives.Add(effect);
            }
        }

        if (data.activeModules != null)
        {
            foreach (BossSkillData m in data.activeModules)
            {
                if (m == null) continue;

                BossSkillRuntime runtime = m.CreateRuntime(enemy, level);
                if (runtime == null) continue;

                runtime.Begin();
                casts.Add(runtime);
            }
        }

        enabled = passives.Count + casts.Count > 0;            // Không có gì để chạy thì tắt Update
    }

    private void Update()
    {
        if (enemy == null || enemy.Data == null) return;

        float dt = Time.deltaTime;

        // Passive luôn chạy (kể cả khi Dormant: "các skill vẫn hoạt động")
        for (int i = 0; i < passives.Count; i++) passives[i].Tick(dt);

        if (casts.Count == 0) return;

        // Dormant (Tối thượng) hoặc Choáng: các kỹ năng thi triển ngừng đếm giờ
        bool paused = enemy.IsDormant || enemy.IsStunned;
        float scaled = dt * enemy.AttackSpeedMultiplier;       // Tê cóng: chuẩn bị chiêu chậm hơn

        // Đếm trước: bao nhiêu kỹ năng exclusive đang thi triển, bao nhiêu đang trong giai đoạn thực hiện
        int busyExclusive = 0, executing = 0;
        for (int i = 0; i < casts.Count; i++)
        {
            BossSkillRuntime r = casts[i];
            if (r.Exclusive && r.Phase != BossCastPhase.Idle) busyExclusive++;
            if (r.Phase == BossCastPhase.Executing) executing++;
        }

        for (int i = 0; i < casts.Count; i++)
        {
            BossSkillRuntime r = casts[i];

            int busyOthers = busyExclusive - (r.Exclusive && r.Phase != BossCastPhase.Idle ? 1 : 0);
            int executingOthers = executing - (r.Phase == BossCastPhase.Executing ? 1 : 0);

            bool canStart = !r.Exclusive || busyOthers == 0;
            bool skillPaused = paused || (r.PausedWhileOthersExecute && executingOthers > 0);

            r.Tick(scaled, dt, skillPaused, canStart);
        }
    }

    // Quái về pool: gỡ sạch mọi thứ đã đăng ký
    private void OnDisable() { Cleanup(); }

    private void Cleanup()
    {
        for (int i = 0; i < passives.Count; i++) passives[i].Disable();
        passives.Clear();

        for (int i = 0; i < casts.Count; i++) casts[i].Cancel();
        casts.Clear();
    }
}