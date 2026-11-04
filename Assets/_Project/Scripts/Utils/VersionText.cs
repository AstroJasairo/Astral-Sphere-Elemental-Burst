using TMPro;
using UnityEngine;

// Hiện số phiên bản ứng dụng (lấy từ Player Settings > Version)
[RequireComponent(typeof(TMP_Text))]
public class VersionText : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TMP_Text>().text = "v " + Application.version;
    }
}