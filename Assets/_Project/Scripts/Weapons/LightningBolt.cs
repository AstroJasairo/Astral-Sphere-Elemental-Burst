using UnityEngine;

// Hình tia sét: một LineRenderer đi qua các mục tiêu, tồn tại ngắn rồi tự trả về pool
[RequireComponent(typeof(LineRenderer))]
public class LightningBolt : MonoBehaviour
{
    [SerializeField] private float duration = 0.15f;

    private LineRenderer line;
    private float timer;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Show(Vector3 start, Transform[] targets, int count)
    {
        line.positionCount = count + 1;
        line.SetPosition(0, start);
        // Dùng vòng lặp SetPosition thay vì tạo mảng mới để không sinh rác
        for (int i = 0; i < count; i++)
            line.SetPosition(i + 1, targets[i].position);
        timer = duration;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) PoolManager.Instance.Release(gameObject);
    }
}