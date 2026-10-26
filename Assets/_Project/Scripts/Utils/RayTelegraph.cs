using System;
using UnityEngine;

// Tia đỏ ngắm theo người chơi (đi xuyên qua người chơi). Gần tới lúc bắn thì khóa hướng,
// rồi chớp tắt liên tục cho tới khi khai hỏa.
public class RayTelegraph : Telegraph
{
    [SerializeField] private float blinkInterval = 0.08f;   // Chu kỳ chớp tắt sau khi khóa (giây)

    private float length;
    private float width;
    private float lockTime;    // Thời điểm khóa hướng (tính từ lúc bắt đầu)

    // Hướng bắn đã khóa. BOSS đọc trong onTriggered để bắn đạn đúng hướng tia đỏ.
    public Vector2 Direction => transform.right;
    public bool IsLocked => elapsed >= lockTime;

    // lockFraction: 0.7 nghĩa là 70% thời gian đầu ngắm theo người chơi, 30% còn lại khóa và chớp
    public void Begin(float length, float width, float duration, float lockFraction, float damage,
                      Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        this.length = length;
        this.width = width;
        StartCommon(duration, damage, onTriggered, follow);
        lockTime = this.duration * Mathf.Clamp01(lockFraction);

        // Tia là một thanh chữ nhật mảnh, dài length
        int n = TelegraphGeometry.Box(points, 0f, length, -width * 0.5f, width * 0.5f);
        outer.SetPolygon(points, n, TelegraphGeometry.Centroid(points, n));

        Tick(0f);
    }

    protected override void Tick(float t)
    {
        if (elapsed < lockTime)
        {
            AimAtPlayer();           // Còn đang ngắm: bám theo người chơi
            outer.Show(true);
            return;
        }

        // Đã khóa: giữ nguyên hướng, chớp tắt
        int phase = (int)((elapsed - lockTime) / blinkInterval);
        outer.Show(phase % 2 == 0);
    }

    private void AimAtPlayer()
    {
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);   // Xoay cả tia về phía người chơi
    }

    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InBox(ToLocal(worldPosition), 0f, length, width * 0.5f, playerMargin);
    }
}