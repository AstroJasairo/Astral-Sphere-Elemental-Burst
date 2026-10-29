using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;   // Tùy chọn
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text costText;          // Chữ nằm trên nút
    [SerializeField] private Image icon;                 // Tùy chọn
    [SerializeField] private Button button;

    // Gắn hành động bấm nút (gọi một lần)
    public void Init(Action onClick)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }

    // cost < 0 nghĩa là đã tối đa
    public void Set(string title, string description, string level, string value, int cost, int wallet, Sprite iconSprite = null)
    {
        nameText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        levelText.text = level;
        valueText.text = value;

        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
        }

        bool maxed = cost < 0;
        costText.text = maxed ? "TỐI ĐA" : cost.ToString();
        button.interactable = !maxed && wallet >= cost;      // Khóa nút khi đã tối đa hoặc không đủ vàng
    }
}