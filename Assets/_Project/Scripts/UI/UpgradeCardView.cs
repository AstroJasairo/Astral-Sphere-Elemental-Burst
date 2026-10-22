using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Một thẻ lựa chọn nâng cấp
public class UpgradeCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    public void Setup(UpgradeOption option, Action<UpgradeOption> onClick)
    {
        titleText.text = option.Title;
        descriptionText.text = option.Description;
        iconImage.sprite = option.Icon;
        iconImage.enabled = option.Icon != null;   // Chưa có icon thì ẩn ô ảnh

        button.onClick.RemoveAllListeners();       // Xóa lựa chọn cũ của lần trước
        button.onClick.AddListener(() => onClick(option));
    }
}