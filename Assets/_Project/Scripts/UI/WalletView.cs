using TMPro;
using UnityEngine;

// Hiển thị số vàng trong ví, tự cập nhật. Gắn vào bất kỳ text nào cần hiện vàng.
public class WalletView : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        goldText.text = MetaProgress.Gold.ToString();
    }
}