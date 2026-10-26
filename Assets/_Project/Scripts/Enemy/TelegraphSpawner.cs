using System;
using UnityEngine;

public class TelegraphSpawner : MonoBehaviour
{
    public static TelegraphSpawner Instance { get; private set; }

    [SerializeField] private GameObject sectorPrefab;
    [SerializeField] private GameObject rectPrefab;
    [SerializeField] private GameObject rayPrefab;
    [SerializeField] private GameObject sweepPrefab;

    private void Awake()
    {
        Instance = this;
    }

    // Dạng 1: vòng tròn. shrink = false: vòng trong lớn dần từ tâm. shrink = true: nhỏ dần vào tâm.
    public SectorTelegraph SpawnCircle(Vector2 center, float radius, float duration, bool shrink,
                                       float damage, Action<Telegraph> onTriggered = null)
    {
        var t = Spawn<SectorTelegraph>(sectorPrefab, center, 0f);
        t.Begin(radius, 360f, duration, shrink, damage, onTriggered);
        return t;
    }

    // Dạng 3: hình quạt hướng theo facingDeg (0 = sang phải, 90 = lên trên)
    public SectorTelegraph SpawnFan(Vector2 origin, float facingDeg, float radius, float angleDeg,
                                    float duration, bool shrink, float damage,
                                    Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        var t = Spawn<SectorTelegraph>(sectorPrefab, origin, facingDeg);
        t.Begin(radius, angleDeg, duration, shrink, damage, onTriggered, follow);
        return t;
    }

    // Dạng 2 và 4: hình chữ nhật xuất phát từ origin, hướng facingDeg
    public RectTelegraph SpawnBox(Vector2 origin, float facingDeg, float length, float width,
                                  float duration, RectFill fill, float damage,
                                  Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        var t = Spawn<RectTelegraph>(rectPrefab, origin, facingDeg);
        t.Begin(length, width, duration, fill, damage, onTriggered, follow);
        return t;
    }

    // Dạng 5: tia đỏ ngắm theo người chơi. damage = 0 nếu BOSS tự bắn đạn trong onTriggered.
    public RayTelegraph SpawnRay(Vector2 origin, float length, float width, float duration,
                                 float lockFraction, float damage,
                                 Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        var t = Spawn<RayTelegraph>(rayPrefab, origin, 0f);
        t.Begin(length, width, duration, lockFraction, damage, onTriggered, follow);
        return t;
    }

    // Dạng 6: đòn "pizza". safeFacingDeg = hướng tâm miếng an toàn (0 = sang phải, 90 = lên trên).
    public SweepTelegraph SpawnSweep(Vector2 origin, float safeFacingDeg, float radius, float safeAngleDeg,
                                     SweepDirection direction, float duration, float damage,
                                     Action<Telegraph> onTriggered = null, Transform follow = null)
    {
        var t = Spawn<SweepTelegraph>(sweepPrefab, origin, safeFacingDeg);
        t.Begin(radius, safeAngleDeg, direction, duration, damage, onTriggered, follow);
        return t;
    }

    private T Spawn<T>(GameObject prefab, Vector2 position, float facingDeg) where T : Telegraph
    {
        // Lấy từ pool, đặt vị trí và hướng. GetComponent lúc spawn (hiếm) là chấp nhận được.
        GameObject go = PoolManager.Instance.Get(prefab, position, Quaternion.Euler(0f, 0f, facingDeg));
        return go.GetComponent<T>();
    }
}