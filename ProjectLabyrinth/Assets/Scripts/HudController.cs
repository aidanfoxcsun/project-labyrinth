using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Hearts (discrete, 0.5 per hit)")]
    [Tooltip("Order left-to-right. Each image must be ImageType=Filled(Horizontal, Left).")]
    public Image[] heartImages;

    [Min(1)] public int maxHearts = 5;     // number of heart icons, not halves
    [SerializeField] private float currentHearts; // can be 0, 0.5, 1.0, ... up to maxHearts

    [Header("Timer")]
    public TMP_Text timerText;
    public bool countDown = false;
    public int startTimeSeconds = 0;
    private float timeCounter;
    private bool timerRunning = true;

    void Awake()
    {
        // initialize health to full (if unset)
        if (currentHearts <= 0f) currentHearts = maxHearts;
        currentHearts = ClampToHalfStep(currentHearts, 0f, maxHearts);
        UpdateHeartsUI();

        timeCounter = countDown ? Mathf.Max(0, startTimeSeconds) : 0f;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!timerRunning) return;

        timeCounter += (countDown ? -1f : 1f) * Time.deltaTime;
        if (countDown && timeCounter <= 0f) { timeCounter = 0f; timerRunning = false; }
        UpdateTimerUI();
    }

    // =========================
    // Hearts API (0.5 step)
    // =========================
    public void DamageHalfHeart()  => AddHearts(-0.5f);
    public void HealHalfHeart()    => AddHearts(+0.5f);
    public void AddHearts(float delta)
    {
        SetHearts(currentHearts + delta);
    }

    public void SetHearts(float hearts)
    {
        currentHearts = ClampToHalfStep(hearts, 0f, maxHearts);
        UpdateHeartsUI();
    }

    public void SetMaxHearts(int newMax, bool fillToMax = true)
    {
        maxHearts = Mathf.Max(1, newMax);
        if (fillToMax) currentHearts = maxHearts;
        currentHearts = ClampToHalfStep(currentHearts, 0f, maxHearts);
        UpdateHeartsUI();
    }

    private void UpdateHeartsUI()
    {
        // Safety: ignore extras / missing
        int uiHearts = heartImages != null ? heartImages.Length : 0;
        for (int i = 0; i < uiHearts; i++)
        {
            float remaining = currentHearts - i; // how much of this heart is filled
            float fill = remaining >= 1f ? 1f :
                        (remaining >= 0.5f ? 0.5f : 0f);

            if (heartImages[i] != null)
                heartImages[i].fillAmount = Mathf.Clamp01(fill);
        }
    }

    private static float ClampToHalfStep(float v, float min, float max)
    {
        v = Mathf.Clamp(v, min, max);
        return Mathf.Round(v * 2f) / 2f; // snap to 0.5 steps
    }

    // =========================
    // Timer API
    // =========================
    public void StartTimer()  => timerRunning = true;
    public void PauseTimer()  => timerRunning = false;
    public void ResetTimer()
    {
        timeCounter = countDown ? Mathf.Max(0, startTimeSeconds) : 0f;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int total = Mathf.Max(0, Mathf.FloorToInt(timeCounter));
        int m = total / 60, s = total % 60;
        timerText.text = $"{m:00}:{s:00}";
    }
}
