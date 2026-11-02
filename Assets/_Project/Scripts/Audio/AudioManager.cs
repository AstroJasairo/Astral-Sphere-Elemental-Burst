using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Quản lý toàn bộ âm thanh. Tự tạo từ prefab Resources/AudioManager và tồn tại qua mọi scene.
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    // Thư viện âm thanh dùng chung (null nếu chưa có AudioManager)
    public static AudioLibrary Library => instance != null ? instance.library : null;

    [SerializeField] private AudioLibrary library;
    [SerializeField] private int sfxVoices = 16;               // Số hiệu ứng được phát cùng lúc tối đa
    [SerializeField] private float musicFadeTime = 0.8f;       // Thời gian chuyển bài
    [SerializeField] private bool logSilentCues;               // Bật để in ra khi một ô âm thanh được gọi mà chưa có file

    private AudioSource[] sfxSources;
    private int nextSfx;

    private AudioSource musicA, musicB;
    private AudioSource activeMusic, otherMusic;               // Nguồn đang là bài hiện tại và nguồn đang mờ dần
    private AudioClip currentMusicClip;
    private float currentCueVolume, otherCueVolume;
    private float activeFade, otherFade;                       // 0..1
    private float musicDuck = 1f;                              // Hạ nhỏ nhạc (ví dụ khi tạm dừng)

    private readonly Dictionary<AudioCue, float> lastPlay = new Dictionary<AudioCue, float>();
    private readonly HashSet<AudioCue> reportedSilent = new HashSet<AudioCue>();
    private readonly AudioClip[] previewTones = new AudioClip[3];   // Tiếng bíp thử cho từng loại, tạo bằng code

    // Chạy trước khi scene đầu tiên nạp: tạo AudioManager nếu chưa có
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        AudioManager prefab = Resources.Load<AudioManager>("AudioManager");
        if (prefab == null)
        {
            Debug.LogWarning("AudioManager: không tìm thấy prefab Resources/AudioManager, game sẽ không có âm thanh.");
            return;
        }
        Instantiate(prefab);          // Awake của bản mới sẽ tự gán instance
    }

    // Khi tắt "Domain Reload" trong Editor, biến static còn giữ giá trị cũ
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        musicA = CreateSource("Music A");
        musicB = CreateSource("Music B");
        activeMusic = musicA;
        otherMusic = musicB;

        sfxSources = new AudioSource[sfxVoices];
        for (int i = 0; i < sfxSources.Length; i++)
            sfxSources[i] = CreateSource("Sfx " + i);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        musicDuck = 1f;               // Sang màn mới thì nhạc trở lại bình thường (phòng khi thoát từ menu tạm dừng)
    }

    private AudioSource CreateSource(string sourceName)
    {
        var go = new GameObject(sourceName);
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;                              // Game 2D: không có âm thanh không gian
        return source;
    }

    // ---------------- Âm lượng (lưu trong SaveData, dùng được cả khi chưa có AudioManager) ----------------

    public static float MasterVolume
    {
        get { return SaveSystem.Data.masterVolume; }
        set { SaveSystem.Data.masterVolume = Mathf.Clamp01(value); }
    }

    public static float GetVolume(AudioCategory category)
    {
        SaveData d = SaveSystem.Data;
        switch (category)
        {
            case AudioCategory.Music: return d.musicVolume;
            case AudioCategory.Sfx: return d.sfxVolume;
            default: return d.uiVolume;
        }
    }

    public static void SetVolume(AudioCategory category, float value)
    {
        value = Mathf.Clamp01(value);
        SaveData d = SaveSystem.Data;
        switch (category)
        {
            case AudioCategory.Music: d.musicVolume = value; break;
            case AudioCategory.Sfx: d.sfxVolume = value; break;
            default: d.uiVolume = value; break;
        }
    }

    // Âm lượng thực tế = (tổng x loại) bình phương: thanh kéo ở giữa cho cảm giác "nhỏ một nửa" tự nhiên hơn
    private static float Effective(AudioCategory category)
    {
        float v = MasterVolume * GetVolume(category);
        return v * v;
    }

    // ---------------- Hiệu ứng âm thanh ----------------

    public static void Play(AudioCue cue)
    {
        if (instance != null) instance.PlayInternal(cue);
    }

    private void PlayInternal(AudioCue cue)
    {
        if (cue == null) return;
        if (!cue.HasClip)
        {
            ReportSilent(cue);
            return;
        }

        // Chống phát dồn dập: hai lần phát của cùng một ô phải cách nhau tối thiểu minInterval
        float now = Time.unscaledTime;
        if (lastPlay.TryGetValue(cue, out float last) && now - last < cue.minInterval) return;
        lastPlay[cue] = now;

        float volume = cue.volume * Effective(cue.category);
        if (volume <= 0.001f) return;

        AudioSource source = NextSource();
        source.clip = cue.PickClip();
        source.volume = volume;
        source.pitch = Random.Range(cue.pitchRange.x, cue.pitchRange.y);
        source.loop = false;
        source.Play();
    }

    // Ưu tiên nguồn đang rảnh; hết rảnh thì "cướp" nguồn kế tiếp theo vòng tròn (âm cũ nhất bị cắt)
    private AudioSource NextSource()
    {
        for (int i = 0; i < sfxSources.Length; i++)
        {
            int index = (nextSfx + i) % sfxSources.Length;
            if (sfxSources[index].isPlaying) continue;

            nextSfx = (index + 1) % sfxSources.Length;
            return sfxSources[index];
        }

        AudioSource stolen = sfxSources[nextSfx];
        nextSfx = (nextSfx + 1) % sfxSources.Length;
        return stolen;
    }

    private void ReportSilent(AudioCue cue)
    {
        if (!logSilentCues || !reportedSilent.Add(cue)) return;     // Mỗi ô chỉ in một lần
        string name = string.IsNullOrEmpty(cue.label) ? "chưa đặt tên" : cue.label;
        Debug.Log($"[Audio] Ô âm thanh '{name}' được gọi nhưng chưa có file.");
    }

    // ---------------- Nhạc nền ----------------

    // Phát một bài nhạc (lặp lại). Truyền null hoặc ô trống để tắt nhạc dần dần.
    public static void PlayMusic(AudioCue cue)
    {
        if (instance != null) instance.PlayMusicInternal(cue);
    }

    // Hạ nhỏ nhạc nền (ví dụ 0.35 khi tạm dừng, 1 là bình thường)
    public static void SetMusicDuck(float multiplier)
    {
        if (instance != null) instance.musicDuck = multiplier;
    }

    private void PlayMusicInternal(AudioCue cue)
    {
        AudioClip clip = (cue != null && cue.HasClip) ? cue.PickClip() : null;
        if (clip == currentMusicClip) return;           // Đang phát đúng bài này (hoặc đang im lặng và vẫn yêu cầu im lặng)

        // Hoán đổi vai: nguồn đang phát chuyển sang "mờ dần", nguồn còn lại nhận bài mới và "rõ dần"
        AudioSource previous = activeMusic;
        activeMusic = previous == musicA ? musicB : musicA;
        otherMusic = previous;
        otherFade = activeFade;
        otherCueVolume = currentCueVolume;

        currentMusicClip = clip;
        currentCueVolume = cue != null ? cue.volume : 0f;
        activeFade = 0f;

        if (clip != null)
        {
            activeMusic.clip = clip;
            activeMusic.loop = true;
            activeMusic.volume = 0f;
            activeMusic.Play();
        }
    }

    private void Update()
    {
        // Dùng thời gian không bị ảnh hưởng bởi timeScale để nhạc vẫn chuyển mượt khi game tạm dừng
        float step = Time.unscaledDeltaTime / Mathf.Max(0.01f, musicFadeTime);
        float effective = Effective(AudioCategory.Music) * musicDuck;

        activeFade = Mathf.MoveTowards(activeFade, currentMusicClip != null ? 1f : 0f, step);
        otherFade = Mathf.MoveTowards(otherFade, 0f, step);

        if (activeMusic.isPlaying) activeMusic.volume = activeFade * currentCueVolume * effective;
        if (otherMusic.isPlaying) otherMusic.volume = otherFade * otherCueVolume * effective;

        // Dừng hẳn các nguồn đã mờ hết
        if (otherFade <= 0f && otherMusic.isPlaying) otherMusic.Stop();
        if (currentMusicClip == null && activeFade <= 0f && activeMusic.isPlaying) activeMusic.Stop();
    }

    // ---------------- Tiếng bíp thử trong màn hình Settings ----------------

    public static void PlayPreview(AudioCategory category)
    {
        if (instance != null) instance.PlayPreviewInternal(category);
    }

    private void PlayPreviewInternal(AudioCategory category)
    {
        int index = (int)category;
        if (previewTones[index] == null)
        {
            float frequency = category == AudioCategory.Music ? 330f : (category == AudioCategory.Sfx ? 660f : 880f);
            previewTones[index] = MakeTone(frequency, 0.18f);
        }

        AudioSource source = NextSource();
        source.clip = previewTones[index];
        source.volume = 0.8f * Effective(category);
        source.pitch = 1f;
        source.loop = false;
        source.Play();
    }

    // Tạo một tiếng bíp (sóng sin tắt dần) bằng code, nên thử âm lượng được ngay cả khi chưa có file nào
    private static AudioClip MakeTone(float frequency, float duration)
    {
        const int sampleRate = 22050;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (float)i / samples;               // Tắt dần để không bị tiếng "tạch" ở cuối
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Preview_" + frequency, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}