using System;
using UnityEngine;

// Gắn vào prefab vật phẩm (vàng, pha lê...). Nằm yên cho đến khi người chơi vào tầm hút.
[RequireComponent(typeof(Collider2D))]
public class Pickup : MonoBehaviour
{
    // Sự kiện toàn cục "có vật phẩm được nhặt": (loại, số lượng)
    public static event Action<PickupType, int> Collected;

    [SerializeField] private float startSpeed = 3f;        // Tốc độ lúc bắt đầu bị hút
    [SerializeField] private float acceleration = 25f;     // Càng bay càng nhanh để không bị người chơi chạy thoát
    [SerializeField] private float collectDistance = 0.3f; // Cách người chơi bao xa thì tính là đã nhặt

    public PickupType Type { get; private set; }
    public int Amount { get; private set; }

    private Collider2D col;
    private Transform target;
    private float speed;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    // Gọi mỗi lần lấy từ pool: làm mới trạng thái
    public void Init(PickupType type, int amount)
    {
        Type = type;
        Amount = amount;
        target = null;
        speed = 0f;
        col.enabled = true;
    }

    // PickupCollector gọi khi vật phẩm lọt vào bán kính hút
    public void StartAttract(Transform player)
    {
        if (target != null) return;   // Đã đang bay rồi
        target = player;
        speed = startSpeed;
        col.enabled = false;          // Tắt collider để không bị quét lại, và đỡ tốn vật lý khi di chuyển
    }

    private void Update()
    {
        if (target == null) return;   // Chưa bị hút thì không làm gì (rất nhẹ)

        speed += acceleration * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // So sánh bình phương khoảng cách để tránh phép căn bậc hai
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