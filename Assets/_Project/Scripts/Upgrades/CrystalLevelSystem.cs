using System;
using System.Collections.Generic;
using UnityEngine;

public class CrystalLevelSystem : MonoBehaviour
{
    [SerializeField] private int thresholdStep = 10;   // Lv1: 10, Lv2: 30, Lv3: 60...
    [SerializeField] private UpgradeGenerator generator;
    [SerializeField] private LevelUpUI levelUpUI;

    public int Level { get; private set; } = 1;
    public int Crystals { get; private set; }          // Tổng pha lê đã nhặt (tích lũy)
    public int PreviousThreshold => thresholdStep * (Level - 1) * Level / 2;
    public int NextThreshold => thresholdStep * Level * (Level + 1) / 2;

    private readonly RemainderAccumulator crystalAcc = new RemainderAccumulator();
    private float crystalMultiplier = 1f;

    public event Action OnChanged;                     // HUD lắng nghe để cập nhật thanh pha lê

    private bool isChoosing;                           // Đang hiện màn chọn nâng cấp

    private void Awake()
    {
        crystalMultiplier = 1f + MetaProgress.GetSharedBonus(SharedUpgradeType.Crystal);
    }


    private void OnEnable() { Pickup.Collected += HandleCollected; }
    private void OnDisable() { Pickup.Collected -= HandleCollected; }

    private void HandleCollected(PickupType type, int amount)
    {
        if (type != PickupType.Crystal) return;

        int gained = crystalAcc.Add(amount * crystalMultiplier);
        if (gained <= 0) return;                         // Chưa tích đủ phần lẻ

        Crystals += gained;
        OnChanged?.Invoke();
        TryLevelUp();
    }

    private void TryLevelUp()
    {
        // Dùng while vì nhặt một lúc nhiều pha lê có thể vượt qua nhiều ngưỡng
        while (!isChoosing && !GameManager.Instance.IsEnded && Crystals >= NextThreshold)
        {
            List<UpgradeOption> options = generator.Generate(3);

            if (options.Count == 0)   // Không còn gì để chọn: chỉ tăng level
            {
                Level++;
                OnChanged?.Invoke();
                continue;
            }

            isChoosing = true;
            GameManager.Instance.Pause();
            levelUpUI.Show(options, OnOptionChosen);
        }
    }

    private void OnOptionChosen(UpgradeOption option)
    {
        option.Apply();                    // Chạy hành động của lựa chọn
        Level++;
        isChoosing = false;
        GameManager.Instance.Resume();
        OnChanged?.Invoke();
        TryLevelUp();                      // Nếu còn đủ pha lê cho cấp tiếp thì hiện màn chọn tiếp
    }
}