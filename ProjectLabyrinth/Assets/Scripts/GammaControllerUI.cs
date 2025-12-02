using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // If HDRP: use UnityEngine.Rendering.HighDefinition instead
using TMPro;

public class GammaControllerUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Post-Processing")]
    public Volume volume; // Assign your Global Volume that has a Color Adjustments override enabled.

    [Header("UI")]
    public Button gammaButton;       // Assign your UI Button
    public TMP_Text buttonText;          // Assign the Text on the button (or a child Text)

    [Header("Gamma (as percent)")]
    [Tooltip("Displayed and controlled value. 100 = neutral.")]
    public int gammaPercent = 100;
    public int minPercent = 10;
    public int maxPercent = 300;
    public int step = 5;             // step for keyboard and left-click
    public int rightClickStep = 5;   // step for right-click decrease

    [Header("Mouse Wheel")]
    public int wheelStep = 2;        // each wheel 'tick' changes this many percent

    [Header("Keys")]
    public KeyCode increaseKey1 = KeyCode.Equals;     // '=' key
    public KeyCode increaseKey2 = KeyCode.UpArrow;
    public KeyCode increaseKey3 = KeyCode.RightArrow;
    public KeyCode increaseKey4 = KeyCode.KeypadPlus;

    public KeyCode decreaseKey1 = KeyCode.Minus;      // '-' key
    public KeyCode decreaseKey2 = KeyCode.DownArrow;
    public KeyCode decreaseKey3 = KeyCode.LeftArrow;
    public KeyCode decreaseKey4 = KeyCode.KeypadMinus;

    public KeyCode resetKey = KeyCode.R;

    // internals
    private ColorAdjustments _colorAdj;

    void Awake()
    {
        if (!volume || !volume.profile || !volume.profile.TryGet(out _colorAdj))
        {
            Debug.LogWarning("GammaControllerUI: Missing Volume or Color Adjustments override. " +
                             "The value will still display but won't affect exposure.");
        }

        if (!gammaButton) Debug.LogWarning("GammaControllerUI: Assign the Button.");
        if (!buttonText) Debug.LogWarning("GammaControllerUI: Assign the Button's Text.");

        gammaPercent = Mathf.Clamp(gammaPercent, minPercent, maxPercent);
        ApplyToExposure();
        UpdateButtonLabel();
    }

    void Update()
    {
        // Keyboard increase
        if (Input.GetKeyDown(increaseKey1) || Input.GetKeyDown(increaseKey2) ||
            Input.GetKeyDown(increaseKey3) || Input.GetKeyDown(increaseKey4))
        {
            ChangeGamma(+step);
        }

        // Keyboard decrease
        if (Input.GetKeyDown(decreaseKey1) || Input.GetKeyDown(decreaseKey2) ||
            Input.GetKeyDown(decreaseKey3) || Input.GetKeyDown(decreaseKey4))
        {
            ChangeGamma(-step);
        }

        // Reset
        if (Input.GetKeyDown(resetKey))
        {
            SetGamma(100);
        }

        // Mouse wheel (up = increase, down = decrease)
        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.01f)
        {
            ChangeGamma((int)Mathf.Sign(wheel) * wheelStep);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Left-click increases, Right-click decreases, Middle-click resets
        if (eventData.button == PointerEventData.InputButton.Left)
            ChangeGamma(+step);
        else if (eventData.button == PointerEventData.InputButton.Right)
            ChangeGamma(-rightClickStep);
        else if (eventData.button == PointerEventData.InputButton.Middle)
            SetGamma(100);
    }

    // Optional: call from Button.onClick if you prefer (increments)
    public void OnButtonClicked()
    {
        ChangeGamma(+step);
    }

    private void ChangeGamma(int delta)
    {
        SetGamma(gammaPercent + delta);
    }

    private void SetGamma(int newPercent)
    {
        gammaPercent = Mathf.Clamp(newPercent, minPercent, maxPercent);
        ApplyToExposure();
        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        if (buttonText)
            buttonText.text = $"{gammaPercent}%";
    }

    private void ApplyToExposure()
    {
        if (_colorAdj == null) return;

        // Map percentage to exposure EV so 100% = 0 EV (neutral), doubling = +1 EV, halving = -1 EV
        float multiplier = Mathf.Max(1e-4f, gammaPercent / 100f);
        float ev = Mathf.Log(multiplier, 2f);
        _colorAdj.postExposure.value = ev;
    }
}
