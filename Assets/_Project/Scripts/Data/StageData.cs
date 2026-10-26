using System.Collections.Generic;
using UnityEngine;

// Một mốc spawn: từ startTime (giây) trở đi, mỗi chu kỳ spawn bấy nhiêu quái mỗi loại
[System.Serializable]
public class SpawnPhase
{
    public float startTime;                              // Tính bằng GIÂY (3:00 = 180)
    public int melee, ranged, tank, mage, assassin;      // Cận chiến/Xạ thủ/Đỡ đòn/Pháp sư/Sát thủ

    public int GetCount(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Melee: return melee;
            case EnemyType.Ranged: return ranged;
            case EnemyType.Tank: return tank;
            case EnemyType.Mage: return mage;
            case EnemyType.Assassin: return assassin;
            default: return 0;
        }
    }
}

// Dữ liệu một màn chơi. Tạo bằng: Assets > Create > Astral Sphere > Stage Data
[CreateAssetMenu(menuName = "Astral Sphere/Stage Data")]
public class StageData : ScriptableObject
{
    public float spawnInterval = 2f;                     // Chu kỳ spawn (tài liệu: 2 giây)
    public float bossTime = 1800f;                       // 30:00 BOSS xuất hiện (dùng ở giai đoạn sau)
    public bool stopNormalSpawnOnBoss = false;           // true: hết 30:00 thì ngừng spawn quái thường

    // Hệ số máu/sát thương theo thời gian. Trục X = PHÚT, trục Y = hệ số nhân.
    // Chỉnh trực tiếp đường cong trong Inspector để cân bằng độ khó, không cần sửa code.
    public AnimationCurve hpMultiplier = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(30, 4f));
    public AnimationCurve damageMultiplier = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(30, 3f));

    [Tooltip("PHẢI sắp xếp theo startTime tăng dần")]
    public List<SpawnPhase> phases = new List<SpawnPhase>();

    // Trả về mốc spawn đang hiệu lực tại thời điểm t (giây). Chưa tới mốc đầu tiên thì trả về null.
    public SpawnPhase GetPhase(float t)
    {
        SpawnPhase result = null;
        foreach (var p in phases)
        {
            if (p.startTime <= t) result = p;   // Mốc nào đã qua thì ghi nhận, lấy mốc cuối cùng đã qua
            else break;
        }
        return result;
    }
}