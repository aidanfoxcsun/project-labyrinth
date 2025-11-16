using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuRouterSimple : MonoBehaviour
{
    [Header("Panels (plain GameObjects)")]
    public GameObject startPanel;       // first screen (optional)
    public GameObject mainPanel;        // home
    public GameObject newGamePanel;
    public GameObject continuePanel;
    public GameObject challengesPanel;
    public GameObject statsPanel;
    public GameObject optionsPanel;
    public GameObject confirmExitPanel;

    [Header("Fade Settings")]
    [Tooltip("Seconds for cross-fade between panels.")]
    public float fadeDuration = 0.25f;
    [Range(0f,1f)] public float outAlpha = 0f;  // where old panel finishes
    [Range(0f,1f)] public float inAlpha  = 1f;  // where new panel finishes

    [Header("Optional Overlay (full-screen Image)")]
    public Image overlay;               // leave null to skip overlay
    public float overlayFade = 0.18f;
    public float overlayHold = 0.04f;

    [Header("Continue Button (gating)")]
    public Button continueButton;
    public string continueKey = "Save_Slot_0";
    public Color disabledTextColor = new Color(0.65f,0.65f,0.65f,1f);
    public Color enabledTextColor  = Color.white;

    [Header("Start Input")]
    public Button startButton;                 // optional OnClick -> OnStartPressed
    public bool allowAnyKeyToStart = true;
    public KeyCode extraGamepadStart = KeyCode.JoystickButton7;

    // ---- internal ----
    GameObject current;
    bool menuInitialized = false;
    Coroutine swapCo;
    int swapToken = 0; // cancels older fades if user clicks quickly

    // =================== Lifecycle ===================
    void Start()
    {
        // Ensure every panel has a CanvasGroup and is fully reset.
        foreach (var p in All())
        {
            if (!p) continue;
            var cg = EnsureCanvasGroup(p);
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable   = false;
            p.SetActive(false);
        }
        if (overlay) SetOverlayAlpha(0f);

        if (startPanel)
        {
            // Show start panel only
            var cg = EnsureCanvasGroup(startPanel);
            startPanel.SetActive(true);
            cg.alpha = inAlpha;
            cg.blocksRaycasts = true;
            cg.interactable   = true;
            current = startPanel;
            SelectFirst(startPanel);
        }
        else
        {
            InitMainMenu();
        }
    }

    void Update()
    {
        if (!allowAnyKeyToStart || menuInitialized) return;
        if (!startPanel || current != startPanel) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetKeyDown(extraGamepadStart))
            OnStartPressed();
    }

    // =================== Start -> Main ===================
    public void OnStartPressed()
    {
        if (menuInitialized) return;

        if (!startPanel)
        {
            InitMainMenu();
            return;
        }

        // fade start to main
        FadeTo(mainPanel);
        StartCoroutine(FinishInitAfterNextSwap());
    }

    IEnumerator FinishInitAfterNextSwap()
    {
        // wait until the current fade completes
        int tokenAtStart = swapToken;
        yield return new WaitUntil(() => swapCo == null || tokenAtStart != swapToken);
        InitMainFinalize();
    }

    void InitMainMenu()
    {
        // No start screen; show main immediately
        ForceShowOnly(mainPanel);
        InitMainFinalize();
    }

    void InitMainFinalize()
    {
        menuInitialized = true;
        UpdateContinueState();
        SelectFirst(mainPanel);
    }

    // =================== Button hooks ===================
    public void OnNewGame()      { if (!menuInitialized) return; FadeTo(newGamePanel); }
    public void OnContinue()     { if (!menuInitialized) return; if (HasSave()) FadeTo(continuePanel); }
    public void OnChallenges()   { if (!menuInitialized) return; FadeTo(challengesPanel); }
    public void OnStats()        { if (!menuInitialized) return; FadeTo(statsPanel); }
    public void OnOptions()      { if (!menuInitialized) return; FadeTo(optionsPanel); }
    public void OnExit()         { if (!menuInitialized) return; if (confirmExitPanel) FadeTo(confirmExitPanel); else QuitApp(); }
    public void OnConfirmExitYes(){ if (!menuInitialized) return; QuitApp(); }
    public void OnConfirmExitNo() { if (!menuInitialized) return; FadeTo(mainPanel); }
    public void OnBackToMain()   { if (!menuInitialized) return; FadeTo(mainPanel); UpdateContinueState(); }

    // =================== Core fade routing ===================
    public void FadeTo(GameObject target)
    {
        if (!target) return;
        if (current == target && target.activeInHierarchy) return;

        // Cancel any in-flight swap and start a fresh one.
        if (swapCo != null) StopCoroutine(swapCo);
        swapToken++;
        swapCo = StartCoroutine(CrossFadeSwap(current, target, swapToken));
    }

    IEnumerator CrossFadeSwap(GameObject from, GameObject to, int token)
    {
        // Defensive: make sure both CGs exist
        var toCG = EnsureCanvasGroup(to);
        CanvasGroup fromCG = null;
        if (from) fromCG = EnsureCanvasGroup(from);

        // Bring overlay up first (optional)
        if (overlay) yield return FadeOverlay(0f, 1f, overlayFade);

        // Prepare target: active, alpha 0, input on
        to.SetActive(true);
        toCG.blocksRaycasts = true;
        toCG.interactable   = true;
        toCG.alpha          = Mathf.Min(toCG.alpha, 0f); // ensure starts at 0

        // Ensure outgoing panel starts at its current (usually 1), and will end at outAlpha
        float fromA0 = (fromCG != null) ? fromCG.alpha : 0f;
        float toA0   = toCG.alpha;

        float t = 0f;
        while (t < fadeDuration)
        {
            // If another swap started, abort this one.
            if (token != swapToken) yield break;

            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);

            if (fromCG) fromCG.alpha = Mathf.Lerp(fromA0, outAlpha, k);
            toCG.alpha = Mathf.Lerp(toA0,   inAlpha,  k);

            yield return null;
        }

        // Snap to final
        if (fromCG)
        {
            fromCG.alpha = outAlpha;
            fromCG.blocksRaycasts = false;
            fromCG.interactable   = false;
        }
        if (from) from.SetActive(false); // <<<<< HARD DEACTIVATE PREVIOUS PANEL

        toCG.alpha = inAlpha;

        // Deactivate *all other panels* to prevent leftovers sticking around
        DeactivateOthers(to);

        // Overlay down (optional)
        if (overlay)
        {
            if (overlayHold > 0f) yield return new WaitForSecondsRealtime(overlayHold);
            yield return FadeOverlay(1f, 0f, overlayFade);
        }

        current = to;
        SelectFirst(current);
        swapCo = null;
    }

    // Deactivate everything except the given panel (extra safety)
    void DeactivateOthers(GameObject keep)
    {
        foreach (var p in All())
        {
            if (!p) continue;
            if (p == keep) { p.SetActive(true); continue; }

            var cg = EnsureCanvasGroup(p);
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable   = false;
            p.SetActive(false);
        }
    }

    // Immediate show only one (used during init)
    void ForceShowOnly(GameObject show)
    {
        foreach (var p in All())
        {
            if (!p) continue;
            var cg = EnsureCanvasGroup(p);
            bool on = (p == show);
            p.SetActive(on);
            cg.alpha = on ? inAlpha : 0f;
            cg.blocksRaycasts = on;
            cg.interactable   = on;
        }
        current = show;
    }

    // =================== Continue gating ===================
    void UpdateContinueState()
    {
        if (!continueButton) return;

        bool has = HasSave();

        continueButton.interactable = has;

        var nav = continueButton.navigation;
        nav.mode = has ? Navigation.Mode.Automatic : Navigation.Mode.None;
        continueButton.navigation = nav;

        var label = continueButton.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (label) label.color = has ? enabledTextColor : disabledTextColor;

        var colors = continueButton.colors;
        colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);
        continueButton.colors = colors;

        if (!has && EventSystem.current &&
            EventSystem.current.currentSelectedGameObject == continueButton.gameObject)
        {
            SelectFirst(mainPanel);
        }
    }

    bool HasSave() => PlayerPrefs.HasKey(continueKey);

    // =================== Helpers ===================
    CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    GameObject[] All() => new[]
    {
        startPanel, mainPanel, newGamePanel, continuePanel,
        challengesPanel, statsPanel, optionsPanel, confirmExitPanel
    };

    void SelectFirst(GameObject panel)
    {
        if (!panel || EventSystem.current == null) return;
        var first = panel.GetComponentInChildren<Button>(true);
        if (!first) return;
        StartCoroutine(SelectNextFrame(first));
    }

    IEnumerator SelectNextFrame(Button b)
    {
        yield return null;
        if (b) EventSystem.current.SetSelectedGameObject(b.gameObject);
    }

    IEnumerator FadeOverlay(float from, float to, float dur)
    {
        float t = 0f;
        SetOverlayAlpha(from);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            SetOverlayAlpha(Mathf.Lerp(from, to, k));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    void SetOverlayAlpha(float a)
    {
        if (!overlay) return;
        var c = overlay.color; c.a = a; overlay.color = c;
    }

    void QuitApp()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
