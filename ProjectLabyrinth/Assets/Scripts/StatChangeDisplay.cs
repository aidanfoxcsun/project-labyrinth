using System.Collections;
using UnityEngine;
using TMPro;

public class StatChangeDisplay : MonoBehaviour
{
    [Header("References")]
    public TMP_Text statValueText;       // shows the live stat value e.g. "2.00"
    public TMP_Text deltaText;           // the floating +/- label

    [Header("Colors (set in Inspector)")]
    public Color increaseColor = new Color(0.36f, 0.94f, 0.47f);
    public Color decreaseColor = new Color(0.94f, 0.36f, 0.36f);
    public Color neutralColor = Color.white;

    [Header("Animation")]
    public float displayDuration = 1.4f;    // how long delta stays visible
    public float floatDistance = 20f;     // how far it floats up (px)

    private Coroutine currentAnim;

    void Awake()
    {
        if (deltaText != null)
            deltaText.alpha = 0f;
    }

    // Call this whenever a stat changes
    public void ShowChange(float newValue, float delta)
    {
        if (statValueText != null)
            statValueText.text = newValue.ToString("F2");

        if (delta == 0f) return;

        bool isIncrease = delta > 0f;
        Color col = isIncrease ? increaseColor : decreaseColor;

        // Flash the value text briefly
        if (statValueText != null)
            statValueText.color = col;

        // Show the floating delta
        if (deltaText != null)
        {
            string sign = isIncrease ? "+" : "";
            deltaText.text = $"{sign}{delta:F2}";
            deltaText.color = col;

            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(AnimateDelta());
        }
    }

    private IEnumerator AnimateDelta()
    {
        RectTransform rt = deltaText.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatDistance;

        float elapsed = 0f;
        float fadeStart = displayDuration * 0.5f;

        while (elapsed < displayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / displayDuration;

            // Float upward
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // Fade out in second half
            float alpha = elapsed < fadeStart
                ? 1f
                : Mathf.Lerp(1f, 0f, (elapsed - fadeStart) / (displayDuration - fadeStart));

            deltaText.alpha = alpha;

            yield return null;
        }

        deltaText.alpha = 0f;
        rt.anchoredPosition = startPos;

        // Reset value text color
        if (statValueText != null)
            statValueText.color = neutralColor;
    }
}