using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenuController : MonoBehaviour
{
    [Header("Panels (plain GameObjects)")]
    public GameObject startPanel;   // shown first
    public GameObject mainPanel;    // hidden first

    [Header("Optional Fade Overlay (full-screen Image on top of Canvas)")]
    public Image fadeOverlay;       // can be null; leave unassigned to skip fade
    public float fadeDuration = 0.35f;

    [Header("Built-in Transition (no extra scripts needed)")]
    public bool useAnimation = true;             // uncheck to just SetActive
    public float panelAnimDuration = 0.35f;
    public Vector2 hiddenOffset = new Vector2(0f, -80f); // slide from below
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // cached
    RectTransform startRT, mainRT;
    Vector2 startShownPos, mainShownPos;

    void Awake()
    {
        if (startPanel) startRT = startPanel.GetComponent<RectTransform>();
        if (mainPanel)  mainRT  = mainPanel.GetComponent<RectTransform>();

        if (startRT) startShownPos = startRT.anchoredPosition;
        if (mainRT)  mainShownPos  = mainRT.anchoredPosition;

        // Ensure initial visibility
        if (startPanel)
        {
            startPanel.SetActive(true);
            SetInstantState(startPanel, startRT, startShownPos, 1f);
        }

        if (mainPanel)
        {
            if (useAnimation)
            {
                // place main offscreen & invisible but active (so it can animate in later)
                mainPanel.SetActive(true);
                SetInstantState(mainPanel, mainRT, mainShownPos + hiddenOffset, 0f);
            }
            else
            {
                // simple mode: keep it inactive until we switch
                mainPanel.SetActive(false);
            }
        }

        if (fadeOverlay) SetOverlayAlpha(0f);
    }

    // Hook this in your Start button OnClick
    public void OnStartPressed()
    {
        if (!useAnimation)
        {
            // simple swap
            if (startPanel) startPanel.SetActive(false);
            if (mainPanel)  mainPanel.SetActive(true);
            return;
        }

        StartCoroutine(DoAnimatedSwap());
    }

    IEnumerator DoAnimatedSwap()
    {
        // optional screen fade to black (half)
        if (fadeOverlay) yield return FadeOverlay(0f, 1f, fadeDuration * 0.5f);

        // make sure both are active for animation
        if (startPanel && !startPanel.activeSelf) startPanel.SetActive(true);
        if (mainPanel  && !mainPanel.activeSelf)  mainPanel.SetActive(true);

        // animate panels simultaneously: start hides, main shows
        yield return AnimatePanels(
            startPanel, startRT, startShownPos, startShownPos + hiddenOffset, 1f, 0f,
            mainPanel,  mainRT,  mainShownPos + hiddenOffset, mainShownPos,    0f, 1f,
            panelAnimDuration, ease
        );

        // after anim, fully hide the start panel (inactive)
        if (startPanel) startPanel.SetActive(false);

        // fade back in
        if (fadeOverlay) yield return FadeOverlay(1f, 0f, fadeDuration * 0.5f);
    }

    // ---------- helpers ----------

    void SetInstantState(GameObject panel, RectTransform rt, Vector2 pos, float alpha)
    {
        if (!panel || !rt) return;

        rt.anchoredPosition = pos;

        var cg = panel.GetComponent<CanvasGroup>();
        if (!cg) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
        cg.blocksRaycasts = alpha > 0.99f;
        cg.interactable   = alpha > 0.99f;
    }

    IEnumerator AnimatePanels(
        GameObject outPanel, RectTransform outRT, Vector2 outFrom, Vector2 outTo, float outA0, float outA1,
        GameObject inPanel,  RectTransform inRT,  Vector2 inFrom,  Vector2 inTo,  float inA0,  float inA1,
        float duration, AnimationCurve curve
    )
    {
        // ensure CanvasGroups
        var outCG = EnsureCanvasGroup(outPanel);
        var inCG  = EnsureCanvasGroup(inPanel);

        // initial states
        if (outRT) outRT.anchoredPosition = outFrom;
        if (inRT)  inRT.anchoredPosition  = inFrom;

        outCG.alpha = outA0; outCG.blocksRaycasts = false; outCG.interactable = false;
        inCG.alpha  = inA0;  inCG.blocksRaycasts  = true;  inCG.interactable  = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // ignore timescale
            float k = Mathf.Clamp01(t / duration);
            float e = curve.Evaluate(k);

            if (outRT) outRT.anchoredPosition = Vector2.LerpUnclamped(outFrom, outTo, e);
            if (inRT)  inRT.anchoredPosition  = Vector2.LerpUnclamped(inFrom,  inTo,  e);

            outCG.alpha = Mathf.LerpUnclamped(outA0, outA1, e);
            inCG.alpha  = Mathf.LerpUnclamped(inA0,  inA1,  e);

            yield return null;
        }

        if (outRT) outRT.anchoredPosition = outTo;
        if (inRT)  inRT.anchoredPosition  = inTo;

        outCG.alpha = outA1; outCG.blocksRaycasts = false; outCG.interactable = false;
        inCG.alpha  = inA1;  inCG.blocksRaycasts  = true;  inCG.interactable  = true;
    }

    CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        if (!go) return null;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    IEnumerator FadeOverlay(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            SetOverlayAlpha(Mathf.Lerp(from, to, t / dur));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    void SetOverlayAlpha(float a)
    {
        if (!fadeOverlay) return;
        var c = fadeOverlay.color;
        c.a = a;
        fadeOverlay.color = c;
    }
}
