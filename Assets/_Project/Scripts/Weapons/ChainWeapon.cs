using UnityEngine;

// Vũ khí giật chuỗi: Lôi
public class ChainWeapon : WeaponBase
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject boltPrefab;           // Prefab có LineRenderer + LightningBolt

    private readonly Transform[] chain = new Transform[16];   // Bộ đệm dùng lại, tối đa 16 mục tiêu

    protected override bool Fire()
    {
        // 1) Mục tiêu đầu tiên: kẻ địch gần người chơi nhất trong tầm
        Transform first = TargetFinder.FindNearest(transform.position, data.targetRange, enemyMask);
        if (first == null) return false;

        // 2) Nối chuỗi: từ mục tiêu hiện tại tìm con gần nhất chưa bị giật
        int maxTargets = Mathf.Min(CurrentLevel.chainTargets, chain.Length);
        int count = 0;
        chain[count++] = first;

        while (count < maxTargets)
        {
            Transform next = FindNextInChain(chain[count - 1].position, count);
            if (next == null) break;          // Hết kẻ địch trong tầm nhảy: dừng chuỗi
            chain[count++] = next;
        }

        // 3) Vẽ tia sét TRƯỚC khi gây sát thương (quái chết sẽ bị tắt nhưng vị trí đã được ghi lại)
        GameObject boltObj = PoolManager.Instance.Get(boltPrefab, transform.position, Quaternion.identity);
        boltObj.GetComponent<LightningBolt>().Show(transform.position, chain, count);

        // 4) Gây sát thương cho cả chuỗi
        float damage = CalculateDamage();
        for (int i = 0; i < count; i++)
        {
            if (chain[i].TryGetComponent(out HealthSystem hp))
                hp.TakeDamage(damage);
        }
        return true;
    }

    // Tìm kẻ địch gần "from" nhất trong chainRange mà chưa nằm trong chuỗi
    private Transform FindNextInChain(Vector3 from, int chainCount)
    {
        int found = TargetFinder.FindAll(from, data.chainRange, enemyMask);
        Transform best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < found; i++)
        {
            Transform t = TargetFinder.Results[i].transform;
            if (IsInChain(t, chainCount)) continue;   // Đã bị giật rồi

            float sqr = (t.position - from).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = t;
            }
        }
        return best;
    }

    private bool IsInChain(Transform t, int chainCount)
    {
        for (int i = 0; i < chainCount; i++)
            if (chain[i] == t) return true;
        return false;
    }
}