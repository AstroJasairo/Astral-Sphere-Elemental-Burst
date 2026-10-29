using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Bộ chọn nhân vật dạng vòng lặp. Đổi nhân vật sẽ LƯU lựa chọn (MetaProgress.SelectCharacter),
// và mọi giao diện khác đọc nhân vật hiện tại qua "Current" khi MetaProgress.OnChanged phát ra.
public class CharacterCarouselView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image portrait;
    [SerializeField] private Image[] starImages;            // 5 ngôi sao
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform dotsParent;
    [SerializeField] private Image dotPrefab;               // Prefab (asset trong Project) cho mỗi chấm
    [SerializeField] private Color starOn = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color starOff = new Color(0.45f, 0.45f, 0.45f);
    [SerializeField] private Color dotOn = Color.white;
    [SerializeField] private Color dotOff = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private float swipeDistance = 80f;     // Vuốt tối thiểu bao nhiêu pixel thì tính là chuyển

    private readonly List<CharacterData> characters = new List<CharacterData>();
    private readonly List<Image> dots = new List<Image>();
    private int index;
    private bool initialized;
    private Vector2 dragStart;

    // Nhân vật đang hiển thị. Lấy lười nên đọc lúc nào cũng an toàn, kể cả trước Start.
    public CharacterData Current
    {
        get
        {
            EnsureInit();
            return characters.Count > 0 ? characters[index] : null;
        }
    }

    private void EnsureInit()
    {
        if (initialized) return;
        initialized = true;

        MetaConfig cfg = MetaProgress.Config;
        if (cfg == null) return;

        foreach (CharacterData c in cfg.characters)
            if (c != null) characters.Add(c);

        // Bắt đầu từ nhân vật đã chọn lần trước
        CharacterData selected = MetaProgress.GetSelectedCharacter();
        index = Mathf.Max(0, selected != null ? characters.IndexOf(selected) : 0);
    }

    private void Start()
    {
        EnsureInit();

        previousButton.onClick.AddListener(Previous);
        nextButton.onClick.AddListener(Next);

        bool multiple = characters.Count > 1;
        previousButton.gameObject.SetActive(multiple);       // Chỉ có 1 nhân vật thì ẩn mũi tên
        nextButton.gameObject.SetActive(multiple);

        if (dotPrefab != null)
            for (int i = 0; i < characters.Count; i++)
                dots.Add(Instantiate(dotPrefab, dotsParent));

        RefreshVisuals();
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += RefreshVisuals;            // Sao đổi, nhân vật đổi... đều làm mới
        RefreshVisuals();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= RefreshVisuals;
    }

    public void Next() { Step(1); }
    public void Previous() { Step(-1); }

    private void Step(int delta)
    {
        EnsureInit();
        if (characters.Count <= 1) return;

        // Vòng lặp A-B-C-A-B: sau nhân vật cuối quay về nhân vật đầu (không chạy ngược lại)
        index = (index + delta + characters.Count) % characters.Count;

        // Lưu lựa chọn. Hàm này phát OnChanged nên mọi giao diện sẽ tự làm mới.
        MetaProgress.SelectCharacter(characters[index].id);
    }

    private void RefreshVisuals()
    {
        CharacterData c = Current;
        if (c == null) return;

        nameText.text = c.characterName;
        if (portrait != null)
        {
            portrait.sprite = c.icon;
            portrait.enabled = c.icon != null;
        }

        int stars = MetaProgress.GetStars(c.id);
        for (int i = 0; i < starImages.Length; i++)
            starImages[i].color = i < stars ? starOn : starOff;

        for (int i = 0; i < dots.Count; i++)
            dots[i].color = i == index ? dotOn : dotOff;
    }

    // ----- Vuốt ngang để chuyển nhân vật -----
    // Phải cài đủ cả 3 interface kéo thì Unity mới gửi sự kiện, dù OnDrag để trống.
    public void OnBeginDrag(PointerEventData e) { dragStart = e.position; }
    public void OnDrag(PointerEventData e) { }

    public void OnEndDrag(PointerEventData e)
    {
        float dx = e.position.x - dragStart.x;
        if (Mathf.Abs(dx) < swipeDistance) return;
        if (dx < 0f) Next(); else Previous();                // Vuốt sang trái = nhân vật kế tiếp
    }
}