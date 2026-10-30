using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hộp thoại xếp hạng một map. Dùng chung cho màn Map và màn chọn nhân vật.
// Script nằm trên một object LUÔN BẬT; "panel" là object con (mặc định tắt).
public class LeaderboardPopup : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject emptyText;             // "Chưa có điểm"
    [SerializeField] private Transform rowsParent;
    [SerializeField] private LeaderboardRowView rowPrefab;
    [SerializeField] private Button closeButton;

    private readonly List<LeaderboardRowView> rows = new List<LeaderboardRowView>();

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        // Tạo sẵn đủ số dòng của bảng xếp hạng một lần, sau đó chỉ bật/tắt và đổi chữ
        for (int i = 0; i < MetaProgress.MaxScores; i++)
            rows.Add(Instantiate(rowPrefab, rowsParent));

        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    public void Show(MapData map)
    {
        if (map == null) return;

        titleText.text = "Xếp hạng: " + map.displayName;
        List<ScoreEntry> top = MetaProgress.GetTopScores(map.id);

        for (int i = 0; i < rows.Count; i++)
        {
            if (i < top.Count) rows[i].Set(i + 1, top[i]);
            else rows[i].Hide();
        }

        emptyText.SetActive(top.Count == 0);
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }
}