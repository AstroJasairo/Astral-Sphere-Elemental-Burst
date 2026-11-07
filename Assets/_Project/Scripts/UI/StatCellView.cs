using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Một ô của StatGrid: icon bên trái + chữ bên phải.
public class StatCellView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    public void Set(Sprite sprite, string text)
    {
        label.text = text;
        icon.sprite = sprite;
        icon.enabled = sprite != null;     // không có icon thì ẩn, tránh ô trắng
    }
}