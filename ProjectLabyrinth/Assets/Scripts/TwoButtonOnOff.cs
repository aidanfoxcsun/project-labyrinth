using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TwoButtonOnOff : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Button onButton;       // the ON button (has Image)
    public Button offButton;      // the OFF button (has Image)
    public Image onImage;         // the ON image to show
    public Image offImage;        // the OFF image to show

    [Header("Start State")]
    public bool isOn = false;

    [Header("Optional Events")]
    public UnityEvent onTurnOn;
    public UnityEvent onTurnOff;

    void Awake()
    {
        // Wire clicks
        onButton.onClick.AddListener(() => SetState(true));
        offButton.onClick.AddListener(() => SetState(false));
        Apply(isOn);
    }

    public void SetState(bool value)
    {
        if (isOn == value) return;
        isOn = value;
        Apply(isOn);
        if (isOn) onTurnOn?.Invoke(); else onTurnOff?.Invoke();
    }

    void Apply(bool value)
    {
        // Show/hide images
        if (onImage)  onImage.gameObject.SetActive(value);
        if (offImage) offImage.gameObject.SetActive(!value);

        // (Optional) disable the button that matches the current state
        if (onButton)  onButton.interactable = !value;  // when ON, disable ON button
        if (offButton) offButton.interactable = value;  // when ON, enable OFF button
    }
}
