using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuRouter : MonoBehaviour
{
    [Header("Common Panels")]
    public GameObject mainPanel;              // your home screen
    public Image fadeOverlay;                 // optional; leave empty to skip fades
    public float fadeDuration = 0.2f;

    void Start()
    {
        // ensure we start at main if it's assigned
        if (mainPanel != null) ShowOnly(mainPanel);
        SetOverlayAlpha(0f);
    }

    // --- UNIVERSAL ENTRY POINT ---
    // Wire this to each Button's OnClick, and drag the specific panel into the argument slot.
    public void ShowPanel(GameObject targetPanel)
    {
        if (targetPanel == null) return;
        if (fadeOverlay == null) { ShowOnly(targetPanel); return; }
        StartCoroutine(FadeSwap(targetPanel));
    }

    // Optional helper for "Back" buttons
    public void ShowMain()
    {
        ShowPanel(mainPanel);
    }

    // ----- internals -----
    IEnumerator FadeSwap(GameObject target)
    {
        yield return Fade(0f, 1f, fadeDuration * 0.5f);
        ShowOnly(target);
        yield return Fade(1f, 0f, fadeDuration * 0.5f);
    }

    void ShowOnly(GameObject target)
    {
        // Hide every sibling under the same parent Canvas so only one is visible
        var canvas = target.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                // skip non-panels like Background / overlays by layer or by tag if you use them
                var go = child.gameObject;
                // Only toggle top-level panels; keep background/overlay if you want:
                if (go == fadeOverlay?.gameObject) continue; 
                if (go == target) { go.SetActive(true); continue; }
                // Hide only if it looks like a panel (has a RectTransform and is not Background)
                if (go.GetComponent<RectTransform>() != null && go != target)
                    go.SetActive(false);
            }
        }
        else
        {
            // Fallback: just show target
            target.SetActive(true);
        }
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        if (fadeOverlay == null) yield break;
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
        var c = fadeOverlay.color; c.a = a; fadeOverlay.color = c;
    }
}
