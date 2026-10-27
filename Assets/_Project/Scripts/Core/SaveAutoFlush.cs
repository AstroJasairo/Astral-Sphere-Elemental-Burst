using UnityEngine;

// Gắn vào một object trong cả scene Home và Game
public class SaveAutoFlush : MonoBehaviour
{
    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Save();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}