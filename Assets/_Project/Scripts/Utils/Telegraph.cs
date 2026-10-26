using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class Telegraph : MonoBehaviour
{
    [Header("Giao diện")]
    [SerializeField] protected Material material;        // Material dùng shader Sprite Unlit (xem hướng dẫn dựng)
    [SerializeField] protected Color outerFill = new Color(1f, 0.15f, 0.15f, 0.18f);
    [SerializeField] protected Color outerBorder = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] protected Color innerFill = new Color(1f, 0.55f, 0.1f, 0.40f);
    [SerializeField] protected Color innerBorder = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] protected float borderWidth = 0.08f;
    [SerializeField] protected int outerSortingOrder = -8;   // PHẢI cao hơn Order in Layer của nền
    [SerializeField] protected int innerSortingOrder = -6;

    [Header("Trúng đòn")]
    [SerializeField] protected float playerMargin = 0.3f;    // Bán kính thân người chơi: vừa chạm mép là trúng

    protected TelegraphVisual outer;                         // Lớp ngoài: viền kỹ năng (đỏ, cố định)
    protected TelegraphVisual inner;                         // Lớp trong: tiến độ (cam, đổi theo thời gian)
    protected readonly Vector2[] points = new Vector2[TelegraphGeometry.MaxPoints];   // Bộ đệm dùng lại

    protected float duration;
    protected float elapsed;                                 // Thời gian đã trôi qua (giây)
    protected float damage;
    protected Transform playerTransform;
    protected PlayerStats playerStats;

    private static readonly HashSet<Telegraph> active = new HashSet<Telegraph>();
    private static readonly List<Telegraph> cancelBuffer = new List<Telegraph>();   // Dùng lại, không sinh rác

    private Action<Telegraph> onTriggered;
    private Transform followTarget;
    private bool finished = true;

    // Mặc định gây sát thương một lần khi hết giờ. SweepTelegraph tắt đi vì gây sát thương lúc quay.
    protected virtual bool DamageOnFinish => true;

    protected virtual void Awake()
    {
        outer = new TelegraphVisual(transform, "Outer", material, outerSortingOrder, outerFill, outerBorder, borderWidth);
        inner = new TelegraphVisual(transform, "Inner", material, innerSortingOrder, innerFill, innerBorder, borderWidth);
    }

    private void OnDestroy()
    {
        outer?.Destroy();
        inner?.Destroy();
    }

    // Các lớp con gọi ở đầu hàm Begin
    protected void StartCommon(float duration, float damage, Action<Telegraph> onTriggered, Transform follow)
    {
        this.duration = Mathf.Max(0.05f, duration);
        this.damage = damage;
        this.onTriggered = onTriggered;
        followTarget = follow;
        elapsed = 0f;
        finished = false;

        playerTransform = GameManager.Instance.Player;
        if (playerStats == null && playerTransform != null)
            playerTransform.TryGetComponent(out playerStats);   // Chỉ lấy một lần

        outer.Show(true);
        inner.Show(false);   // Lớp trong do lớp con tự bật khi cần
        active.Add(this);
    }

    private void Update()
    {
        if (finished) return;

        if (followTarget != null)
            transform.position = followTarget.position;         // Bám theo kẻ thi triển (nếu có)

        elapsed += Time.deltaTime;                              // Tự dừng khi game tạm dừng (timeScale = 0)
        Tick(Mathf.Clamp01(elapsed / duration));

        if (elapsed >= duration) Finish();
    }

    private void Finish()
    {
        finished = true;
        active.Remove(this);

        if (DamageOnFinish && damage > 0f && playerStats != null && IsPlayerInside(playerTransform.position))
            playerStats.TakeDamage(damage);

        onTriggered?.Invoke(this);                              // BOSS làm hiệu ứng (sét đánh, lao tới...)
        PoolManager.Instance.Release(gameObject);
    }

    // Hủy cảnh báo giữa chừng, không gây sát thương, không gọi onTriggered (ví dụ BOSS chết)
    public void Cancel()
    {
        if (finished) return;
        finished = true;
        active.Remove(this);
        PoolManager.Instance.Release(gameObject);
    }

    // Mỗi frame: cập nhật hình. t chạy từ 0 đến 1 theo thời gian.
    protected abstract void Tick(float t);

    // Điểm (tọa độ thế giới) có nằm trong vùng nguy hiểm hiện tại không
    protected abstract bool IsPlayerInside(Vector2 worldPosition);

    protected Vector2 ToLocal(Vector2 worldPosition)
    {
        return transform.InverseTransformPoint(worldPosition);
    }

    protected void DealDamage()
    {
        if (playerStats != null) playerStats.TakeDamage(damage);
    }

    public static void ResetStatics() { active.Clear(); }

    private void OnDisable() { active.Remove(this); }

    public static void CancelAll()
    {
        cancelBuffer.Clear();
        cancelBuffer.AddRange(active);          // Sao chép ra danh sách phụ vì Cancel() sẽ sửa tập "active"
        foreach (Telegraph t in cancelBuffer) t.Cancel();
    }
}