using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Overlay")]
    public Image fadeOverlay; // full-screen black Image
    public float fadeDuration = 0.4f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (fadeOverlay != null) SetAlpha(0f);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeLoad(sceneName));
    }

    IEnumerator FadeLoad(string scene)
    {
        yield return Fade(1f); // fade to black
        AsyncOperation op = SceneManager.LoadSceneAsync(scene);
        while (!op.isDone) yield return null;
        yield return Fade(0f); // fade in
    }

    IEnumerator Fade(float target)
    {
        if (fadeOverlay == null) yield break;
        float start = fadeOverlay.color.a;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = curve.Evaluate(Mathf.Clamp01(t / fadeDuration));
            SetAlpha(Mathf.Lerp(start, target, k));
            yield return null;
        }
        SetAlpha(target);
    }

    void SetAlpha(float a)
    {
        var c = fadeOverlay.color;
        c.a = a;
        fadeOverlay.color = c;
    }
}
