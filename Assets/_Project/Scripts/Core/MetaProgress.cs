using System;
using System.Collections.Generic;
using UnityEngine;

public struct RunResult
{
    public int score;
    public int timeSeconds;
    public int kills;
    public int gold;
    public bool victory;
}

public struct RunSubmitResult
{
    public int goldEarned;
    public bool isNewBest;
    public int rank;          // Hạng trong bảng điểm cao (1 = cao nhất); 0 = không lọt top
}

// Cổng truy cập duy nhất tới tiến trình vĩnh viễn: ví, nâng cấp, tra bonus, ghi kết quả trận
public static class MetaProgress
{
    public const int MaxLevel = 10;
    public const int MaxStars = 5;
    public const int MaxScores = 10;

    // UI (giai đoạn 7B) lắng nghe để vẽ lại khi dữ liệu thay đổi
    public static event Action OnChanged;

    private static MetaConfig config;
    private static bool warnedMissingConfig;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        config = null;
        OnChanged = null;
        warnedMissingConfig = false;
    }

    public static MetaConfig Config
    {
        get
        {
            if (config == null) config = Resources.Load<MetaConfig>("MetaConfig");
            if (config == null && !warnedMissingConfig)
            {
                warnedMissingConfig = true;
                Debug.LogError("MetaProgress: thiếu asset MetaConfig. Đặt trong thư mục Resources, tên file MetaConfig.");
            }
            return config;
        }
    }

    // ---------------- Ví vàng ----------------

    public static int Gold => SaveSystem.Data.gold;

    private static bool TrySpend(int cost)
    {
        if (cost < 0 || SaveSystem.Data.gold < cost) return false;
        SaveSystem.Data.gold -= cost;
        return true;
    }

    private static void Commit()
    {
        SaveSystem.Save();
        OnChanged?.Invoke();
    }

    // ---------------- Nhân vật ----------------

    // Nhân vật đang được chọn. Chưa chọn (hoặc id cũ không còn) thì dùng nhân vật đầu tiên trong MetaConfig.
    public static CharacterData GetSelectedCharacter()
    {
        MetaConfig cfg = Config;
        if (cfg == null || cfg.characters.Count == 0) return null;

        CharacterData found = cfg.GetCharacter(SaveSystem.Data.selectedCharacterId);
        return found != null ? found : cfg.characters[0];
    }

    public static void SelectCharacter(string id)
    {
        SaveSystem.Data.selectedCharacterId = id;
        Commit();
    }

    public static int GetStars(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        return SaveSystem.Data.GetOrCreateCharacter(id).stars;
    }

    public static int GetStatLevel(string id, StatType stat)
    {
        if (string.IsNullOrEmpty(id)) return 1;
        return SaveSystem.Data.GetOrCreateCharacter(id).statLevels[(int)stat];
    }

    // Bonus hiện tại của một chỉ số theo cấp đã nâng (0 nếu chưa cấu hình)
    public static float GetStatBonus(CharacterData c, StatType stat)
    {
        StatProgression p = c.GetProgression(stat);
        if (p == null || p.bonusByLevel == null || p.bonusByLevel.Length == 0) return 0f;

        int level = GetStatLevel(c.id, stat);
        return p.bonusByLevel[Mathf.Clamp(level, 1, p.bonusByLevel.Length) - 1];
    }

    // Vàng để nâng chỉ số lên cấp kế tiếp. -1 nếu đã tối đa hoặc chưa cấu hình.
    public static int GetStatUpgradeCost(CharacterData c, StatType stat)
    {
        int level = GetStatLevel(c.id, stat);
        if (level >= MaxLevel) return -1;

        StatProgression p = c.GetProgression(stat);
        if (p == null || p.costByLevel == null || level >= p.costByLevel.Length) return -1;
        return p.costByLevel[level];     // Chi phí để lên cấp (level + 1) nằm ở chỉ số mảng "level"
    }

    public static bool TryUpgradeStat(CharacterData c, StatType stat)
    {
        int cost = GetStatUpgradeCost(c, stat);
        if (!TrySpend(cost)) return false;

        SaveSystem.Data.GetOrCreateCharacter(c.id).statLevels[(int)stat]++;
        Commit();
        return true;
    }

    // Vàng để lên sao kế tiếp. -1 nếu đã đủ 5 sao.
    public static int GetStarUpgradeCost(CharacterData c)
    {
        int stars = GetStars(c.id);
        if (stars >= MaxStars || c.starCosts == null || stars >= c.starCosts.Length) return -1;
        return c.starCosts[stars];       // Chi phí lên sao (stars + 1) nằm ở chỉ số mảng "stars"
    }

    public static bool TryUpgradeStar(CharacterData c)
    {
        int cost = GetStarUpgradeCost(c);
        if (!TrySpend(cost)) return false;

        SaveSystem.Data.GetOrCreateCharacter(c.id).stars++;
        Commit();
        return true;
    }

    // Bonus của chỉ số ở cấp KẾ TIẾP (dùng để hiện "20 > 30" trên nút nâng cấp)
    public static float GetNextStatBonus(CharacterData c, StatType stat)
    {
        StatProgression p = c.GetProgression(stat);
        if (p == null || p.bonusByLevel == null || p.bonusByLevel.Length == 0) return 0f;

        int next = Mathf.Min(GetStatLevel(c.id, stat) + 1, MaxLevel);
        return p.bonusByLevel[Mathf.Clamp(next, 1, p.bonusByLevel.Length) - 1];
    }

    public static float GetNextSharedBonus(SharedUpgradeType type)
    {
        MetaConfig cfg = Config;
        if (cfg == null) return 0f;

        SharedUpgradeEntry e = cfg.GetShared(type);
        if (e == null || e.bonusByLevel == null || e.bonusByLevel.Length == 0) return 0f;

        int next = Mathf.Min(GetSharedLevel(type) + 1, MaxLevel);
        return e.bonusByLevel[Mathf.Clamp(next, 1, e.bonusByLevel.Length) - 1];
    }

    // ---------------- Nâng cấp chung ----------------

    public static int GetSharedLevel(SharedUpgradeType type)
    {
        return SaveSystem.Data.sharedLevels[(int)type];
    }

    // Bonus hiện tại của nâng cấp chung (0 nếu chưa cấu hình MetaConfig)
    public static float GetSharedBonus(SharedUpgradeType type)
    {
        MetaConfig cfg = Config;
        if (cfg == null) return 0f;

        SharedUpgradeEntry e = cfg.GetShared(type);
        if (e == null || e.bonusByLevel == null || e.bonusByLevel.Length == 0) return 0f;

        int level = GetSharedLevel(type);
        return e.bonusByLevel[Mathf.Clamp(level, 1, e.bonusByLevel.Length) - 1];
    }

    public static int GetSharedUpgradeCost(SharedUpgradeType type)
    {
        MetaConfig cfg = Config;
        if (cfg == null) return -1;

        int level = GetSharedLevel(type);
        SharedUpgradeEntry e = cfg.GetShared(type);
        if (level >= MaxLevel || e == null || e.costByLevel == null || level >= e.costByLevel.Length) return -1;
        return e.costByLevel[level];
    }

    public static bool TryUpgradeShared(SharedUpgradeType type)
    {
        int cost = GetSharedUpgradeCost(type);
        if (!TrySpend(cost)) return false;

        SaveSystem.Data.sharedLevels[(int)type]++;
        Commit();
        return true;
    }

    // ---------------- Kết thúc trận ----------------

    //public static IReadOnlyList<ScoreEntry> TopScores => SaveSystem.Data.highScores;

    // Cộng vàng của trận vào ví và ghi bảng điểm của map đang chơi. Gọi MỘT LẦN khi trận kết thúc.
    public static RunSubmitResult SubmitRun(RunResult run)
    {
        SaveData d = SaveSystem.Data;
        d.gold += run.gold;

        MapData map = GetSelectedMap();
        string mapId = map != null ? map.id : "";
        CharacterData character = GetSelectedCharacter();

        var entry = new ScoreEntry
        {
            score = run.score,
            timeSeconds = run.timeSeconds,
            kills = run.kills,
            victory = run.victory,
            characterId = character != null ? character.id : "",
            mapId = mapId,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        // Kỷ lục của RIÊNG map này. Phải tính TRƯỚC khi thêm điểm mới vào danh sách.
        bool isNewBest = run.score > 0 && run.score > BestScore(mapId);

        d.highScores.Add(entry);
        TrimScores(mapId);
        int rank = GetTopScores(mapId).IndexOf(entry) + 1;     // IndexOf = -1 nếu bị loại khỏi top, nên rank = 0

        Commit();
        return new RunSubmitResult { goldEarned = run.gold, isNewBest = isNewBest, rank = rank };
    }

    // ---------------- Công cụ debug ----------------

    public static void DebugAddGold(int amount)
    {
        SaveSystem.Data.gold += amount;
        Commit();
    }

    public static void DebugResetAll()
    {
        SaveSystem.ResetAll();
        OnChanged?.Invoke();
    }

    // ---------------- Map ----------------

    // Map đang chọn (lần cuối chọn ở màn Map). Chưa chọn hoặc map đã khóa thì dùng map đầu tiên còn mở.
    public static MapData GetSelectedMap()
    {
        MetaConfig cfg = Config;
        if (cfg == null || cfg.maps.Count == 0) return null;

        MapData found = cfg.GetMap(SaveSystem.Data.lastMapId);
        if (found != null && found.isAvailable) return found;

        foreach (MapData m in cfg.maps)
            if (m != null && m.isAvailable) return m;
        return cfg.maps[0];
    }

    public static void SelectMap(string id)
    {
        SaveSystem.Data.lastMapId = id;
        Commit();
    }

    // ---------------- Xếp hạng theo map ----------------

    // Top điểm của MỘT map, xếp từ cao xuống thấp (tạo danh sách mới mỗi lần gọi, chỉ dùng cho giao diện)
    public static List<ScoreEntry> GetTopScores(string mapId)
    {
        var list = new List<ScoreEntry>();
        foreach (ScoreEntry e in SaveSystem.Data.highScores)
            if (BelongsToMap(e, mapId)) list.Add(e);

        list.Sort((a, b) => b.score.CompareTo(a.score));
        return list;
    }

    private static bool BelongsToMap(ScoreEntry e, string mapId)
    {
        if (e.mapId == mapId) return true;
        // Điểm cũ từ trước khi có map (giai đoạn 7A) không có mapId: xếp vào map mặc định
        return string.IsNullOrEmpty(e.mapId) && mapId == DefaultMapId();
    }

    private static string DefaultMapId()
    {
        MetaConfig cfg = Config;
        return (cfg != null && cfg.maps.Count > 0 && cfg.maps[0] != null) ? cfg.maps[0].id : "";
    }

    private static int BestScore(string mapId)
    {
        int best = 0;
        foreach (ScoreEntry e in SaveSystem.Data.highScores)
            if (BelongsToMap(e, mapId) && e.score > best) best = e.score;
        return best;
    }

    // Chỉ giữ MaxScores điểm cao nhất CỦA MAP NÀY, xóa phần dư khỏi danh sách chung
    private static void TrimScores(string mapId)
    {
        List<ScoreEntry> top = GetTopScores(mapId);
        for (int i = MaxScores; i < top.Count; i++)
            SaveSystem.Data.highScores.Remove(top[i]);
    }
}