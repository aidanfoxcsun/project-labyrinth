using UnityEngine;

public class FullscreenManager : MonoBehaviour
{
    [Header("Desired fullscreen mode when ON")]
    public FullScreenMode onMode = FullScreenMode.FullScreenWindow;

    [Header("Startup behavior")]
    public bool applyCurrentOnStart = true; // read Screen.fullScreen and apply mode on Start

    void Start()
    {
        if (applyCurrentOnStart)
        {
            // Normalize mode on startup (optional)
#if !UNITY_WEBGL
            if (Screen.fullScreen)
                Screen.fullScreenMode = onMode;
#endif
        }
    }

    // Hook this to UI -> OnTurnOn
    public void TurnOn()
    {
#if UNITY_WEBGL
        Screen.fullScreen = true;
#else
        Screen.fullScreenMode = onMode;
        Screen.fullScreen = true;
#endif
    }

    // Hook this to UI -> OnTurnOff
    public void TurnOff()
    {
        Screen.fullScreen = false;
#if !UNITY_WEBGL
        Screen.fullScreenMode = FullScreenMode.Windowed;
#endif
    }

    // Or hook UI -> OnStateChanged(bool) directly to this
    public void SetFullscreen(bool on)
    {
        if (on) TurnOn(); else TurnOff();
    }
}
