using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum InfoTab { Stats, Passive, Active }

// Carousel 3 tab thay cho StatGrid: Stats -> Passive -> Active -> Stats ... (vòng lặp vô hạn)
// Cách điều khiển giống CharacterCarouselView: mũi tên, vuốt ngang, chấm trang.
public class CharacterInfoCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CharacterCarouselView characterCarousel;   // để biết khi nào đổi nhân vật
    [SerializeField] private GameObject[] pages = new GameObject[3];    // THỨ TỰ = InfoTab: Stats, Passive, Active
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform dotsParent;
    [SerializeField] private Image dotPrefab;                            // dùng lại prefab chấm của carousel nhân vật
    [SerializeField] private Color dotOn = Color.white;
    [SerializeField] private Color dotOff = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private float swipeDistance = 80f;

    [Header("Tùy chọn")]
    [SerializeField] private bool resetOnOpen = true;                    // Mở màn Character luôn về tab Stats
    [SerializeField] private bool rememberOnCharacterChange = true;      // Đổi nhân vật: giữ tab hiện tại (tắt = về Stats)

    public InfoTab Current { get; private set; } = InfoTab.Stats;
    public event Action<InfoTab> OnTabChanged;                           // Panel nâng cấp lắng nghe

    private readonly List<Image> dots = new List<Image>();
    private string lastCharacterId = "";
    private Vector2 dragStart;

    private void Start()
    {
        previousButton.onClick.AddListener(Previous);
        nextButton.onClick.AddListener(Next);

        if (dotPrefab != null)
            for (int i = 0; i < pages.Length; i++)
                dots.Add(Instantiate(dotPrefab, dotsParent));

        ApplyVisuals();
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += HandleMetaChanged;
        lastCharacterId = CurrentCharacterId();
        if (resetOnOpen) SetTab(InfoTab.Stats);
        else ApplyVisuals();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= HandleMetaChanged;
    }

    public void Next() { Step(1); }
    public void Previous() { Step(-1); }

    private void Step(int delta)
    {
        int count = pages.Length;
        // Vòng lặp A-B-C-A-B (phép chia dư), giống carousel nhân vật
        SetTab((InfoTab)(((int)Current + delta + count) % count));
    }

    public void SetTab(InfoTab tab)
    {
        bool changed = tab != Current;
        Current = tab;
        ApplyVisuals();
        if (changed) OnTabChanged?.Invoke(tab);
    }

    private void ApplyVisuals()
    {
        for (int i = 0; i < pages.Length; i++)
            if (pages[i] != null) pages[i].SetActive(i == (int)Current);
        for (int i = 0; i < dots.Count; i++)
            dots[i].color = i == (int)Current ? dotOn : dotOff;
    }

    // MetaProgress.OnChanged phát cả khi tiêu vàng, nên so id nhân vật để biết có đổi nhân vật thật không
    private void HandleMetaChanged()
    {
        string id = CurrentCharacterId();
        if (id == lastCharacterId) return;
        lastCharacterId = id;
        if (!rememberOnCharacterChange) SetTab(InfoTab.Stats);
    }

    private string CurrentCharacterId()
    {
        CharacterData c = characterCarousel != null ? characterCarousel.Current : null;
        return c != null ? c.id : "";
    }

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