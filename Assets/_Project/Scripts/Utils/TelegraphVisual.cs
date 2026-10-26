using UnityEngine;
using UnityEngine.Rendering;

// Một lớp hình của telegraph: phần tô màu (Mesh) + phần viền (LineRenderer).
// Không phải MonoBehaviour: được Telegraph tạo và giữ.
public class TelegraphVisual
{
    private readonly GameObject fillObject;
    private readonly GameObject lineObject;
    private readonly Mesh mesh;
    private readonly LineRenderer line;
    private readonly Color fillColor;

    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;
    private int builtPointCount = -1;

    public TelegraphVisual(Transform parent, string name, Material material, int sortingOrder,
                           Color fill, Color border, float borderWidth)
    {
        fillColor = fill;

        // Phần tô màu
        fillObject = new GameObject(name + "_Fill");
        fillObject.transform.SetParent(parent, false);
        mesh = new Mesh { name = name };
        mesh.MarkDynamic();                                // Báo Unity rằng mesh này sẽ đổi liên tục
        fillObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer mr = fillObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.sortingOrder = sortingOrder;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;

        // Phần viền
        lineObject = new GameObject(name + "_Border");
        lineObject.transform.SetParent(parent, false);
        line = lineObject.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = false;                        // Điểm tính theo tọa độ cục bộ, xoay theo object cha
        line.loop = true;                                  // Tự nối điểm cuối về điểm đầu
        line.alignment = LineAlignment.TransformZ;         // Đường phẳng trên mặt phẳng XY
        line.numCornerVertices = 2;
        line.widthMultiplier = borderWidth;
        line.startColor = border;
        line.endColor = border;
        line.sortingOrder = sortingOrder + 1;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    // Đặt hình dạng từ danh sách điểm. "pivot" là điểm gốc để chia tam giác (tâm quạt, trọng tâm hình chữ nhật).
    public void SetPolygon(Vector2[] points, int count, Vector2 pivot)
    {
        if (count != builtPointCount) Rebuild(count);      // Chỉ dựng lại khi số điểm đổi (hiếm)

        vertices[0] = pivot;
        for (int i = 0; i < count; i++) vertices[i + 1] = points[i];
        mesh.SetVertices(vertices);                        // Chỉ cập nhật vị trí đỉnh, dùng lại mảng cũ
        mesh.RecalculateBounds();   // BẮT BUỘC: tính lại hộp bao, nếu không camera sẽ loại bỏ mesh khi tâm nằm ngoài màn hình

        if (line.positionCount != count) line.positionCount = count;
        for (int i = 0; i < count; i++) line.SetPosition(i, points[i]);
    }

    public void Show(bool visible)
    {
        if (fillObject.activeSelf != visible) fillObject.SetActive(visible);
        if (lineObject.activeSelf != visible) lineObject.SetActive(visible);
    }

    public void Destroy()
    {
        Object.Destroy(mesh);                              // Mesh tạo bằng code phải tự hủy
    }

    // Tạo mảng đỉnh, tam giác, màu cho "count" điểm: tam giác nối pivot với từng cặp điểm liền kề
    private void Rebuild(int count)
    {
        builtPointCount = count;
        vertices = new Vector3[count + 1];
        colors = new Color[count + 1];
        triangles = new int[count * 3];

        for (int i = 0; i < colors.Length; i++) colors[i] = fillColor;
        for (int i = 0; i < count; i++)
        {
            triangles[i * 3] = 0;                          // pivot
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % count + 1;    // Điểm kế tiếp (điểm cuối nối về điểm đầu)
        }

        mesh.Clear();
        mesh.SetVertices(vertices);                        // Phải gán đỉnh trước khi gán tam giác
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
    }
}