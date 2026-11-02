using UnityEngine;

// Thư viện âm thanh dùng chung. Tạo bằng: Assets > Create > Astral Sphere > Audio Library
[CreateAssetMenu(menuName = "Astral Sphere/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Nhạc nền")]
    public AudioCue homeMusic = new AudioCue();

    [Header("Chiến đấu")]
    public AudioCue enemyDeath = new AudioCue { minInterval = 0.04f };
    public AudioCue bossDeath = new AudioCue();
    public AudioCue bossAppear = new AudioCue();
    public AudioCue playerHurt = new AudioCue { minInterval = 0.25f };
    public AudioCue aoaStart = new AudioCue();
    public AudioCue aoaEnd = new AudioCue();

    [Header("Nhặt vật phẩm")]
    public AudioCue pickupGold = new AudioCue { minInterval = 0.03f };
    public AudioCue pickupCrystal = new AudioCue { minInterval = 0.03f };
    public AudioCue pickupItem = new AudioCue();

    [Header("Kết thúc trận")]
    public AudioCue victory = new AudioCue();
    public AudioCue gameOver = new AudioCue();

    [Header("Giao diện")]
    public AudioCue buttonClick = new AudioCue();
    public AudioCue levelUp = new AudioCue();
    public AudioCue upgradeChosen = new AudioCue();

    // Mỗi lần sửa asset trong Inspector: gán tên và loại cho từng ô
    private void OnValidate()
    {
        Setup(homeMusic, nameof(homeMusic), AudioCategory.Music);

        Setup(enemyDeath, nameof(enemyDeath), AudioCategory.Sfx);
        Setup(bossDeath, nameof(bossDeath), AudioCategory.Sfx);
        Setup(bossAppear, nameof(bossAppear), AudioCategory.Sfx);
        Setup(playerHurt, nameof(playerHurt), AudioCategory.Sfx);
        Setup(aoaStart, nameof(aoaStart), AudioCategory.Sfx);
        Setup(aoaEnd, nameof(aoaEnd), AudioCategory.Sfx);
        Setup(pickupGold, nameof(pickupGold), AudioCategory.Sfx);
        Setup(pickupCrystal, nameof(pickupCrystal), AudioCategory.Sfx);
        Setup(pickupItem, nameof(pickupItem), AudioCategory.Sfx);
        Setup(victory, nameof(victory), AudioCategory.Sfx);
        Setup(gameOver, nameof(gameOver), AudioCategory.Sfx);

        Setup(buttonClick, nameof(buttonClick), AudioCategory.Ui);
        Setup(levelUp, nameof(levelUp), AudioCategory.Ui);
        Setup(upgradeChosen, nameof(upgradeChosen), AudioCategory.Ui);
    }

    private static void Setup(AudioCue cue, string label, AudioCategory category)
    {
        cue.label = label;
        cue.category = category;
    }
}