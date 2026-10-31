#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Công cụ Editor: Tắt Raycast Target cho tất cả object con của object được chọn
public static class DisableRaycastTarget
{
    [MenuItem("Tools/Astral Sphere/Disable Raycast Target")]
    private static void FixSelected()
    {
        int fixedCount = 0;

        // Lấy tất cả các GameObject đang được chọn trong Hierarchy hoặc Project
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Chưa chọn Object nào để tắt Raycast Target!");
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            // Lấy tất cả các thành phần Graphic (Image, Text, TextMeshProUGUI...) ở đối tượng và con của nó
            Graphic[] graphics = obj.GetComponentsInChildren<Graphic>(true);

            foreach (Graphic graphic in graphics)
            {
                if (graphic.raycastTarget)
                {
                    // Đăng ký Undo để có thể Ctrl + Z nếu muốn
                    Undo.RecordObject(graphic, "Disable Raycast Target");

                    graphic.raycastTarget = false;
                    fixedCount++;
                }
            }
        }

        Debug.Log($"Đã tắt {fixedCount} Raycast Target.");
    }
}
#endif