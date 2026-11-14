using System;
using UnityEngine;

// Nhẫn (angleDeg = 360) hoặc nón nhẫn (angleDeg < 360): vùng nguy hiểm nằm giữa hai bán kính.
// Vùng cam lan từ vòng trong ra vòng ngoài (inward = false) hoặc từ vòng ngoài vào vòng trong (inward = true).
// Hướng của nón nhẫn = hướng xoay của object (do TelegraphSpawner/vũ khí đặt).
public class RingTelegraph : Telegraph
{
    private float innerRadius;
    private float outerRadius;
    private float angle;
    private bool inward;

    public float InnerRadius => innerRadius;
    public float OuterRadius => outerRadius;
    public float Angle => angle;

    public void Begin(float innerRadius, float outerRadius, float angleDeg, float duration, bool inward,
                      float damage, Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        this.outerRadius = Mathf.Max(0.05f, outerRadius);
        this.innerRadius = Mathf.Clamp(innerRadius, 0f, this.outerRadius);
        this.angle = Mathf.Clamp(angleDeg, 1f, 360f);
        this.inward = inward;

        StartCommon(duration, damage, onTriggered, follow);

        // Viền ngoài cố định: vẽ một lần
        outer.SetAnnulus(this.innerRadius, this.outerRadius, -angle * 0.5f, angle);
        Tick(0f);
    }

    protected override void Tick(float t)
    {
        // Vùng cam là một "nhẫn mỏng hơn" nằm trong nhẫn lớn
        float r0, r1;
        if (inward)
        {
            r0 = Mathf.Lerp(outerRadius, innerRadius, t);   // Cạnh trong co dần về vòng trong
            r1 = outerRadius;
        }
        else
        {
            r0 = innerRadius;
            r1 = Mathf.Lerp(innerRadius, outerRadius, t);   // Cạnh ngoài lan dần ra vòng ngoài
        }

        bool visible = r1 - r0 > 0.02f;                      // Quá mỏng thì ẩn, tránh viền lạ
        inner.Show(visible);
        if (!visible) return;

        inner.SetAnnulus(r0, r1, -angle * 0.5f, angle);
    }

    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InRingSector(ToLocal(worldPosition), innerRadius, outerRadius, angle, playerMargin);
    }
}