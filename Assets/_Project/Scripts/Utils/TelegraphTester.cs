using UnityEngine;
using UnityEngine.InputSystem;

// CÔNG CỤ TEST, chỉ hoạt động trong Editor. Gắn vào Player.
// 1: vòng tròn lớn dần   2: vòng tròn nhỏ dần   3: hình quạt   4: tứ giác ngắn (quét tới)
// 5: tứ giác dài (mở rộng ra hai bên)   6: tia đỏ ngắm theo người chơi   7: xoay toàn map
public class TelegraphTester : MonoBehaviour
{
    [SerializeField] private float testDamage = 10f;

    private void Update()
    {
#if UNITY_EDITOR
        Keyboard kb = Keyboard.current;
        TelegraphSpawner sp = TelegraphSpawner.Instance;
        if (kb == null || sp == null) return;

        Vector2 p = transform.position;

        if (kb.digit1Key.wasPressedThisFrame)
            sp.SpawnCircle(p, 3f, 2f, false, testDamage, Log);
        if (kb.digit2Key.wasPressedThisFrame)
            sp.SpawnCircle(p, 3f, 2f, true, testDamage, Log);
        if (kb.digit3Key.wasPressedThisFrame)
            sp.SpawnFan(p, 90f, 5f, 90f, 2f, false, testDamage, Log);
        if (kb.digit4Key.wasPressedThisFrame)       // Bắt đầu từ 3 đơn vị phía dưới, hướng lên: người chơi nằm giữa hình
            sp.SpawnBox(p + Vector2.down * 3f, 90f, 6f, 3f, 2f, RectFill.SweepForward, testDamage, Log);
        if (kb.digit5Key.wasPressedThisFrame)       // Bắt đầu từ 8 đơn vị bên trái, hướng sang phải
            sp.SpawnBox(p + Vector2.left * 8f, 0f, 20f, 3f, 2.5f, RectFill.ExpandSideways, testDamage, Log);
        if (kb.digit6Key.wasPressedThisFrame)       // Tia đỏ từ 8 đơn vị phía trên, ngắm theo người chơi
            sp.SpawnRay(p + Vector2.up * 8f, 30f, 0.4f, 3f, 0.7f, testDamage, Log);
        if (kb.digit7Key.wasPressedThisFrame)
            sp.SpawnSweep(p + Vector2.down * 4f, 90f, 12f, 60f, SweepDirection.CounterClockwise, 3f, testDamage, Log);
        if (kb.digit8Key.wasPressedThisFrame)
            sp.SpawnSweep(p + Vector2.down * 4f, 90f, 12f, 60f, SweepDirection.Clockwise, 3f, testDamage, Log);
#endif
    }

    private void Log(Telegraph t)
    {
        Debug.Log("Telegraph kích hoạt: " + t.name);
    }
}