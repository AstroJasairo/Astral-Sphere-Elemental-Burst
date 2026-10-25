#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Công cụ Editor: sửa Slider để thanh tiến độ khớp đúng với giá trị (0 = rỗng hoàn toàn, 1 = đầy hoàn toàn)
public static class SliderFillFixer
{
    [MenuItem("Tools/Astral Sphere/Fix Selected Sliders")]
    private static void FixSelected()
    {
        int fixedCount = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            // Quét cả các Slider nằm trong object con (ví dụ chọn cả nhóm HUD)
            foreach (Slider slider in go.GetComponentsInChildren<Slider>(true))
            {
                if (slider.fillRect == null) continue;

                // 1) Fill: bỏ chiều rộng dư 10px
                Undo.RecordObject(slider.fillRect, "Fix slider fill");
                slider.fillRect.offsetMin = Vector2.zero;
                slider.fillRect.offsetMax = Vector2.zero;
                EditorUtility.SetDirty(slider.fillRect);

                // 2) Fill Area (cha của Fill): bỏ lề trái/phải dành cho núm kéo, giữ nguyên lề trên/dưới
                RectTransform area = slider.fillRect.parent as RectTransform;
                if (area != null && area != slider.transform)
                {
                    Undo.RecordObject(area, "Fix slider fill area");
                    area.offsetMin = new Vector2(0f, area.offsetMin.y);
                    area.offsetMax = new Vector2(0f, area.offsetMax.y);
                    EditorUtility.SetDirty(area);
                }
                fixedCount++;
            }
        }

        Debug.Log($"Đã sửa {fixedCount} Slider.");
    }
}
#endif