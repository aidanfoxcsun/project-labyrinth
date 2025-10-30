// AntiAliasingController_BuiltIn.cs
using UnityEngine;

public class AntiAliasingController_BuiltIn : MonoBehaviour
{
    [Header("MSAA sample count when ON (valid: 2,4,8)")]
    [Range(0,8)] public int msaaSamples = 4;

    [Header("Cameras to update (leave empty to use all active cameras)")]
    public Camera[] targetCameras;

    // Hook this to your TwoButtonOnOffUI -> OnStateChanged(bool)
    public void SetAA(bool on)
    {
        QualitySettings.antiAliasing = on ? NormalizeSamples(msaaSamples) : 0;

        var cams = (targetCameras != null && targetCameras.Length > 0)
            ? targetCameras
            : Camera.allCameras; // avoids deprecated API

        for (int i = 0; i < cams.Length; i++)
        {
            var cam = cams[i];
            if (!cam) continue;
            cam.allowMSAA = on;
        }
    }

    public void TurnOn()  => SetAA(true);
    public void TurnOff() => SetAA(false);

    int NormalizeSamples(int s) => (s >= 8) ? 8 : (s >= 4) ? 4 : (s >= 2) ? 2 : 0;
}
