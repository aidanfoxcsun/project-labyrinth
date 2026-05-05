using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Single-script main menu controller.
/// Handles: Start Screen → Main Menu → sub-panels, and Start Game → next scene.
///
/// SETUP
/// -----
/// 1. Attach this script to a persistent GameObject in your menu scene.
/// 2. Assign panels in the Inspector. Only assign panels you actually use.
/// 3. Set nextSceneName to the exact Build-Settings scene name you want to load.
/// 4. Wire buttons:
///      "Any Key / Start" button  → OnStartPressed()
///      "New Game"                → OnNewGame()
///      "Options"                 → OnOptions()
///      "Quit"                    → OnQuit()
///      "Back" buttons            → OnBackToMain()
///      "Confirm Quit: Yes"       → OnConfirmQuitYes()
///      "Confirm Quit: No"        → OnBackToMain()
/// 5. Optional: assign fadeOverlay (full-screen black Image, sits on top of Canvas).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    // ── Scene to load when the player starts a new game ──────────────────────
    [Header("Scene")]
    public string nextSceneName = "Game"; // must match Build Settings name exactly

    // ── Panels ───────────────────────────────────────────────────────────────
    [Header("Panels")]
    [Tooltip("The very first 'Press Any Key' screen. Leave null to skip it.")]
    public GameObject startPanel;
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject confirmQuitPanel;
    // Add more sub-panels here as needed and call FadeTo() from their buttons.

    // ── Transition ───────────────────────────────────────────────────────────
    [Header("Transition")]
    [Tooltip("Full-screen black Image that lives above all panels in the Canvas. Optional.")]
    public Image fadeOverlay;
    [Tooltip("Seconds for a panel cross-fade.")]
    public float fadeDuration = 0.22f;
    [Tooltip("Seconds for the overlay fade (used on Start → Main only).")]
    public float overlayDuration = 0.30f;

    // ── Any-Key-to-Start ─────────────────────────────────────────────────────
    [Header("Any-Key Start")]
    public bool allowAnyKey = true;

    // ── Internal state ───────────────────────────────────────────────────────
    GameObject _current;
    bool _atStartScreen;   // true while the pre-menu start screen is shown
    bool _busy;            // guard: ignore input while a transition plays
    Coroutine _fadeCo;

    // ═════════════════════════════════════════════════════════════════════════
    //  Lifecycle
    // ═════════════════════════════════════════════════════════════════════════

    void Awake()
    {
        // Hard-reset every panel so nothing bleeds through from edit-mode state.
        foreach (var p in AllPanels())
            HideInstant(p);

        if (fadeOverlay)
            SetOverlayAlpha(0f);

        if (startPanel)
        {
            ShowInstant(startPanel);
            _current = startPanel;
            _atStartScreen = true;
        }
        else
        {
            ShowInstant(mainPanel);
            _current = mainPanel;
            _atStartScreen = false;
        }
    }

    void Update()
    {
        // Any key/click advances from the start screen to the main menu.
        if (!_atStartScreen || _busy || !allowAnyKey) return;
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            OnStartPressed();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Public button hooks  (wire these in the Inspector OnClick)
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>"Press Any Key" screen → Main Menu.</summary>
    public void OnStartPressed()
    {
        if (_busy || !_atStartScreen) return;
        _atStartScreen = false;
        StartCoroutine(StartScreenToMain());
    }

    /// <summary>Main Menu → load next scene immediately.</summary>
    public void OnNewGame()
    {
        if (_busy) return;
        StartCoroutine(LoadScene());
    }

    public void OnOptions() => FadeTo(optionsPanel);
    public void OnQuit() => FadeTo(confirmQuitPanel);
    public void OnBackToMain() => FadeTo(mainPanel);
    public void OnConfirmQuitYes() { if (!_busy) QuitApp(); }

    // ═════════════════════════════════════════════════════════════════════════
    //  Core transitions
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>Cross-fade from the current panel to <paramref name="target"/>.</summary>
    public void FadeTo(GameObject target)
    {
        if (!target || _busy || _current == target) return;
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(CrossFade(_current, target));
    }

    IEnumerator CrossFade(GameObject from, GameObject to)
    {
        _busy = true;

        // Grab / create CanvasGroups.
        var fromCG = EnsureCG(from);
        var toCG = EnsureCG(to);

        // Make target visible but input-blocked while fading in.
        to.SetActive(true);
        toCG.alpha = 0f;
        toCG.blocksRaycasts = false;
        toCG.interactable = false;

        // Also disable input on the outgoing panel immediately.
        if (fromCG != null)
        {
            fromCG.blocksRaycasts = false;
            fromCG.interactable = false;
        }

        float elapsed = 0f;
        float fromStart = fromCG != null ? fromCG.alpha : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / fadeDuration);

            if (fromCG != null) fromCG.alpha = Mathf.Lerp(fromStart, 0f, k);
            toCG.alpha = Mathf.Lerp(0f, 1f, k);

            yield return null;
        }

        // Snap & clean up outgoing panel.
        if (fromCG != null) fromCG.alpha = 0f;
        if (from)
        {
            from.SetActive(false);
        }

        // Snap incoming panel to fully visible & interactive.
        toCG.alpha = 1f;
        toCG.blocksRaycasts = true;
        toCG.interactable = true;

        _current = to;
        _busy = false;
        AutoSelectFirst(to);
        _fadeCo = null;
    }

    /// <summary>Special transition: start screen fades out with overlay, then main fades in.</summary>
    IEnumerator StartScreenToMain()
    {
        _busy = true;

        var fromCG = EnsureCG(startPanel);
        fromCG.blocksRaycasts = false;
        fromCG.interactable = false;

        // Fade overlay in.
        if (fadeOverlay)
            yield return FadeOverlay(0f, 1f, overlayDuration * 0.5f);
        else
            yield return FadeOut(fromCG, overlayDuration);

        startPanel.SetActive(false);

        // Prepare main panel.
        mainPanel.SetActive(true);
        var mainCG = EnsureCG(mainPanel);
        mainCG.alpha = 0f;
        mainCG.blocksRaycasts = false;
        mainCG.interactable = false;

        // Fade overlay out (revealing main).
        if (fadeOverlay)
        {
            yield return FadeOverlay(1f, 0f, overlayDuration * 0.5f);
            mainCG.alpha = 1f;
        }
        else
        {
            yield return FadeIn(mainCG, overlayDuration);
        }

        mainCG.blocksRaycasts = true;
        mainCG.interactable = true;

        _current = mainPanel;
        _busy = false;
        AutoSelectFirst(mainPanel);
    }

    /// <summary>Fade overlay then load the next scene.</summary>
    IEnumerator LoadScene()
    {
        _busy = true;

        if (fadeOverlay)
            yield return FadeOverlay(0f, 1f, overlayDuration);

        SceneManager.LoadScene(nextSceneName);
        // _busy intentionally left true — scene is changing.
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═════════════════════════════════════════════════════════════════════════

    void ShowInstant(GameObject p)
    {
        if (!p) return;
        p.SetActive(true);
        var cg = EnsureCG(p);
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    void HideInstant(GameObject p)
    {
        if (!p) return;
        var cg = EnsureCG(p);
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        p.SetActive(false);
    }

    CanvasGroup EnsureCG(GameObject go)
    {
        if (!go) return null;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    IEnumerator FadeOut(CanvasGroup cg, float dur)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < dur) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Lerp(start, 0f, t / dur); yield return null; }
        cg.alpha = 0f;
    }

    IEnumerator FadeIn(CanvasGroup cg, float dur)
    {
        float t = 0f;
        while (t < dur) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Lerp(0f, 1f, t / dur); yield return null; }
        cg.alpha = 1f;
    }

    IEnumerator FadeOverlay(float from, float to, float dur)
    {
        SetOverlayAlpha(from);
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            SetOverlayAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / dur)));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    void SetOverlayAlpha(float a)
    {
        if (!fadeOverlay) return;
        var c = fadeOverlay.color; c.a = a; fadeOverlay.color = c;
    }

    void AutoSelectFirst(GameObject panel)
    {
        if (!panel || EventSystem.current == null) return;
        var btn = panel.GetComponentInChildren<Button>(true);
        if (btn) StartCoroutine(SelectNextFrame(btn));
    }

    IEnumerator SelectNextFrame(Button b)
    {
        yield return null; // wait one frame so layout is settled
        if (b && EventSystem.current) EventSystem.current.SetSelectedGameObject(b.gameObject);
    }

    /// <summary>All panels for bulk init. Extend this if you add more.</summary>
    GameObject[] AllPanels() => new[] { startPanel, mainPanel, optionsPanel, confirmQuitPanel };

    void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}