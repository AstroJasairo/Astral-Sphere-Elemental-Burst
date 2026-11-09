using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hộp xác nhận nâng cấp. Gọi từ bất kỳ đâu: ConfirmDialog.Ask(tiêu đề, nội dung, giá, hành động khi đồng ý)
public class ConfirmDialog : MonoBehaviour
{
    private static ConfirmDialog instance;

    [SerializeField] private GameObject panel;               // Khung + lớp tối (mặc định TẮT; object chứa script này luôn BẬT)
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action pending;

    public static bool IsOpen => instance != null && instance.panel.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; }

    private void Awake()
    {
        instance = this;
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void Ask(string title, string message, int cost, Action onConfirm)
    {
        if (instance == null)                                // Chưa dựng hộp trong scene: nâng thẳng để game vẫn dùng được
        {
            Debug.LogWarning("ConfirmDialog: chưa có trong scene, nâng cấp thực hiện ngay.");
            onConfirm?.Invoke();
            return;
        }
        instance.Open(title, message, cost, onConfirm);
    }

    // Nút Back của HomeUI gọi: đóng hộp nếu đang mở
    public static void Dismiss()
    {
        if (instance != null) instance.Close();
    }

    private void Open(string title, string message, int cost, Action onConfirm)
    {
        titleText.text = title;
        messageText.text = message;
        costText.text = cost >= 0 ? $"Giá: {cost} vàng" : "";
        pending = onConfirm;
        panel.SetActive(true);
    }

    private void Confirm()
    {
        Action action = pending;
        Close();                                             // Đóng trước, rồi mới thực hiện
        action?.Invoke();
    }

    private void Close()
    {
        pending = null;
        panel.SetActive(false);
    }
}