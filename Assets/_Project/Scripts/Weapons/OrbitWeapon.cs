using System.Collections.Generic;
using UnityEngine;

public class OrbitWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject orbPrefab;           // Hình quả cầu (chỉ cần hình ảnh, không cần collider)
    [SerializeField] private float orbHitRadius = 0.45f;     // Bán kính va chạm của mỗi quả cầu

    private const float ScanInterval = 0.05f;                // Quét va chạm 20 lần mỗi giây, không phụ thuộc hồi chiêu

    private readonly List<Transform> orbs = new List<Transform>();
    private readonly Dictionary<int, float> lastHitTime = new Dictionary<int, float>();   // Kẻ địch -> lần cuối bị đánh
    private readonly List<int> staleKeys = new List<int>();
    private float angle;
    private float scanTimer;

    // Khi lên cấp: bổ sung thêm quả cầu cho đủ số lượng của cấp mới
    protected override void OnLevelChanged()
    {
        int count = CurrentLevel.projectileCount;

        while (orbs.Count < count)
        {
            GameObject go = Instantiate(orbPrefab, transform);       // Là con của vũ khí nên đi theo người chơi
            orbs.Add(go.transform);
        }
        for (int i = 0; i < orbs.Count; i++)
            orbs[i].gameObject.SetActive(i < count);
    }

    // Vũ khí này không dùng hồi chiêu của WeaponBase (nên KHÔNG gọi base.Update): tự xoay và tự quét
    protected override void Update()
    {
        if (data == null) return;

        Rotate();

        scanTimer += Time.deltaTime;
        if (scanTimer < ScanInterval) return;
        scanTimer = 0f;
        Scan();
    }

    // Xoay đều các quả cầu quanh người chơi. All Out Assault làm xoay nhanh hơn.
    private void Rotate()
    {
        int count = CurrentLevel.projectileCount;
        float speedMultiplier = owner != null ? owner.OrbitSpeedMultiplier : 1f;

        angle = Mathf.Repeat(angle + data.orbitSpeed * speedMultiplier * Time.deltaTime, 360f);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float a = (angle + i * step) * Mathf.Deg2Rad;
            orbs[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * data.orbitRadius;
        }
    }

    // Một lần quét va chạm quanh các quả cầu. Mỗi kẻ địch chỉ bị đánh tối đa một lần mỗi hitInterval.
    private void Scan()
    {
        int count = CurrentLevel.projectileCount;
        float damage = CalculateDamage();
        float intervalMultiplier = owner != null ? owner.OrbitHitIntervalMultiplier : 1f;
        float hitInterval = data.hitInterval * intervalMultiplier;      // All Out Assault: đánh dày hơn
        float now = Time.time;

        for (int i = 0; i < count; i++)
        {
            int found = TargetFinder.FindAll(orbs[i].position, orbHitRadius, enemyMask);
            for (int j = 0; j < found; j++)
            {
                Collider2D col = TargetFinder.Results[j];
                int id = col.GetInstanceID();

                if (lastHitTime.TryGetValue(id, out float last) && now - last < hitInterval) continue;
                lastHitTime[id] = now;

                if (col.TryGetComponent(out HealthSystem hp)) hp.TakeDamage(damage);
            }
        }

        CleanUpHitMemory(now);
    }

    // Abstract của WeaponBase, nhưng vũ khí này không dùng (không gọi base.Update nên Fire không bao giờ được gọi)
    protected override bool Fire()
    {
        return false;
    }

    // Dọn các bản ghi cũ để từ điển không phình mãi
    private void CleanUpHitMemory(float now)
    {
        if (lastHitTime.Count <= 128) return;

        staleKeys.Clear();
        foreach (KeyValuePair<int, float> entry in lastHitTime)
            if (now - entry.Value > 2f) staleKeys.Add(entry.Key);
        foreach (int key in staleKeys) lastHitTime.Remove(key);
    }
}