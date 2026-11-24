using UnityEngine;

// Component BOSS chung (thay ThunderBirdBoss và LordOfTheWolfBoss). KHÔNG chứa logic kỹ năng:
// kỹ năng là module trong EnemyData, chạy bởi EnemySkillRunner. Lớp này chỉ lo hiển thị miễn nhiễm cho HUD và màu thân.
public class BossController : BossBase
{
    [SerializeField] private SpriteRenderer bodyRenderer;                 // Hình thân (đổi màu khi miễn nhiễm sát thương). Tùy chọn
    [SerializeField] private Color immuneColor = new Color(0.55f, 0.65f, 0.9f);

    private Color normalColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        if (bodyRenderer != null) normalColor = bodyRenderer.color;
    }

    protected override void OnBegin() { SyncImmunity(true); }

    private void Update() { SyncImmunity(false); }

    // Miễn nhiễm sát thương (từ bất kỳ module nào) → báo HUD (icon Miễn nhiễm) và đổi màu thân
    private void SyncImmunity(bool force)
    {
        bool immune = Health.Immunities.Has(ImmunityFlags.Damage) || Health.Invulnerable;
        if (immune == IsImmune && !force) return;

        SetImmuneState(immune, false);                         // false: chỉ cập nhật hiển thị, không tự đặt bất tử

        if (bodyRenderer != null) bodyRenderer.color = immune ? immuneColor : normalColor;
    }
}