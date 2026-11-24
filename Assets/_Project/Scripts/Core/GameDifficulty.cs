using UnityEngine;

// Độ khó của trận hiện tại (1 đến 3). MapLoader đặt từ MapData. Chưa có giao diện chọn độ khó.
public static class GameDifficulty
{
    public static int Current { get; set; } = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Current = 1; }
}