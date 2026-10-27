using System;
using System.IO;
using UnityEngine;

// Đọc/ghi file lưu. Mọi nơi khác nên đi qua MetaProgress thay vì dùng trực tiếp.
public static class SaveSystem
{
    private const string FileName = "save.json";
    private static SaveData data;

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    // Tự đọc file ở lần truy cập đầu tiên
    public static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    // Khi vào Play mà tắt "Domain Reload", biến static còn giữ giá trị cũ: xóa để đọc lại từ file
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        data = null;
    }

    public static void Load()
    {
        data = null;

        try
        {
            if (File.Exists(FilePath))
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
        }
        catch (Exception e)
        {
            Debug.LogError("SaveSystem: file lưu bị hỏng, tạo file mới. " + e.Message);
            BackupCorruptFile();
        }

        if (data == null) data = new SaveData();
        data.Normalize();
    }

    public static void Save()
    {
        if (data == null) return;

        try
        {
            // Ghi ra file tạm trước rồi mới chép đè lên file chính, tránh mất dữ liệu nếu app bị tắt giữa chừng
            string tempPath = FilePath + ".tmp";
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));
            File.Copy(tempPath, FilePath, true);
            File.Delete(tempPath);
        }
        catch (Exception e)
        {
            Debug.LogError("SaveSystem: không ghi được file lưu. " + e.Message);
        }
    }

    // Xóa sạch và tạo lại dữ liệu mặc định
    public static void ResetAll()
    {
        data = new SaveData();
        data.Normalize();
        Save();
    }

    private static void BackupCorruptFile()
    {
        try { File.Copy(FilePath, FilePath + ".corrupt", true); }
        catch (Exception) { /* Không sao lưu được thì bỏ qua */ }
    }
}