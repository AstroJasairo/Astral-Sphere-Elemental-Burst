using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Danh sách map dạng "bánh xe" cuộn dọc, vòng lặp, tự hít về thẻ gần nhất.
// Gắn vào một object có Image trong suốt (bật Raycast Target) để vuốt được cả vùng trống giữa các thẻ.
public class MapWheelCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private MapCardView cardPrefab;
    [SerializeField] private int cardsEachSide = 2;            // 2 nghĩa là 5 thẻ cùng lúc
    [SerializeField] private float gap = 40f;                  // Khoảng hở giữa MÉP hai thẻ liền kề (luôn đều nhau)
    [SerializeField] private float scaleFalloff = 0.2f;        // Mỗi 1 thẻ cách tâm thì nhỏ đi 20%
    [SerializeField] private float minScale = 0.45f;           // Phải nhỏ hơn 1 - scaleFalloff x (cardsEachSide + 0.5)
    [SerializeField] private float alphaFalloff = 0.35f;       // Mỗi 1 thẻ cách tâm thì mờ đi 35%
    [SerializeField] private float smoothTime = 0.12f;         // Thời gian "hít" về thẻ (nhỏ = hít nhanh)
    [SerializeField] private float flingFactor = 0.25f;        // Độ "trôi" khi thả tay đang vuốt nhanh

    public event Action<MapData> MapChosen;                    // Người chơi bấm thẻ giữa
    public event Action<MapData> RankingRequested;             // Người chơi bấm nút xếp hạng của một thẻ

    private readonly List<MapData> maps = new List<MapData>();
    private readonly List<MapCardView> slots = new List<MapCardView>();

    // offsets[k] = khoảng cách từ tâm bánh xe tới tâm của thẻ cách tâm k vị trí (đã tính theo kích thước thẻ)
    private float[] offsets;
    private float pitch;                     // = offsets[1]: thẻ giữa dịch bao nhiêu thì bánh xe quay 1 thẻ

    private float position;                  // Vị trí cuộn liên tục, tính bằng số thẻ. KHÔNG bị bó trong [0, N): vòng lặp nhờ phép chia dư
    private float targetPosition;
    private float smoothVelocity;
    private float lastLayoutPosition = float.NaN;
    private bool dragging;
    private float dragVelocity;              // Số thẻ mỗi giây
    private float canvasScale = 1f;
    private bool initialized;

    private void Start()
    {
        MetaConfig cfg = MetaProgress.Config;
        if (cfg != null)
            foreach (MapData m in cfg.maps)
                if (m != null) maps.Add(m);

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) canvasScale = canvas.rootCanvas.scaleFactor;

        // Tạo các thẻ: độ lệch từ -cardsEachSide đến +cardsEachSide
        for (int offset = -cardsEachSide; offset <= cardsEachSide; offset++)
        {
            MapCardView slot = Instantiate(cardPrefab, transform);
            slot.Offset = offset;
            slot.Clicked += OnCardClicked;
            slot.RankingClicked += OnRankingClicked;
            slots.Add(slot);
        }

        // Thẻ xa tâm vẽ trước, thẻ giữa vẽ sau cùng nên nằm trên cùng
        for (int k = cardsEachSide; k >= 0; k--)
            foreach (MapCardView s in slots)
                if (Mathf.Abs(s.Offset) == k) s.transform.SetAsLastSibling();

        BuildOffsets();

        initialized = true;
        SyncToSelectedMap();
    }

    private void OnEnable()
    {
        if (initialized) SyncToSelectedMap();                  // Quay lại màn Map thì dừng ở map đã chọn
    }

    // Kích thước thẻ theo khoảng cách tới tâm. Dùng ở CẢ HAI nơi (tính vị trí và vẽ) nên luôn khớp nhau.
    private float ScaleAt(float abs)
    {
        return Mathf.Max(minScale, 1f - scaleFalloff * abs);
    }

    // Tính sẵn vị trí tâm của các thẻ ở các mốc nguyên: thẻ sau cách thẻ trước đúng
    // (nửa cao thẻ trước + nửa cao thẻ sau + gap), nên khoảng hở giữa mép luôn bằng "gap".
    private void BuildOffsets()
    {
        float cardHeight = ((RectTransform)slots[0].transform).rect.height;

        offsets = new float[cardsEachSide + 2];     // Dư một mốc để nội suy cho thẻ ở xa nhất
        for (int k = 1; k < offsets.Length; k++)
        {
            float previousHalf = cardHeight * ScaleAt(k - 1) * 0.5f;
            float currentHalf = cardHeight * ScaleAt(k) * 0.5f;
            offsets[k] = offsets[k - 1] + previousHalf + currentHalf + gap;
        }

        pitch = offsets.Length > 1 ? offsets[1] : 1f;
    }

    // Khoảng cách từ tâm bánh xe tới tâm thẻ khi thẻ cách tâm "abs" vị trí (abs có thể lẻ).
    // Nội suy tuyến tính giữa hai mốc nguyên gần nhất.
    private float OffsetAt(float abs)
    {
        int i = Mathf.Min((int)abs, offsets.Length - 2);
        float t = abs - i;
        return Mathf.Lerp(offsets[i], offsets[i + 1], t);
    }

    private void SyncToSelectedMap()
    {
        MapData selected = MetaProgress.GetSelectedMap();
        int index = selected != null ? Mathf.Max(0, maps.IndexOf(selected)) : 0;

        position = index;
        targetPosition = index;
        smoothVelocity = 0f;
        lastLayoutPosition = float.NaN;                        // Buộc vẽ lại
    }

    private void Update()
    {
        if (maps.Count == 0) return;

        if (!dragging)
        {
            // Trượt mượt về thẻ đích. Dùng thời gian không bị ảnh hưởng bởi timeScale.
            position = Mathf.SmoothDamp(position, targetPosition, ref smoothVelocity, smoothTime,
                                        Mathf.Infinity, Time.unscaledDeltaTime);
            if (Mathf.Abs(position - targetPosition) < 0.0005f) position = targetPosition;
        }

        if (Mathf.Approximately(position, lastLayoutPosition)) return;   // Đứng yên thì khỏi tính lại
        lastLayoutPosition = position;
        Layout();
    }

    private void Layout()
    {
        int baseIndex = Mathf.RoundToInt(position);            // Thẻ gần tâm nhất
        float frac = position - baseIndex;                     // Độ lệch nhỏ, từ -0.5 đến +0.5

        foreach (MapCardView slot in slots)
        {
            // Khoảng cách từ thẻ này tới tâm, tính bằng số thẻ (âm = phía trên, dương = phía dưới)
            float rel = slot.Offset - frac;
            float abs = Mathf.Abs(rel);

            // Thẻ này đang đại diện cho map nào (phép chia dư giúp vòng lặp vô hạn)
            int mapIndex = Mod(baseIndex + slot.Offset, maps.Count);
            slot.Bind(maps[mapIndex], mapIndex);

            float alpha = Mathf.Clamp01(1f - alphaFalloff * abs);
            float y = -Mathf.Sign(rel) * OffsetAt(abs);        // Thẻ có chỉ số cao hơn nằm bên dưới (y âm)
            slot.SetPose(new Vector2(0f, y), ScaleAt(abs), alpha);
        }
    }

    // ----- Vuốt -----

    public void OnBeginDrag(PointerEventData e)
    {
        dragging = true;
        dragVelocity = 0f;
        smoothVelocity = 0f;
    }

    public void OnDrag(PointerEventData e)
    {
        if (maps.Count == 0) return;

        // Pixel màn hình -> đơn vị canvas -> số thẻ. Vuốt LÊN (delta.y > 0) làm thẻ bên dưới tiến lên giữa.
        // Chia cho "pitch" để thẻ giữa đi đúng theo ngón tay.
        float deltaCards = (e.delta.y / canvasScale) / pitch;
        position += deltaCards;

        // Ước lượng vận tốc để tạo cảm giác "trôi" khi thả tay
        dragVelocity = Mathf.Lerp(dragVelocity, deltaCards / Mathf.Max(Time.unscaledDeltaTime, 0.0001f), 0.4f);
    }

    public void OnEndDrag(PointerEventData e)
    {
        dragging = false;

        // Đích = thẻ gần nhất sau khi cộng thêm đà trôi (giới hạn tối đa 2 thẻ để không quay quá đà)
        float projected = position + dragVelocity * flingFactor;
        projected = Mathf.Clamp(projected, position - 2f, position + 2f);
        targetPosition = Mathf.Round(projected);
        smoothVelocity = 0f;
    }

    // ----- Bấm thẻ -----

    private void OnCardClicked(MapCardView slot)
    {
        if (dragging) return;

        if (slot.Offset != 0)
        {
            // Bấm thẻ bên cạnh: cuộn thẻ đó vào giữa
            targetPosition = Mathf.Round(position) + slot.Offset;
            return;
        }

        // Bấm thẻ giữa: chọn map (map đang khóa thì không vào được)
        if (slot.Map == null || !slot.Map.isAvailable) return;
        MapChosen?.Invoke(slot.Map);
    }

    private void OnRankingClicked(MapData map)
    {
        if (map != null) RankingRequested?.Invoke(map);
    }

    private static int Mod(int a, int n)
    {
        return ((a % n) + n) % n;
    }
}