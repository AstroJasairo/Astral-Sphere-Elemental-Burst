// Hành vi runtime của skill bị động. Lớp thường (không MonoBehaviour), SkillSystem gọi mỗi frame.
public abstract class PassiveSkillEffect
{
    protected readonly PlayerStats stats;
    protected PassiveSkillEffect(PlayerStats stats) { this.stats = stats; }

    public abstract void SetLevel(int level);
    public virtual void Enable() { }       // Bắt đầu có hiệu lực: đăng ký nguồn giảm sát thương, hồi phục...
    public virtual void Disable() { }      // Tạm vô hiệu (Silence): PHẢI gỡ mọi thứ đã đăng ký
    public virtual void Tick(float deltaTime) { }

    // ----- Dữ liệu cho HUD (BuffHudView đọc mỗi frame). Mặc định: passive luôn bật không hiện icon -----

    // true: hiện icon viền XANH DƯƠNG (hiệu lực skill)
    public virtual bool IsEffectShown => false;
    // false: icon không có đồng hồ (ví dụ Brave chỉ có số stack)
    public virtual bool EffectUsesTimer => true;
    // 1 lúc mới kích hoạt, về 0 khi hết hiệu lực
    public virtual float EffectRemaining01 => 0f;
    // Hiện ô số góc dưới phải khi từ 2 trở lên
    public virtual int EffectStacks => 0;
    // Lớn hơn 0: hiện icon viền TÍM (đang hồi chiêu), giá trị = tỉ lệ thời gian hồi còn lại
    public virtual float CooldownRemaining01 => 0f;
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