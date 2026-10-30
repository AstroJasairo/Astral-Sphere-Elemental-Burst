using TMPro;
using UnityEngine;

public class LeaderboardRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text detailText;

    public void Set(int rank, ScoreEntry e)
    {
        gameObject.SetActive(true);
        rankText.text = rank.ToString();
        scoreText.text = e.score.ToString();
        detailText.text = $"{e.timeSeconds / 60:00}:{e.timeSeconds % 60:00}   {(e.victory ? "Thắng" : "Thua")}   {e.date}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}