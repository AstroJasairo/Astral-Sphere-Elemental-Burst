// Hành vi runtime của skill bị động. Lớp thường (không MonoBehaviour), SkillSystem gọi mỗi frame.
public abstract class PassiveSkillEffect
{
    protected readonly PlayerStats stats;
    protected PassiveSkillEffect(PlayerStats stats) { this.stats = stats; }

    public abstract void SetLevel(int level);
    public virtual void Enable() { }       // Bắt đầu có hiệu lực: đăng ký nguồn giảm sát thương, hồi phục...
    public virtual void Disable() { }      // Tạm vô hiệu (Silence): PHẢI gỡ mọi thứ đã đăng ký
    public virtual void Tick(float deltaTime) { }
}

// Hành vi runtime của skill chủ động
public abstract class ActiveSkillEffect
{
    protected readonly PlayerStats stats;
    protected ActiveSkillEffect(PlayerStats stats) { this.stats = stats; }
    public abstract void Begin(int level, float duration);      // Bắt đầu hiệu lực
    public virtual void Tick(float deltaTime) { }               // Mỗi frame khi đang hiệu lực
    public virtual void OnEnemyKilled(EnemyBase enemy) { }      // Khi hạ gục quái lúc đang hiệu lực
    public abstract void End();
}