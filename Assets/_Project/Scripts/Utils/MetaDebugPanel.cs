using UnityEngine;

// CÔNG CỤ TEST: bảng nâng cấp tạm. Gắn vào một object trong scene Home. Bỏ đi khi có menu thật (7B).
public class MetaDebugPanel : MonoBehaviour
{
    [SerializeField] private float uiScale = 2f;

    private Vector2 scroll;
    private bool visible = true;

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;     // Chỉ hiện trong Editor và Development Build

        GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
        float w = Screen.width / uiScale;
        float h = Screen.height / uiScale;

        if (GUI.Button(new Rect(w - 80, 5, 75, 30), visible ? "Ẩn" : "DEBUG")) visible = !visible;
        if (!visible) return;

        MetaConfig config = MetaProgress.Config;
        if (config == null) { GUI.Label(new Rect(10, 10, w, 30), "Thiếu MetaConfig trong thư mục Resources"); return; }

        GUILayout.BeginArea(new Rect(10, 40, w - 20, h - 50));
        scroll = GUILayout.BeginScrollView(scroll);

        GUILayout.Label($"VÀNG: {MetaProgress.Gold}");
        if (GUILayout.Button("+10000 vàng")) MetaProgress.DebugAddGold(10000);
        if (GUILayout.Button("In file lưu ra Console"))
            Debug.Log(SaveSystem.FilePath + "\n" + JsonUtility.ToJson(SaveSystem.Data, true));
        if (GUILayout.Button("XÓA TOÀN BỘ SAVE")) MetaProgress.DebugResetAll();

        CharacterData c = MetaProgress.GetSelectedCharacter();
        if (c != null)
        {
            GUILayout.Space(8);
            GUILayout.Label($"NHÂN VẬT: {c.characterName} ({c.id})");

            int starCost = MetaProgress.GetStarUpgradeCost(c);
            if (GUILayout.Button($"Sao: {MetaProgress.GetStars(c.id)}/5   nâng: {CostText(starCost)}"))
                MetaProgress.TryUpgradeStar(c);

            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            {
                int cost = MetaProgress.GetStatUpgradeCost(c, stat);
                string line = $"{stat}: Lv{MetaProgress.GetStatLevel(c.id, stat)}   bonus {MetaProgress.GetStatBonus(c, stat):0.##}   nâng: {CostText(cost)}";
                if (GUILayout.Button(line)) MetaProgress.TryUpgradeStat(c, stat);
            }
        }

        GUILayout.Space(8);
        GUILayout.Label("NÂNG CẤP CHUNG");
        foreach (SharedUpgradeType type in System.Enum.GetValues(typeof(SharedUpgradeType)))
        {
            int cost = MetaProgress.GetSharedUpgradeCost(type);
            string line = $"{type}: Lv{MetaProgress.GetSharedLevel(type)}   bonus {MetaProgress.GetSharedBonus(type):0.##}   nâng: {CostText(cost)}";
            if (GUILayout.Button(line)) MetaProgress.TryUpgradeShared(type);
        }

        GUILayout.Space(8);
        GUILayout.Label("BẢNG ĐIỂM CAO");
        int rank = 1;
        foreach (ScoreEntry s in MetaProgress.TopScores)
            GUILayout.Label($"{rank++}. {s.score} điểm | {s.timeSeconds / 60:00}:{s.timeSeconds % 60:00} | {(s.victory ? "Thắng" : "Thua")} | {s.date}");

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static string CostText(int cost)
    {
        return cost < 0 ? "TỐI ĐA" : cost.ToString();
    }
}