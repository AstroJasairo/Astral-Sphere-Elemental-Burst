using UnityEngine;

// Thiết lập chung chạy một lần trước khi vào scene đầu tiên
public static class ReleaseSettings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        QualitySettings.vSyncCount = 0;                       // Tắt đồng bộ dọc để targetFrameRate có tác dụng
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;        // Không tự tắt màn hình khi đang chơi

        // Bản phát hành chỉ ghi lỗi nghiêm trọng, bớt chi phí tạo chuỗi nhật ký
        if (!Debug.isDebugBuild)
            Debug.unityLogger.filterLogType = LogType.Error;
    }
}