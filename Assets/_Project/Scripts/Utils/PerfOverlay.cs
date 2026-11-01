using UnityEngine;

// CÔNG CỤ ĐO: hiện FPS, số quái, số vật phẩm, bộ nhớ. Chỉ chạy ở Editor và Development Build.
public class PerfOverlay : MonoBehaviour
{
    [SerializeField] private float uiScale = 2.5f;

    private int frames;
    private float timer;
    private float fps;
    private float worstFrameMs;
    private float shownWorstMs;

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        frames++;
        timer += dt;
        worstFrameMs = Mathf.Max(worstFrameMs, dt * 1000f);

        if (timer < 0.5f) return;                    // Cập nhật số hiển thị mỗi nửa giây
        fps = frames / timer;
        shownWorstMs = worstFrameMs;
        frames = 0;
        timer = 0f;
        worstFrameMs = 0f;
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
        long megabytes = System.GC.GetTotalMemory(false) / 1048576;
        GUI.Label(new Rect(10, 90, 600, 30),
            $"FPS {fps:0} (tệ nhất {shownWorstMs:0}ms)  Quái {EnemyBase.ActiveCount}  Vật phẩm {Pickup.ActiveCount}  RAM {megabytes}MB");
    }
}