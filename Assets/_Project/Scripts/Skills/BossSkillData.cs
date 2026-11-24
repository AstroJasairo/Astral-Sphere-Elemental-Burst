using System;
using UnityEngine;

public enum BossSkillMode { Cooldown, OneTime }
public enum BossCastPhase { Idle, Warning, Executing }

// Thông số chung của mỗi cấp một kỹ năng thi triển. Lớp con thêm thông số riêng.
[Serializable]
public class BossSkillLevel
{
    [Tooltip("Chờ bao nhiêu giây sau khi xuất hiện thì thi triển lần đầu")]
    public float firstDelay = 3f;
    [Tooltip("Chế độ Cooldown: số giây giữa hai lần thi triển. Chế độ OneTime: bỏ qua")]
    public float cooldown = 6f;
}

// Kỹ năng thi triển (có hồi chiêu) của quái/BOSS, dạng module. Mỗi asset dùng lại được cho nhiều đối tượng.
public abstract class BossSkillData : ScriptableObject
{
    public string skillName;

    [Tooltip("Cooldown: lặp lại sau mỗi 'cooldown' giây. OneTime: chỉ thi triển MỘT lần (sau firstDelay)")]
    public BossSkillMode mode = BossSkillMode.Cooldown;

    [Tooltip("Bật: không chạy cùng lúc với kỹ năng exclusive khác (Lao tới, Cào)")]
    public bool exclusive = true;

    [Tooltip("Bật: tạm dừng đếm giờ khi một kỹ năng khác đang trong giai đoạn thực hiện (Sét ngừng khi BOSS đang lao)")]
    public bool pausedWhileOthersExecute = false;

    public abstract int LevelCount { get; }
    public abstract BossSkillRuntime CreateRuntime(EnemyBase enemy, int level);

    // Lấy dữ liệu của cấp (1 = phần tử 0), kẹp trong mảng
    protected static T Pick<T>(T[] levels, int level) where T : class
    {
        if (levels == null || levels.Length == 0) return null;
        return levels[Mathf.Clamp(level, 1, levels.Length) - 1];
    }
}

// Hành vi runtime của một kỹ năng thi triển. Lớp thường, EnemySkillRunner gọi mỗi frame.
public abstract class BossSkillRuntime
{
    protected readonly EnemyBase enemy;
    private readonly BossSkillData baseData;
    protected float timer;
    private bool spent;                                       // Chế độ OneTime: đã thi triển xong

    public BossCastPhase Phase { get; protected set; }
    public bool Exclusive => baseData.exclusive;
    public bool PausedWhileOthersExecute => baseData.pausedWhileOthersExecute;

    protected BossSkillRuntime(EnemyBase enemy, BossSkillData data)
    {
        this.enemy = enemy;
        baseData = data;
    }

    public abstract void Begin();                             // Đặt đồng hồ lần đầu

    // scaledDt: đã nhân tốc độ tấn công (Tê cóng). rawDt: thời gian thật (đòn lao không bị Tê cóng).
    // paused: không đếm giờ. canStart: false khi kỹ năng exclusive khác đang thi triển.
    public abstract void Tick(float scaledDt, float rawDt, bool paused, bool canStart);

    public virtual void Cancel() { }                          // Quái về pool giữa chừng

    // true khi đã đến lúc thi triển. Chưa được phép bắt đầu thì giữ trạng thái "sẵn sàng" chờ
    protected bool CountdownReady(float scaledDt, bool paused, bool canStart)
    {
        if (Phase != BossCastPhase.Idle || spent || paused) return false;

        timer -= scaledDt;
        if (timer > 0f) return false;

        if (!canStart) { timer = 0f; return false; }
        return true;
    }

    // Gọi khi thi triển xong: Cooldown thì đặt lại đồng hồ, OneTime thì khóa vĩnh viễn
    protected void FinishCast(float cooldown)
    {
        Phase = BossCastPhase.Idle;
        if (baseData.mode == BossSkillMode.OneTime) spent = true;
        else timer = Mathf.Max(0.05f, cooldown);
    }
}