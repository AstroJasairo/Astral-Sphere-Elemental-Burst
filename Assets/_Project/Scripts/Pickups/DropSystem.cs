using System.Collections.Generic;
using UnityEngine;

public class DropSystem : MonoBehaviour
{
    [Header("Rải vật phẩm lúc bắt đầu trận (tài liệu: 00:00 có ít vàng và pha lê trên map)")]
    [SerializeField] private PickupData goldPickup;
    [SerializeField] private PickupData crystalPickup;
    [SerializeField] private int startGoldCount = 8;
    [SerializeField] private int startCrystalCount = 4;
    [SerializeField] private float scatterRadius = 8f;

    private void OnEnable() { EnemyBase.EnemyKilled += OnEnemyKilled; }
    private void OnDisable() { EnemyBase.EnemyKilled -= OnEnemyKilled; }

    private void Start()
    {
        Vector2 center = GameManager.Instance.Player.position;
        for (int i = 0; i < startGoldCount; i++)
            SpawnPickup(goldPickup, 1, center + Random.insideUnitCircle * scatterRadius);
        for (int i = 0; i < startCrystalCount; i++)
            SpawnPickup(crystalPickup, 1, center + Random.insideUnitCircle * scatterRadius);
    }

    private void OnEnemyKilled(EnemyBase enemy)
    {
        EnemyData data = enemy.Data;
        Vector2 pos = enemy.transform.position;

        // 1) Món luôn rơi (vàng)
        foreach (DropEntry entry in data.guaranteedDrops)
            SpawnPickup(entry.pickup, entry.amount, pos + Random.insideUnitCircle * 0.3f);

        // 2) Có thể rơi thêm 1 món ngẫu nhiên theo trọng số
        List<DropEntry> shared = data.sharedDrops != null ? data.sharedDrops.entries : null;
        bool hasPool = data.randomDrops.Count > 0 || (shared != null && shared.Count > 0);

        if (hasPool && Random.value <= data.randomDropChance)
        {
            DropEntry picked = PickWeighted(data.randomDrops, shared);   // Chọn trong bảng riêng + bảng chung
            if (picked != null)
                SpawnPickup(picked.pickup, picked.amount, pos + Random.insideUnitCircle * 0.3f);
        }
    }

    // Random theo trọng số: trọng số càng lớn càng dễ trúng
    private DropEntry PickWeighted(List<DropEntry> own, List<DropEntry> shared)
    {
        float total = SumWeights(own) + SumWeights(shared);
        if (total <= 0f) return null;

        float roll = Random.value * total;     // Điểm ngẫu nhiên trên "thanh" tổng trọng số của CẢ HAI bảng
        DropEntry picked = FindByRoll(own, ref roll);
        if (picked == null) picked = FindByRoll(shared, ref roll);   // Phần còn lại của roll tiếp tục sang bảng chung
        return picked;
    }

    private float SumWeights(List<DropEntry> list)
    {
        if (list == null) return 0f;
        float sum = 0f;
        foreach (DropEntry e in list) sum += e.weight;
        return sum;
    }

    private DropEntry FindByRoll(List<DropEntry> list, ref float roll)
    {
        if (list == null) return null;
        foreach (DropEntry e in list)
        {
            roll -= e.weight;
            if (roll <= 0f) return e;
        }
        return null;
    }

    private void SpawnPickup(PickupData data, int amount, Vector2 position)
    {
        // Cấu hình thiếu thì cảnh báo rõ ràng và bỏ qua, không làm hỏng các hệ thống khác
        // Fix: Thông báo thiếu component
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning("DropSystem: một DropEntry thiếu PickupData hoặc Prefab. Kiểm tra lại EnemyData.", this);
            return;
        }
        GameObject go = PoolManager.Instance.Get(data.prefab, position, data.prefab.transform.rotation);
        go.GetComponent<Pickup>().Init(data.type, amount);
    }
}