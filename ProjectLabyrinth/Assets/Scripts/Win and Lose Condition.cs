using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance;

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    // Call this when the boss is defeated
    public void Win()
    {
        if (gameEnded) return;
        gameEnded = true;

        Time.timeScale = 0f;
        if (winPanel) winPanel.SetActive(true);

        Debug.Log("YOU WIN!");
    }

    // Call this when the player reaches 0 HP
    public void Lose()
    {
        if (gameEnded) return;
        gameEnded = true;

        Time.timeScale = 0f;
        if (losePanel) losePanel.SetActive(true);

        Debug.Log("YOU LOST!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
