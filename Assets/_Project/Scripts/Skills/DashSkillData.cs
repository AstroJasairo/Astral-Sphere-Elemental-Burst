using System;
using UnityEngine;

// Lao tới: đứng lại gồng (vùng cam mở rộng ra hai bên), rồi lao theo đường đã khóa.
// Sát thương của cú lao là sát thương CHẠM của quái (luôn gây nếu trúng, dù Tê cóng).
[CreateAssetMenu(menuName = "Astral Sphere/Skills/Boss/Dash")]
public class DashSkillData : BossSkillData
{
    [Serializable]
    public class DashLevel : BossSkillLevel
    {
        public float warning = 1.5f;
        public float length = 14f;
        public float width = 3.2f;         // Nên bằng đường kính thân cộng chút lề
        public float speed = 18f;
    }

    [Header("Theo cấp (1 cấp mặc định)")]
    public DashLevel[] levels = { new DashLevel { firstDelay = 6f, cooldown = 8f } };

    public override int LevelCount => levels != null ? levels.Length : 0;

    public override BossSkillRuntime CreateRuntime(EnemyBase enemy, int level)
    {
        DashLevel l = Pick(levels, level);
        return l == null ? null : new DashRuntime(enemy, l, this);
    }

    private void Reset() { exclusive = true; pausedWhileOthersExecute = false; }

    [ContextMenu("Fill Thunder Bird Defaults")]
    private void FillDefaults()
    {
        skillName = "Lao tới";
        mode = BossSkillMode.Cooldown;
        exclusive = true;
        pausedWhileOthersExecute = false;
        levels = new[] { new DashLevel { firstDelay = 6f, cooldown = 8f, warning = 1.5f, length = 14f, width = 3.2f, speed = 18f } };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public class DashRuntime : BossSkillRuntime
{
    private readonly DashSkillData.DashLevel level;
    private readonly Rigidbody2D rb;
    private readonly Action<Telegraph> warningEnded;

    private Vector2 direction;
    private float timeLeft;

    public DashRuntime(EnemyBase enemy, DashSkillData.DashLevel level, DashSkillData data) : base(enemy, data)
    {
        this.level = level;
        rb = enemy.Body;
        warningEnded = OnWarningEnded;
    }

    public override void Begin()
    {
        Phase = BossCastPhase.Idle;
        timer = level.firstDelay;
    }

    public override void Tick(float scaledDt, float rawDt, bool paused, bool canStart)
    {
        switch (Phase)
        {
            case BossCastPhase.Idle:
                if (CountdownReady(scaledDt, paused, canStart)) BeginWarning();
                break;

            case BossCastPhase.Executing:
                timeLeft -= rawDt;                                    // Đang lao: không bị Tê cóng ảnh hưởng
                rb.position = MapArea.Clamp(rb.position, 1f);         // Cú lao dừng ở mép biên, không bay ra ngoài
                if (timeLeft <= 0f) EndDash();
                break;
        }                                                             // Warning: chờ telegraph gọi OnWarningEnded
    }

    private void BeginWarning()
    {
        Phase = BossCastPhase.Warning;
        enemy.OverrideMovement = true;                // Ngừng đuổi theo
        rb.linearVelocity = Vector2.zero;             // Đứng yên trong lúc gồng

        // Khóa hướng lao tại thời điểm này: đường đỏ không đuổi theo người chơi
        Vector2 origin = enemy.transform.position;
        Transform target = enemy.Target;
        Vector2 toTarget = target != null ? (Vector2)target.position - origin : Vector2.right;
        direction = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector2.right;
        float facing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // damage = 0 vì sát thương do BOSS đâm vào người chơi (cơ chế "chạm" có sẵn của EnemyBase)
        TelegraphSpawner.Instance.SpawnBox(origin, facing, level.length, level.width,
                                           level.warning / enemy.AttackSpeedMultiplier,
                                           RectFill.ExpandSideways, 0f, warningEnded,
                                           element: enemy.Data.damageElement);
    }

    // Telegraph gọi khi vùng cam chạm hai cạnh bên: bắt đầu lao
    private void OnWarningEnded(Telegraph t)
    {
        if (Phase != BossCastPhase.Warning) return;

        Phase = BossCastPhase.Executing;
        float speed = Mathf.Max(0.1f, level.speed * enemy.SpeedMultiplier);   // Slow làm cú lao chậm đi (quãng đường không đổi)
        timeLeft = level.length / speed;
        rb.linearVelocity = direction * speed;

        enemy.ResetContactCooldown();                 // Đòn lao luôn gây sát thương nếu trúng, dù đang trong hồi chiêu chạm
    }

    private void EndDash()
    {
        enemy.OverrideMovement = false;               // Trả quyền di chuyển lại cho EnemyBase
        rb.linearVelocity = Vector2.zero;
        FinishCast(level.cooldown);
    }

    public override void Cancel()
    {
        if (Phase == BossCastPhase.Idle) return;
        Phase = BossCastPhase.Idle;
        enemy.OverrideMovement = false;
    }
}