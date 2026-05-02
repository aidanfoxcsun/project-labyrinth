using System.Collections;
using UnityEngine;
using TMPro;

public class StatRow : MonoBehaviour
{
    [Header("References")]
    public TMP_Text valueText;
    public TMP_Text deltaText;

    [Header("Colors")]
    public Color increaseColor = new Color(0.36f, 0.94f, 0.47f);
    public Color decreaseColor = new Color(0.94f, 0.36f, 0.36f);
    public Color normalColor = new Color(0.78f, 0.72f, 0.85f);

    [Header("Animation")]
    public float duration = 1.4f;
    public float floatPixels = 18f;

    private Coroutine anim;
    private Coroutine resetCol;

    void Awake()
    {
        HideDelta();
    }

    public void Show(float newValue, float delta)
    {
        Debug.Log($"[StatRow] {gameObject.name} Show called — value:{newValue} delta:{delta}");
        if (valueText != null)
            valueText.text = newValue.ToString("F2");

        if (delta == 0f) return;

        Color col = delta > 0f ? increaseColor : decreaseColor;
        string sign = delta > 0f ? "+" : "";

        if (valueText != null)
            valueText.color = col;

        if (deltaText != null)
        {
            deltaText.text = $"{sign}{delta:F2}";

            // Stop any in-progress animations before restarting
            if (anim != null) { StopCoroutine(anim); anim = null; }
            if (resetCol != null) { StopCoroutine(resetCol); resetCol = null; }

            anim = StartCoroutine(Animate(col));
            resetCol = StartCoroutine(ResetValueColor());
        }
    }

    private void HideDelta()
    {
        if (deltaText == null) return;
        SetDeltaAlpha(0f);
        deltaText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    // Use color alpha instead of .alpha — works regardless of CanvasGroup
    private void SetDeltaAlpha(float a)
    {
        if (deltaText == null) return;
        Color c = deltaText.color;
        c.a = a;
        deltaText.color = c;
    }

    private IEnumerator Animate(Color col)
    {
        RectTransform rt = deltaText.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        float fadeIn = duration * 0.15f;
        float hold = duration * 0.35f;
        float fadeOut = duration * 0.50f;
        float t;

        // Fade in
        t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(t / fadeIn);
            Color c = col; c.a = alpha; deltaText.color = c;
            rt.anchoredPosition = Vector2.up * Mathf.Lerp(0f, floatPixels, t / duration);
            yield return null;
        }

        // Hold
        t = 0f;
        while (t < hold)
        {
            t += Time.unscaledDeltaTime;
            rt.anchoredPosition = Vector2.up * Mathf.Lerp(0f, floatPixels, (fadeIn + t) / duration);
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (t / fadeOut));
            Color c = col; c.a = alpha; deltaText.color = c;
            rt.anchoredPosition = Vector2.up * Mathf.Lerp(0f, floatPixels, (fadeIn + hold + t) / duration);
            yield return null;
        }

        HideDelta();
        anim = null;
    }

    private IEnumerator ResetValueColor()
    {
        yield return new WaitForSecondsRealtime(1.1f);
        if (valueText != null) valueText.color = normalColor;
        resetCol = null;
    }
}