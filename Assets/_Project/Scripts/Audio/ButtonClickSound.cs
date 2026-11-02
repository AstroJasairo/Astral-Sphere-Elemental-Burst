using UnityEngine;
using UnityEngine.UI;

// Gắn vào một Button để phát âm bấm nút
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }

    private static void Play()
    {
        AudioLibrary lib = AudioManager.Library;
        if (lib != null) AudioManager.Play(lib.buttonClick);
    }
}