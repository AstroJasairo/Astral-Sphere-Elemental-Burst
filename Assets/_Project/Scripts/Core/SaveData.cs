using System;
using System.Collections.Generic;
using UnityEngine;

// Một dòng trong bảng điểm cao
[Serializable]
public class ScoreEntry
{
    public int score;
    public int timeSeconds;
    public int kills;
    public bool victory;
    public string characterId;
    public string date;
}

// Tiến trình vĩnh viễn của một nhân vật
[Serializable]
public class CharacterProgress
{
    public string id;
    public int stars;            // 0 đến 5
    public int[] statLevels;     // Cấp của 6 chỉ số theo thứ tự enum StatType, bắt đầu từ 1

    // Sửa dữ liệu thiếu hoặc sai (file cũ, file bị sửa tay, thêm chỉ số mới...)
    public void Normalize()
    {
        int count = SaveData.StatCount;
        if (statLevels == null || statLevels.Length != count)
        {
            int[] fixedLevels = new int[count];
            for (int i = 0; i < count; i++)
                fixedLevels[i] = (statLevels != null && i < statLevels.Length) ? statLevels[i] : 1;
            statLevels = fixedLevels;
        }
        for (int i = 0; i < count; i++)
            statLevels[i] = Mathf.Clamp(statLevels[i], 1, MetaProgress.MaxLevel);

        stars = Mathf.Clamp(stars, 0, MetaProgress.MaxStars);
    }
}

// Toàn bộ dữ liệu được lưu ra file
[Serializable]
public class SaveData
{
    // Biến static không được JsonUtility lưu
    public static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;
    public static readonly int SharedCount = Enum.GetValues(typeof(SharedUpgradeType)).Length;

    public int version = 1;                  // Dùng để chuyển đổi định dạng sau này
    public int gold;                         // Ví vàng
    public string selectedCharacterId = "";
    public List<CharacterProgress> characters = new List<CharacterProgress>();
    public int[] sharedLevels;               // Cấp của 9 nâng cấp chung theo thứ tự enum SharedUpgradeType
    public List<ScoreEntry> highScores = new List<ScoreEntry>();

    public CharacterProgress GetOrCreateCharacter(string id)
    {
        foreach (CharacterProgress c in characters)
            if (c.id == id) return c;

        var created = new CharacterProgress { id = id };
        created.Normalize();                 // Tạo sẵn mảng cấp = 1
        characters.Add(created);
        return created;
    }

    public void Normalize()
    {
        gold = Mathf.Max(0, gold);
        if (characters == null) characters = new List<CharacterProgress>();
        if (highScores == null) highScores = new List<ScoreEntry>();

        foreach (CharacterProgress c in characters) c.Normalize();

        if (sharedLevels == null || sharedLevels.Length != SharedCount)
        {
            int[] fixedLevels = new int[SharedCount];
            for (int i = 0; i < SharedCount; i++)
                fixedLevels[i] = (sharedLevels != null && i < sharedLevels.Length) ? sharedLevels[i] : 1;
            sharedLevels = fixedLevels;
        }
        for (int i = 0; i < SharedCount; i++)
            sharedLevels[i] = Mathf.Clamp(sharedLevels[i], 1, MetaProgress.MaxLevel);
    }
}