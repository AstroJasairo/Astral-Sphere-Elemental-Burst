using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Màn hình chọn nhân vật trước khi vào trận (hình 4 trong bản vẽ)
public class CharacterSelectScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button rankingButton;           // Nút vàng góc phải trên
    [SerializeField] private LeaderboardPopup leaderboard;

    private void Start()
    {
        playButton.onClick.AddListener(Play);
        rankingButton.onClick.AddListener(() => leaderboard.Show(MetaProgress.GetSelectedMap()));
    }

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        MapData map = MetaProgress.GetSelectedMap();
        mapNameText.text = map != null ? map.displayName : "";
    }

    // Nhân vật đã được CharacterCarouselView lưu lại mỗi lần đổi, nên chỉ cần nạp scene của map
    private void Play()
    {
        MapData map = MetaProgress.GetSelectedMap();
        if (map == null || !map.isAvailable) return;
        SceneNavigator.LoadScene(map.sceneName);
    }
}