using System.Collections.Generic;
using UnityEngine;

// Bảng rơi đồ dùng chung cho nhiều loại quái. Tạo bằng: Assets > Create > Astral Sphere > Drop Table Data
[CreateAssetMenu(menuName = "Astral Sphere/Drop Table Data")]
public class DropTableData : ScriptableObject
{
    public List<DropEntry> entries = new List<DropEntry>();
}