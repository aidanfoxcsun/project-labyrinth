using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private float holdReloadTime = 2f; // Time in seconds to hold R for reload
    public Image fadeImage;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadScene());
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            StopAllCoroutines(); // Stop the reload coroutine if R is released
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f); // Reset fade
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(QuitToMenu());
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            StopAllCoroutines(); // Stop the quit coroutine if Escape is released
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f); // Reset fade
        }
    }

    private IEnumerator QuitToMenu()
    {
        float holdTime = 0f;
        while (holdTime < holdReloadTime)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, holdTime / holdReloadTime); // Fade in based on hold time
            holdTime += Time.deltaTime;
            yield return null;
        }
        // Load the main menu scene after the hold time
        AudioController.Instance.ReturnToMenu();
        SceneManager.LoadScene(0);
    }

    private IEnumerator ReloadScene()
    {
        float holdTime = 0f;

        while (holdTime < holdReloadTime)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, holdTime / holdReloadTime); // Fade in based on hold time
            holdTime += Time.deltaTime;
            yield return null;
        }

        // Reload the scene after the hold time
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
