using System;
using UnityEngine;

// Hình quạt khuyết tâm (hoặc quạt đầy đủ khi innerRadius = 0) có góc mở spanDeg, căn giữa theo hướng object.
// Vùng cam quét từ MỘT CẠNH BÊN sang cạnh còn lại (như vung kiếm). Phần đã quét là vùng nguy hiểm.
//   clockwise = true : bắt đầu ở cạnh trái, quét sang phải (đúng như hình minh họa)
//   clockwise = false: bắt đầu ở cạnh phải, quét sang trái
public class SweepRingTelegraph : Telegraph
{
    private float innerRadius;
    private float outerRadius;
    private float span;
    private bool clockwise;

    public float InnerRadius => innerRadius;
    public float OuterRadius => outerRadius;
    public float Span => span;

    public void Begin(float innerRadius, float outerRadius, float spanDeg, bool clockwise, float duration,
                      float damage, Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        this.outerRadius = Mathf.Max(0.05f, outerRadius);
        this.innerRadius = Mathf.Clamp(innerRadius, 0f, this.outerRadius);
        this.span = Mathf.Clamp(spanDeg, 1f, 360f);
        this.clockwise = clockwise;

        StartCommon(duration, damage, onTriggered, follow);

        // Viền ngoài = toàn bộ vùng nguy hiểm
        outer.SetAnnulus(this.innerRadius, this.outerRadius, -span * 0.5f, span);
        Tick(0f);
    }

    protected override void Tick(float t)
    {
        float covered = span * t;                    // Góc đã quét được
        bool visible = covered > 0.5f;
        inner.Show(visible);
        if (!visible) return;

        // Góc tăng dần = ngược chiều kim đồng hồ. Chiều kim đồng hồ: bắt đầu ở cạnh +span/2 và lùi dần.
        float start = clockwise ? span * 0.5f - covered : -span * 0.5f;

        // allowFull = false: vùng cam luôn là một dải có hai cạnh bên, kể cả khi span = 360
        inner.SetAnnulus(innerRadius, outerRadius, start, covered, false);
    }

    protected override bool IsPlayerInside(Vector2 worldPosition)
    {
        return TelegraphGeometry.InRingSector(ToLocal(worldPosition), innerRadius, outerRadius, span, playerMargin);
    }
}