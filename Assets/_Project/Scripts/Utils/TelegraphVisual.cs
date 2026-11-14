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
    private readonly Color borderColor;

    // Lưu lại để tạo viền thứ hai khi cần (nhẫn đầy đủ 360 độ có HAI vòng)
    private readonly Transform parent;
    private readonly Material material;
    private readonly int sortingOrder;
    private readonly float borderWidth;
    private GameObject line2Object;
    private LineRenderer line2;
    private bool line2Wanted;                 // Hình hiện tại có cần viền thứ hai không

    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;
    private int builtPointCount = -1;         // Khóa của kiểu "đa giác quạt" (SetPolygon)
    private int builtAnnulusKey = -1;         // Khóa của kiểu "dải nhẫn" (SetAnnulus)

    public TelegraphVisual(Transform parent, string name, Material material, int sortingOrder,
                           Color fill, Color border, float borderWidth)
    {
        this.parent = parent;
        this.material = material;
        this.sortingOrder = sortingOrder;
        this.borderWidth = borderWidth;
        fillColor = fill;
        borderColor = border;

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
        line = CreateLine(name + "_Border");
        lineObject = line.gameObject;
    }

    private LineRenderer CreateLine(string lineName)
    {
        GameObject go = new GameObject(lineName);
        go.transform.SetParent(parent, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = material;
        lr.useWorldSpace = false;                          // Điểm tính theo tọa độ cục bộ, xoay theo object cha
        lr.loop = true;                                    // Tự nối điểm cuối về điểm đầu
        lr.alignment = LineAlignment.TransformZ;           // Đường phẳng trên mặt phẳng XY
        lr.numCornerVertices = 2;
        lr.widthMultiplier = borderWidth;
        lr.startColor = borderColor;
        lr.endColor = borderColor;
        lr.sortingOrder = sortingOrder + 1;
        lr.shadowCastingMode = ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
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

        line2Wanted = false;                               // Đa giác thường chỉ có một viền
        if (line2Object != null && line2Object.activeSelf) line2Object.SetActive(false);
    }

    // MỚI: vẽ một "dải nhẫn": phần giữa bán kính innerRadius và outerRadius,
    // bắt đầu từ góc startDeg, mở rộng ngược chiều kim đồng hồ một góc spanDeg.
    // spanDeg >= 360 và allowFull = true: nhẫn đầy đủ (hai vòng tròn). Ngược lại là một dải có hai cạnh bên.
    public void SetAnnulus(float innerRadius, float outerRadius, float startDeg, float spanDeg, bool allowFull = true)
    {
        outerRadius = Mathf.Max(0f, outerRadius);
        innerRadius = Mathf.Clamp(innerRadius, 0f, outerRadius);

        bool full = allowFull && spanDeg >= 359.9f;
        int seg = TelegraphGeometry.ArcSegments;
        int cols = full ? seg : seg + 1;                   // Số cặp điểm (ngoài, trong)

        if (cols != builtAnnulusKey) RebuildAnnulus(cols, full);

        float start = startDeg * Mathf.Deg2Rad;
        float step = (full ? 2f * Mathf.PI : spanDeg * Mathf.Deg2Rad) / seg;

        for (int i = 0; i < cols; i++)
        {
            float a = start + i * step;
            Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
            vertices[2 * i] = dir * outerRadius;           // Chẵn: điểm trên cung ngoài
            vertices[2 * i + 1] = dir * innerRadius;       // Lẻ: điểm trên cung trong
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();

        line2Wanted = full && innerRadius > 0.01f;
        if (full)
        {
            // Nhẫn đầy đủ: viền 1 = vòng ngoài, viền 2 = vòng trong
            line.positionCount = cols;
            for (int i = 0; i < cols; i++) line.SetPosition(i, vertices[2 * i]);

            if (line2Wanted)
            {
                EnsureLine2();
                line2.positionCount = cols;
                for (int i = 0; i < cols; i++) line2.SetPosition(i, vertices[2 * i + 1]);
                line2Object.SetActive(fillObject.activeSelf);
            }
        }
        else
        {
            // Dải có hai cạnh bên: đi dọc cung ngoài, rồi quay về dọc cung trong (viền tự khép kín nhờ loop)
            line.positionCount = cols * 2;
            for (int i = 0; i < cols; i++) line.SetPosition(i, vertices[2 * i]);
            for (int i = 0; i < cols; i++) line.SetPosition(cols + i, vertices[2 * (cols - 1 - i) + 1]);
        }

        if (!line2Wanted && line2Object != null) line2Object.SetActive(false);
    }

    // Đổi độ trong suốt toàn bộ hình, a từ 0 (ẩn) đến 1 (đúng màu gốc).
    // PHẢI gọi sau SetPolygon/SetAnnulus (mảng màu được tạo ở đó).
    public void SetAlpha(float a)
    {
        if (colors == null) return;

        Color fill = fillColor;
        fill.a = fillColor.a * a;
        for (int i = 0; i < colors.Length; i++) colors[i] = fill;
        mesh.SetColors(colors);

        Color border = borderColor;
        border.a = borderColor.a * a;
        line.startColor = border;
        line.endColor = border;
        if (line2 != null)
        {
            line2.startColor = border;
            line2.endColor = border;
        }
    }

    public void Show(bool visible)
    {
        if (fillObject.activeSelf != visible) fillObject.SetActive(visible);
        if (lineObject.activeSelf != visible) lineObject.SetActive(visible);

        if (line2Object != null)
        {
            bool show = visible && line2Wanted;
            if (line2Object.activeSelf != show) line2Object.SetActive(show);
        }
    }

    public void Destroy()
    {
        Object.Destroy(mesh);                              // Mesh tạo bằng code phải tự hủy
    }

    private void EnsureLine2()
    {
        if (line2 != null) return;
        line2 = CreateLine(lineObject.name + "_2");
        line2Object = line2.gameObject;
    }

    // Tạo mảng đỉnh, tam giác, màu cho "count" điểm: tam giác nối pivot với từng cặp điểm liền kề
    private void Rebuild(int count)
    {
        builtPointCount = count;
        builtAnnulusKey = -1;                              // Lần sau dùng SetAnnulus thì phải dựng lại

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

    // Dựng lưới cho dải nhẫn: mỗi ô vuông (ngoài i, ngoài j, trong j, trong i) chia thành 2 tam giác
    private void RebuildAnnulus(int cols, bool full)
    {
        builtAnnulusKey = cols;
        builtPointCount = -1;                              // Lần sau dùng SetPolygon thì phải dựng lại

        int quads = full ? cols : cols - 1;                // Nhẫn đầy đủ có thêm ô nối điểm cuối về điểm đầu

        vertices = new Vector3[cols * 2];
        colors = new Color[cols * 2];
        triangles = new int[quads * 6];

        for (int i = 0; i < colors.Length; i++) colors[i] = fillColor;

        for (int i = 0; i < quads; i++)
        {
            int j = (i + 1) % cols;
            int oi = 2 * i, ii = 2 * i + 1, oj = 2 * j, ij = 2 * j + 1;   // o = ngoài, i = trong

            triangles[i * 6] = oi;
            triangles[i * 6 + 1] = oj;
            triangles[i * 6 + 2] = ii;

            triangles[i * 6 + 3] = oj;
            triangles[i * 6 + 4] = ij;
            triangles[i * 6 + 5] = ii;
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
    }
}