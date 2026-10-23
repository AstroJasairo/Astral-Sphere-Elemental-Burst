using UnityEngine;

// Dữ liệu một loại vật phẩm. Tạo bằng: Assets > Create > Astral Sphere > Pickup Data
[CreateAssetMenu(menuName = "Astral Sphere/Pickup Data")]
public class PickupData : ScriptableObject
{
    public PickupType type;
    public GameObject prefab;
}

// Một dòng trong bảng rơi đồ
[System.Serializable]
public class DropEntry
{
    public PickupData pickup;
    public int amount = 1;      // Số lượng (ví dụ pha lê mỗi loại quái rơi số khác nhau)
    public float weight = 1f;   // Trọng số khi random (chỉ dùng cho bảng rơi ngẫu nhiên)
}