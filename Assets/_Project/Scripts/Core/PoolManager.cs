using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // Mỗi prefab có một hàng đợi các instance đang rảnh
    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
    // Nhớ mỗi instance thuộc prefab nào, để biết trả về hàng đợi nào
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();

    private void Awake()
    {
        Instance = this;
    }

    // Lấy một đối tượng từ pool (hết thì mới tạo mới)
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject obj = null;
        // Lấy ra cho tới khi gặp object còn tồn tại (phòng trường hợp bị Destroy ngoài ý muốn)
        while (queue.Count > 0 && obj == null)
            obj = queue.Dequeue();

        if (obj == null)
        {
            obj = Instantiate(prefab, transform);   // Gom gọn dưới PoolManager trong Hierarchy
            instanceToPrefab[obj] = prefab;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    // Trả đối tượng về pool (thay cho Destroy)
    public void Release(GameObject obj)
    {
        // Chống trả về hai lần: nếu đã tắt thì bỏ qua, tránh cùng một object nằm 2 lần trong hàng đợi
        if (!obj.activeSelf) return;

        if (!instanceToPrefab.TryGetValue(obj, out var prefab))
        {
            Destroy(obj);   // Không do pool tạo ra thì hủy bình thường
            return;
        }

        obj.SetActive(false);
        pools[prefab].Enqueue(obj);
    }
}