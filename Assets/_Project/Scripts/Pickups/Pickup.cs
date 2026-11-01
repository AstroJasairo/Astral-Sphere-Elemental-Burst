using System;
using System.Collections.Generic;
using UnityEngine;

// Gắn vào prefab vật phẩm (vàng, pha lê...). Nằm yên cho đến khi người chơi vào tầm hút.
[RequireComponent(typeof(Collider2D))]
public class Pickup : MonoBehaviour
{
    // Sự kiện toàn cục "có vật phẩm được nhặt": (loại, số lượng)
    public static event Action<PickupType, int> Collected;

    // Tập vật phẩm đang có trên sân (dùng để đếm và để gộp)
    public static readonly HashSet<Pickup> Active = new HashSet<Pickup>();
    public static int ActiveCount => Active.Count;
    public static void ResetStatics() => Active.Clear();           // GameManager gọi khi vào trận mới

    [SerializeField] private float startSpeed = 3f;                 // Tốc độ lúc bắt đầu bị hút
    [SerializeField] private float acceleration = 25f;              // Càng bay càng nhanh để không bị người chơi chạy thoát
    [SerializeField] private float collectDistance = 0.3f;          // Cách người chơi bao xa thì tính là đã nhặt
    [SerializeField] private float scalePerExtra = 0.08f;           // Mỗi đơn vị gộp thêm làm vật phẩm to thêm chừng này
    [SerializeField] private float maxMergeScale = 2f;              // Tối đa to gấp đôi

    public PickupType Type { get; private set; }
    public int Amount { get; private set; }
    public bool IsAttracted => target != null;                      // Đã bị hút (đang bay về người chơi)

    private Collider2D col;
    private Transform target;
    private float speed;
    private Vector3 baseScale;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        baseScale = transform.localScale;                           // Kích thước gốc của prefab
    }

    private void OnEnable() { Active.Add(this); }
    private void OnDisable() { Active.Remove(this); }

    // Gọi mỗi lần lấy từ pool: làm mới trạng thái
    public void Init(PickupType type, int amount)
    {
        Type = type;
        Amount = amount;
        target = null;
        speed = 0f;
        col.enabled = true;
        ApplyScale();
    }

    // Gộp thêm giá trị vào vật phẩm này (khi sân đã đầy)
    public void AddAmount(int extra)
    {
        Amount += extra;
        ApplyScale();
    }

    private void ApplyScale()
    {
        float k = Mathf.Min(1f + scalePerExtra * (Amount - 1), maxMergeScale);
        transform.localScale = baseScale * k;
    }

    // PickupCollector gọi khi vật phẩm lọt vào bán kính hút
    public void StartAttract(Transform player)
    {
        if (target != null) return;
        target = player;
        speed = startSpeed;
        col.enabled = false;                                        // Tắt collider để không bị quét lại
    }

    private void Update()
    {
        if (target == null) return;                                 // Chưa bị hút thì không làm gì (rất nhẹ)

        speed += acceleration * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if ((transform.position - target.position).sqrMagnitude <= collectDistance * collectDistance)
            Collect();
    }

    private void Collect()
    {
        Collected?.Invoke(Type, Amount);
        target = null;
        PoolManager.Instance.Release(gameObject);
    }
}