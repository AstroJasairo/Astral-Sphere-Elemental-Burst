#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AudioTools
{
    // Chọn object cha (ví dụ Canvas, hoặc một prefab đang mở) rồi chạy menu này
    [MenuItem("Tools/Astral Sphere/Audio/Add Click Sound To Selected Buttons")]
    private static void AddClickSounds()
    {
        int added = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            foreach (Button button in go.GetComponentsInChildren<Button>(true))
            {
                if (button.GetComponent<ButtonClickSound>() != null) continue;
                Undo.AddComponent<ButtonClickSound>(button.gameObject);
                added++;
            }
        }
        Debug.Log($"Đã thêm ButtonClickSound cho {added} nút.");
    }
}
#endif