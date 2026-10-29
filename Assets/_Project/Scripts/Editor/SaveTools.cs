#if UNITY_EDITOR
using UnityEditor;

// Menu tiện ích cho file lưu
public static class SaveTools
{
    [MenuItem("Tools/Astral Sphere/Save/Open Save Folder")]
    private static void OpenFolder()
    {
        SaveSystem.Load();                          // Đảm bảo file tồn tại trước khi mở
        SaveSystem.Save();
        EditorUtility.RevealInFinder(SaveSystem.FilePath);
    }

    [MenuItem("Tools/Astral Sphere/Save/Delete Save")]
    private static void DeleteSave()
    {
        if (EditorUtility.DisplayDialog("Xóa save", "Xóa toàn bộ tiến trình đã lưu?", "Xóa", "Hủy"))
            SaveSystem.ResetAll();
    }

    [MenuItem("Tools/Astral Sphere/Save/Add 10000 Gold")]
    private static void AddGold()
    {
        MetaProgress.DebugAddGold(10000);
    }
}
#endif