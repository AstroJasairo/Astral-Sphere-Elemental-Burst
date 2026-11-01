using System;
using UnityEngine;

// Nạp trước các đối tượng hay dùng vào pool ngay khi vào trận
public class PoolPrewarmer : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public GameObject prefab;
        public int count = 20;
    }

    [SerializeField] private Entry[] entries;

    // Dùng Start (không phải Awake) để chắc chắn PoolManager.Instance đã sẵn sàng
    private void Start()
    {
        foreach (Entry e in entries)
            if (e.prefab != null) PoolManager.Instance.Prewarm(e.prefab, e.count);
    }
}