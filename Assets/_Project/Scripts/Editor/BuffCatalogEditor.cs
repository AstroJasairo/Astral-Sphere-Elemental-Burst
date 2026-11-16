#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Giao diện BuffCatalog: nhóm theo loại (Buff, Debuff, BOSS), có tìm kiếm, mỗi mục thu gọn được.
// KHÔNG đổi dữ liệu lưu: nhóm được tính từ BuffType.
[CustomEditor(typeof(BuffCatalog))]
public class BuffCatalogEditor : Editor
{
    private static readonly string[] GroupNames =
    {
        "Buff (viền xanh lá)", "Debuff (viền đỏ)", "BOSS", "KHÔNG HỢP LỆ (loại đã bị xóa khỏi enum BuffType)"
    };

    private string search = "";
    private readonly bool[] groupClosed = new bool[4];
    private readonly HashSet<int> openEntries = new HashSet<int>();    // Khóa = giá trị BuffType

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawColors();
        EditorGUILayout.Space();
        DrawToolbar();
        EditorGUILayout.Space();
        DrawGroups();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawColors()
    {
        EditorGUILayout.LabelField("Màu viền icon", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buffBorderColor"), new GUIContent("Buff (xanh lá)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debuffBorderColor"), new GUIContent("Debuff (đỏ)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillEffectBorderColor"), new GUIContent("Hiệu lực skill (xanh dương)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillCooldownBorderColor"), new GUIContent("Hồi chiêu passive (tím)"));
    }

    private void DrawToolbar()
    {
        search = EditorGUILayout.TextField("Tìm kiếm", search);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Thêm mục còn thiếu"))
        {
            serializedObject.ApplyModifiedProperties();            // Lưu thay đổi đang dở trước khi sửa trực tiếp
            Undo.RecordObject(target, "Fill Missing Buff Entries");
            ((BuffCatalog)target).FillMissing();
            serializedObject.Update();
        }

        if (GUILayout.Button("Xóa mục không hợp lệ"))
        {
            SerializedProperty entries = serializedObject.FindProperty("entries");
            for (int i = entries.arraySize - 1; i >= 0; i--)
            {
                int typeInt = entries.GetArrayElementAtIndex(i).FindPropertyRelative("type").intValue;
                if (!Enum.IsDefined(typeof(BuffType), typeInt)) entries.DeleteArrayElementAtIndex(i);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGroups()
    {
        SerializedProperty entries = serializedObject.FindProperty("entries");

        // Chia các mục vào 4 nhóm (chỉ lưu chỉ số trong mảng)
        var groups = new List<int>[4];
        for (int g = 0; g < groups.Length; g++) groups[g] = new List<int>();

        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            int typeInt = e.FindPropertyRelative("type").intValue;
            bool valid = Enum.IsDefined(typeof(BuffType), typeInt);

            if (!MatchesSearch(e, typeInt, valid)) continue;

            int group = valid ? (int)BuffCatalog.CategoryOf((BuffType)typeInt) : 3;
            groups[group].Add(i);
        }

        for (int g = 0; g < groups.Length; g++)
        {
            if (groups[g].Count == 0) continue;

            groupClosed[g] = !EditorGUILayout.Foldout(!groupClosed[g], $"{GroupNames[g]} ({groups[g].Count})", true, EditorStyles.foldoutHeader);
            if (groupClosed[g]) continue;

            EditorGUI.indentLevel++;
            foreach (int index in groups[g]) DrawEntry(entries, index, g == 3);
            EditorGUI.indentLevel--;
        }
    }

    private bool MatchesSearch(SerializedProperty e, int typeInt, bool valid)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;

        string name = valid ? ((BuffType)typeInt).ToString() : typeInt.ToString();
        string display = e.FindPropertyRelative("displayName").stringValue;
        string label = e.FindPropertyRelative("shortLabel").stringValue;

        return Contains(name, search) || Contains(display, search) || Contains(label, search);
    }

    private static bool Contains(string text, string part)
    {
        return !string.IsNullOrEmpty(text) && text.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void DrawEntry(SerializedProperty entries, int index, bool invalid)
    {
        SerializedProperty e = entries.GetArrayElementAtIndex(index);
        int typeInt = e.FindPropertyRelative("type").intValue;

        string typeName = invalid ? "?" + typeInt : ((BuffType)typeInt).ToString();
        string display = e.FindPropertyRelative("displayName").stringValue;
        string label = e.FindPropertyRelative("shortLabel").stringValue;
        bool hasIcon = e.FindPropertyRelative("icon").objectReferenceValue != null;

        // ● đã có icon, ○ chưa có
        string title = $"{(hasIcon ? "●" : "○")}  {typeName}  -  {display}  [{label}]";

        bool wasOpen = openEntries.Contains(typeInt);
        bool open = EditorGUILayout.Foldout(wasOpen, title, true);
        if (open != wasOpen)
        {
            if (open) openEntries.Add(typeInt);
            else openEntries.Remove(typeInt);
        }
        if (!open) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(e.FindPropertyRelative("type"));
        EditorGUILayout.PropertyField(e.FindPropertyRelative("displayName"));
        EditorGUILayout.PropertyField(e.FindPropertyRelative("shortLabel"));
        EditorGUILayout.PropertyField(e.FindPropertyRelative("icon"));
        EditorGUILayout.PropertyField(e.FindPropertyRelative("isDebuff"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(2);
    }
}
#endif