using System;
using System.Collections.Generic;
using UnityEngine;

// Giao diện của một loại buff/debuff
[Serializable]
public class BuffVisual
{
    public BuffType type;
    public string displayName;
    public string shortLabel;      // Chữ hiện thay cho icon khi chưa có hình (ví dụ "SPD")
    public Sprite icon;            // Hình bên trong khung (thêm sau, mỗi buff/debuff một hình)
    public bool isDebuff;          // false: buff (viền xanh lá). true: debuff (viền đỏ)
}

// Danh mục giao diện buff/debuff. Tạo bằng: Assets > Create > Astral Sphere > Buff Catalog
[CreateAssetMenu(menuName = "Astral Sphere/Buff Catalog")]
public class BuffCatalog : ScriptableObject
{
    public Color buffBorderColor = new Color(0.25f, 0.85f, 0.3f);      // Xanh lá
    public Color debuffBorderColor = new Color(0.9f, 0.2f, 0.2f);      // Đỏ
    public List<BuffVisual> entries = new List<BuffVisual>();

    private BuffVisual[] byType;            // Bảng tra nhanh theo enum, dựng lười

    public Color BorderColor(bool isDebuff)
    {
        return isDebuff ? debuffBorderColor : buffBorderColor;
    }

    // Trả về null nếu loại này chưa có trong danh mục
    public BuffVisual Get(BuffType type)
    {
        if (byType == null) Rebuild();
        int i = (int)type;
        return (i >= 0 && i < byType.Length) ? byType[i] : null;
    }

    private void Rebuild()
    {
        byType = new BuffVisual[BuffController.TypeCount];
        foreach (BuffVisual e in entries)
        {
            int i = (int)e.type;
            if (i >= 0 && i < byType.Length) byType[i] = e;
        }
    }

    private void OnValidate()
    {
        byType = null;                      // Sửa trong Inspector thì dựng lại bảng tra
    }

    // Chuột phải vào tiêu đề asset: thêm các dòng còn thiếu cho mọi giá trị của BuffType
    [ContextMenu("Fill Missing Entries")]
    private void FillMissing()
    {
        foreach (BuffType t in Enum.GetValues(typeof(BuffType)))
        {
            if (entries.Exists(e => e.type == t)) continue;
            entries.Add(new BuffVisual
            {
                type = t,
                displayName = t.ToString(),
                shortLabel = DefaultLabel(t),
                isDebuff = BuffController.IsDebuff(t)              // debuff: viền đỏ
            });
        }
        byType = null;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static string DefaultLabel(BuffType t)
    {
        switch (t)
        {
            case BuffType.Speed: return "SPD";
            case BuffType.Rage: return "ATK";
            case BuffType.Magnet: return "MAG";
            case BuffType.ScoreBoost: return "PTS";
            case BuffType.ActiveSkill: return "ACT";
            case BuffType.Slow: return "SLW";
            case BuffType.Root: return "RT";
            case BuffType.Stun: return "STN";
            case BuffType.Frostbite: return "FRZ";
            case BuffType.Silence: return "SIL";
            case BuffType.Vulnerable: return "VUL";
            case BuffType.Exhaust: return "EXH";
            case BuffType.Blind: return "BLD";
            case BuffType.Weakness: return "WKN";
            case BuffType.ManaDrain: return "DRN";
            default:
                string name = t.ToString().ToUpper();
                return name.Length > 3 ? name.Substring(0, 3) : name;
        }
    }
}