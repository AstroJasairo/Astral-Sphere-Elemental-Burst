using System;
using UnityEngine;

public class ThunderBirdBoss : BossBase
{
    private enum State
    {
        Chasing,       // Đuổi theo người chơi như quái tầm xa
        DashWarning,   // Đứng yên, đường đỏ mở rộng
        Dashing        // Đang lao
    }

    [SerializeField] private GameObject strikeEffectPrefab;   // Chớp sáng khi sét đánh (prefab có TimedEffect). Tùy chọn

    private ThunderBirdData skills;
    private Rigidbody2D rb;
    private State state;
    private float strikeTimer;
    private float dashTimer;
    private float dashTimeLeft;
    private Vector2 dashDirection;

    // Lưu sẵn các hàm gọi lại để không tạo delegate mới mỗi lần thi triển (tránh sinh rác)
    private Action<Telegraph> strikeCallback;
    private Action<Telegraph> dashCallback;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        strikeCallback = OnStrikeLanded;
        dashCallback = OnDashWarningEnded;
    }

    protected override void OnBegin()
    {
        skills = Enemy.Data as ThunderBirdData;
        if (skills == null)
        {
            Debug.LogError("ThunderBirdBoss: EnemyData phải là ThunderBirdData.", this);
            return;
        }

        state = State.Chasing;
        strikeTimer = skills.firstStrikeDelay;
        dashTimer = skills.firstDashDelay;
    }

    private void Update()
    {
        if (skills == null) return;

        switch (state)
        {
            case State.Chasing:
                TickStrikes();
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f) BeginDashWarning();
                break;

            case State.DashWarning:
                TickStrikes();                    // Sét vẫn rơi trong lúc BOSS gồng
                break;

            case State.Dashing:
                dashTimeLeft -= Time.deltaTime;   // Đang lao thì không thi triển sét
                if (dashTimeLeft <= 0f) EndDash();
                break;
        }
    }

    // ---------- Nội tại: sét đánh ----------

    private void TickStrikes()
    {
        strikeTimer -= Time.deltaTime;
        if (strikeTimer > 0f) return;

        strikeTimer = skills.strikeInterval;
        CastLightningWave();
    }

    private void CastLightningWave()
    {
        Vector2 playerPos = player.position;

        for (int i = 0; i < skills.strikesPerWave; i++)
        {
            // Vòng đầu tiên nhắm đúng chỗ người chơi đang đứng (buộc phải di chuyển),
            // các vòng còn lại rải ngẫu nhiên quanh đó
            Vector2 center = i == 0 ? playerPos : playerPos + UnityEngine.Random.insideUnitCircle * skills.strikeSpread;

            // shrink = false: vòng cam lớn dần từ tâm, chạm viền thì sét đánh (dạng 1 trong bản vẽ)
            TelegraphSpawner.Instance.SpawnCircle(center, skills.strikeRadius, skills.strikeWarning,
                                                  false, skills.strikeDamage, strikeCallback);
        }
    }

    // Telegraph gọi khi vòng cam chạm viền: tạo hiệu ứng chớp sáng
    private void OnStrikeLanded(Telegraph t)
    {
        if (strikeEffectPrefab == null) return;
        var sector = t as SectorTelegraph;
        if (sector == null) return;

        GameObject fx = PoolManager.Instance.Get(strikeEffectPrefab, t.transform.position, Quaternion.identity);
        fx.GetComponent<TimedEffect>().Play(sector.Radius * 2f);
    }

    // ---------- Kỹ năng: lao tới ----------

    private void BeginDashWarning()
    {
        state = State.DashWarning;
        Enemy.OverrideMovement = true;            // Ngừng đuổi theo
        rb.linearVelocity = Vector2.zero;         // Đứng yên trong lúc gồng

        // Khóa hướng lao tại thời điểm này: đường đỏ không đuổi theo người chơi (đúng bản vẽ dạng 4)
        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)player.position - origin;
        dashDirection = toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : Vector2.right;
        float facing = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;

        // Dạng 4: vùng cam mở rộng từ giữa ra hai bên. damage = 0 vì sát thương do BOSS đâm vào người chơi
        // (cơ chế "chạm" có sẵn của EnemyBase).
        TelegraphSpawner.Instance.SpawnBox(origin, facing, skills.dashLength, skills.dashWidth,
                                           skills.dashWarning, RectFill.ExpandSideways, 0f, dashCallback);
    }

    // Telegraph gọi khi vùng cam chạm hai cạnh bên: bắt đầu lao
    private void OnDashWarningEnded(Telegraph t)
    {
        if (state != State.DashWarning) return;

        state = State.Dashing;
        dashTimeLeft = skills.dashLength / skills.dashSpeed;   // Lao đúng quãng đường của đường đỏ
        rb.linearVelocity = dashDirection * skills.dashSpeed;
    }

    private void EndDash()
    {
        state = State.Chasing;
        Enemy.OverrideMovement = false;           // Trả quyền di chuyển lại cho EnemyBase
        rb.linearVelocity = Vector2.zero;
        dashTimer = skills.dashInterval;
    }
}