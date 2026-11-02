using UnityEngine;

public enum AudioCategory { Music, Sfx, Ui }

// Một "ô âm thanh". Là class nhúng trong các asset dữ liệu, không phải asset riêng.
// Để trống (không có clip) thì không phát gì và không báo lỗi.
[System.Serializable]
public class AudioCue
{
    [HideInInspector] public string label;                    // Tên để in nhật ký (do AudioLibrary tự gán)
    public AudioClip[] clips;                                 // Nhiều file thì mỗi lần phát chọn ngẫu nhiên một file
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);    // Cao độ ngẫu nhiên trong khoảng này để bớt đơn điệu
    public float minInterval = 0.05f;                         // Hai lần phát phải cách nhau ít nhất chừng này giây
    [HideInInspector] public AudioCategory category = AudioCategory.Sfx;

    public bool HasClip
    {
        get
        {
            if (clips == null) return false;
            for (int i = 0; i < clips.Length; i++)
                if (clips[i] != null) return true;
            return false;
        }
    }

    // Chọn ngẫu nhiên một clip hợp lệ (null nếu chưa có)
    public AudioClip PickClip()
    {
        if (clips == null || clips.Length == 0) return null;

        int start = Random.Range(0, clips.Length);
        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip clip = clips[(start + i) % clips.Length];
            if (clip != null) return clip;
        }
        return null;
    }
}