#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Chuyển ThunderBirdData / LordOfTheWolfData (số kỹ năng cũ) thành các asset module, KHÔNG phải nhập lại số.
// CHẠY TRƯỚC khi thay component BOSS trên prefab (công cụ còn đọc được prefab ở trạng thái cũ).
public static class BossModuleMigration
{
    [MenuItem("Tools/Astral Sphere/Boss/Migrate Selected Boss Data To Modules")]
    private static void Migrate()
    {
        int done = 0;
        foreach (Object o in Selection.objects)
        {
            if (o is ThunderBirdData tb) { MigrateThunderBird(tb); done++; }
            else if (o is LordOfTheWolfData lw) { MigrateWolf(lw); done++; }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(done == 0
            ? "Chưa chọn asset ThunderBirdData hoặc LordOfTheWolfData trong Project."
            : $"Đã chuyển {done} asset BOSS sang module. Giờ thay component BOSS trên prefab bằng BossController.");
    }

    private static void MigrateThunderBird(ThunderBirdData d)
    {
        if (d.activeModules.Count > 0) { Debug.LogWarning($"{d.name} đã có module, bỏ qua.", d); return; }
        string folder = FolderOf(d);

        var strikes = CreateAsset<LightningStrikesSkillData>(folder, d.name + "_Strikes");
        strikes.skillName = "Sét đánh";
        strikes.exclusive = false;
        strikes.pausedWhileOthersExecute = true;
        strikes.levels = new[]
        {
            new LightningStrikesSkillData.StrikeLevel
            {
                firstDelay = d.firstStrikeDelay, cooldown = d.strikeInterval, strikesPerWave = d.strikesPerWave,
                radius = d.strikeRadius, warning = d.strikeWarning, spread = d.strikeSpread, damage = d.strikeDamage
            }
        };
        strikes.effects = new List<HitEffect>(d.strikeEffects);
        strikes.strikeEffectPrefab = ReadPrefabField(d.prefab, "ThunderBirdBoss", "strikeEffectPrefab") as GameObject;
        EditorUtility.SetDirty(strikes);

        var dash = CreateAsset<DashSkillData>(folder, d.name + "_Dash");
        dash.skillName = "Lao tới";
        dash.exclusive = true;
        dash.levels = new[]
        {
            new DashSkillData.DashLevel
            {
                firstDelay = d.firstDashDelay, cooldown = d.dashInterval, warning = d.dashWarning,
                length = d.dashLength, width = d.dashWidth, speed = d.dashSpeed
            }
        };
        EditorUtility.SetDirty(dash);

        d.activeModules.Add(strikes);
        d.activeModules.Add(dash);
        EditorUtility.SetDirty(d);
    }

    private static void MigrateWolf(LordOfTheWolfData d)
    {
        if (d.activeModules.Count > 0 || d.passiveModules.Count > 0) { Debug.LogWarning($"{d.name} đã có module, bỏ qua.", d); return; }
        string folder = FolderOf(d);

        var claw = CreateAsset<ClawSkillData>(folder, d.name + "_Claw");
        claw.skillName = "Cào";
        claw.exclusive = true;
        claw.levels = new[]
        {
            new ClawSkillData.ClawLevel
            {
                firstDelay = d.firstClawDelay, cooldown = d.clawInterval, radius = d.clawRadius,
                warning = d.clawWarning, damage = d.clawDamage, shrink = d.clawShrink
            }
        };
        claw.effects = new List<HitEffect>(d.clawEffects);
        claw.clawEffectPrefab = ReadPrefabField(d.prefab, "LordOfTheWolfBoss", "clawEffectPrefab") as GameObject;
        EditorUtility.SetDirty(claw);

        var far = CreateAsset<ImmuneWhenFarPassiveData>(folder, d.name + "_ImmuneWhenFar");
        far.skillName = "Miễn nhiễm khi ở xa";
        far.immunity = ImmunityFlags.Damage;
        far.levels = new[] { new ImmuneWhenFarPassiveData.FarLevel { distance = d.immuneDistance, margin = d.immuneMargin } };
        EditorUtility.SetDirty(far);

        var threshold = CreateAsset<HpThresholdReductionPassiveData>(folder, d.name + "_HpThresholdReduction");
        threshold.skillName = "Giảm sát thương theo mốc máu";
        threshold.levels = new[]
        {
            new HpThresholdReductionPassiveData.ThresholdLevel
            {
                thresholds = (float[])d.reductionHpThresholds.Clone(), reduction = d.thresholdReduction, duration = d.thresholdDuration
            }
        };
        EditorUtility.SetDirty(threshold);

        d.activeModules.Add(claw);
        d.passiveModules.Add(far);
        d.passiveModules.Add(threshold);
        EditorUtility.SetDirty(d);
    }

    private static string FolderOf(Object asset)
    {
        return Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset)).Replace('\\', '/');
    }

    private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath($"{folder}/{assetName}.asset"));
        return asset;
    }

    // Đọc một trường tham chiếu private của component BOSS cũ trên prefab (strikeEffectPrefab, clawEffectPrefab)
    private static Object ReadPrefabField(GameObject prefab, string componentTypeName, string fieldName)
    {
        if (prefab == null) return null;

        foreach (MonoBehaviour mb in prefab.GetComponents<MonoBehaviour>())
        {
            if (mb == null || mb.GetType().Name != componentTypeName) continue;

            SerializedProperty p = new SerializedObject(mb).FindProperty(fieldName);
            return p != null ? p.objectReferenceValue : null;
        }
        return null;
    }
}
#endif