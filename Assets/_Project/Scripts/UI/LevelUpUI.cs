using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;              // Panel (mặc định TẮT)
    [SerializeField] private UpgradeCardView[] cards;       // 3 thẻ

    public void Show(List<UpgradeOption> options, Action<UpgradeOption> onChosen)
    {
        panel.SetActive(true);

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasOption = i < options.Count;
            cards[i].gameObject.SetActive(hasOption);   // Ít hơn 3 lựa chọn thì ẩn thẻ thừa
            if (!hasOption) continue;

            cards[i].Setup(options[i], chosen =>
            {
                panel.SetActive(false);   // Ẩn trước, rồi mới báo kết quả
                onChosen(chosen);
            });
        }
    }
}