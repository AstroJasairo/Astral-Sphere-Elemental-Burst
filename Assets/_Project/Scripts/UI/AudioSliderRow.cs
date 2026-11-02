using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Một thanh trượt âm lượng trong màn hình Settings
[RequireComponent(typeof(Slider))]
public class AudioSliderRow : MonoBehaviour, IPointerUpHandler
{
    public enum Channel { Master, Music, Sfx, Ui }

    [SerializeField] private Channel channel;
    [SerializeField] private TMP_Text valueText;          // Hiện phần trăm (tùy chọn)

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    // Mỗi lần mở màn hình Settings: đọc lại giá trị đã lưu
    private void OnEnable()
    {
        if (slider == null) return;
        slider.SetValueWithoutNotify(Get());              // Đặt giá trị mà không kích hoạt sự kiện đổi
        UpdateText(slider.value);
    }

    private void OnValueChanged(float value)
    {
        Set(value);
        UpdateText(value);
    }

    // Thả tay: lưu file và phát tiếng bíp thử để nghe mức âm lượng mới
    public void OnPointerUp(PointerEventData eventData)
    {
        SaveSystem.Save();
        AudioManager.PlayPreview(PreviewCategory());
    }

    private float Get()
    {
        switch (channel)
        {
            case Channel.Master: return AudioManager.MasterVolume;
            case Channel.Music: return AudioManager.GetVolume(AudioCategory.Music);
            case Channel.Sfx: return AudioManager.GetVolume(AudioCategory.Sfx);
            default: return AudioManager.GetVolume(AudioCategory.Ui);
        }
    }

    private void Set(float value)
    {
        switch (channel)
        {
            case Channel.Master: AudioManager.MasterVolume = value; break;
            case Channel.Music: AudioManager.SetVolume(AudioCategory.Music, value); break;
            case Channel.Sfx: AudioManager.SetVolume(AudioCategory.Sfx, value); break;
            default: AudioManager.SetVolume(AudioCategory.Ui, value); break;
        }
    }

    // Thanh tổng thử bằng loại hiệu ứng
    private AudioCategory PreviewCategory()
    {
        switch (channel)
        {
            case Channel.Music: return AudioCategory.Music;
            case Channel.Ui: return AudioCategory.Ui;
            default: return AudioCategory.Sfx;
        }
    }

    private void UpdateText(float value)
    {
        if (valueText != null) valueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}