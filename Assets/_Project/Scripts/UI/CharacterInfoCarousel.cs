using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum InfoTabKind { Stats, Passive, Active }

// Một tab: loại + số thứ tự (index chỉ có nghĩa với Passive: passive thứ mấy của nhân vật)
public readonly struct InfoTab
{
    public static readonly InfoTab Stats = new InfoTab(InfoTabKind.Stats, 0);

    public readonly InfoTabKind kind;
    public readonly int index;

    public InfoTab(InfoTabKind kind, int index) { this.kind = kind; this.index = index; }
    public bool SameAs(InfoTab other) => kind == other.kind && index == other.index;
}

// Carousel tab thay cho StatGrid, vòng lặp vô hạn: Stats -> Passive 1..n -> Active -> Stats ...
// Mỗi passive một tab. Nhân vật không có passive/active thì giữ MỘT tab "Chưa có kỹ năng" cho loại đó.
public class CharacterInfoCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CharacterCarouselView characterCarousel;   // để biết nhân vật và khi nào đổi nhân vật
    [SerializeField] private GameObject[] pages = new GameObject[3];    // THỨ TỰ = InfoTabKind: StatGrid, PassivePage, ActivePage
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform dotsParent;
    [SerializeField] private Image dotPrefab;                            // dùng lại prefab chấm của carousel nhân vật
    [SerializeField] private Color dotOn = Color.white;
    [SerializeField] private Color dotOff = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private float swipeDistance = 80f;

    [Header("Tùy chọn")]
    [SerializeField] private bool resetOnOpen = true;                    // Mở màn Character luôn về tab Stats
    [SerializeField] private bool rememberOnCharacterChange = true;      // Đổi nhân vật: giữ loại/số thứ tự tab (tắt = về Stats)

    public InfoTab Current => (position >= 0 && position < tabs.Count) ? tabs[position] : InfoTab.Stats;
    public int Count => tabs.Count;
    public event Action<InfoTab> OnTabChanged;                           // Trang skill và panel nâng cấp lắng nghe

    private readonly List<InfoTab> tabs = new List<InfoTab>();
    private readonly List<Image> dots = new List<Image>();
    private int position;
    private string lastCharacterId = "";
    private Vector2 dragStart;

    private void Start()
    {
        previousButton.onClick.AddListener(Previous);
        nextButton.onClick.AddListener(Next);
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += HandleMetaChanged;
        lastCharacterId = CurrentCharacterId();
        RebuildTabs(!resetOnOpen);
        ApplyVisuals();
        Notify();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= HandleMetaChanged;
    }

    public void Next() { Step(1); }
    public void Previous() { Step(-1); }

    private void Step(int delta)
    {
        if (tabs.Count == 0) return;
        position = (position + delta + tabs.Count) % tabs.Count;         // Vòng lặp A-B-C-A-B (phép chia dư)
        ApplyVisuals();
        Notify();
    }

    // Dựng lại danh sách tab theo nhân vật hiện tại. keepCurrent: cố giữ đúng loại/số thứ tự tab đang xem.
    private void RebuildTabs(bool keepCurrent)
    {
        InfoTab previous = Current;                                       // Đọc TRƯỚC khi xóa danh sách
        CharacterData c = characterCarousel != null ? characterCarousel.Current : null;

        tabs.Clear();
        tabs.Add(InfoTab.Stats);
        int passiveTabs = Mathf.Max(1, c != null ? c.PassiveCount : 0);   // Không có passive: vẫn một tab trống
        for (int i = 0; i < passiveTabs; i++) tabs.Add(new InfoTab(InfoTabKind.Passive, i));
        tabs.Add(new InfoTab(InfoTabKind.Active, 0));

        position = keepCurrent ? FindPosition(previous) : 0;
    }

    // Tìm đúng tab (loại, số thứ tự). Passive vượt quá số passive mới thì lấy passive cuối cùng
    private int FindPosition(InfoTab wanted)
    {
        int best = 0;
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].kind != wanted.kind) continue;
            best = i;
            if (tabs[i].index == wanted.index) return i;
        }
        return best;
    }

    // MetaProgress.OnChanged phát cả khi tiêu vàng, nên so id nhân vật để biết có đổi nhân vật thật không
    private void HandleMetaChanged()
    {
        string id = CurrentCharacterId();
        if (id == lastCharacterId) return;
        lastCharacterId = id;

        RebuildTabs(rememberOnCharacterChange);
        ApplyVisuals();
        Notify();                                                         // Số tab đổi nên luôn báo cho các panel
    }

    private string CurrentCharacterId()
    {
        CharacterData c = characterCarousel != null ? characterCarousel.Current : null;
        return c != null ? c.id : "";
    }

    private void ApplyVisuals()
    {
        InfoTabKind kind = Current.kind;
        for (int i = 0; i < pages.Length; i++)
            if (pages[i] != null) pages[i].SetActive(i == (int)kind);     // Mọi tab Passive dùng CHUNG một trang

        if (dotPrefab != null)                                            // Chấm trang theo số tab hiện có
            while (dots.Count < tabs.Count) dots.Add(Instantiate(dotPrefab, dotsParent));
        for (int i = 0; i < dots.Count; i++)
        {
            bool used = i < tabs.Count;
            dots[i].gameObject.SetActive(used);
            if (used) dots[i].color = i == position ? dotOn : dotOff;
        }
    }

    private void Notify() { OnTabChanged?.Invoke(Current); }

    // ----- Vuốt ngang: cần đủ cả 3 interface kéo thì Unity mới gửi sự kiện -----
    public void OnBeginDrag(PointerEventData e) { dragStart = e.position; }
    public void OnDrag(PointerEventData e) { }

    public void OnEndDrag(PointerEventData e)
    {
        float dx = e.position.x - dragStart.x;
        if (Mathf.Abs(dx) < swipeDistance) return;
        if (dx < 0f) Next(); else Previous();
    }
}