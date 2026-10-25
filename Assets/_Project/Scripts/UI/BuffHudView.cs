using UnityEngine;
using UnityEngine.UI;

public class BuffHudView : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [Tooltip("Thứ tự khớp enum BuffType: Speed, Rage, Magnet, ScoreBoost")]
    [SerializeField] private Image[] icons;

    private void Update()
    {
        BuffController buffs = stats.Buffs;

        for (int i = 0; i < icons.Length; i++)
        {
            float remaining = buffs.GetRemaining01((BuffType)i);
            bool active = remaining > 0f;

            // Chỉ bật/tắt khi trạng thái thay đổi, tránh gọi SetActive mỗi frame
            if (icons[i].gameObject.activeSelf != active)
                icons[i].gameObject.SetActive(active);

            if (active) icons[i].fillAmount = remaining;   // Vòng tròn cạn dần
        }
    }
}