using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour
{
    public static GameEndUI Instance;

    [Header("End Game Panels")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "StartMenu";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Hide both panels when gameplay starts.
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Show the Victory Panel.
    public void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Show the Game Over Panel.
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    // Restart the current level.
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Return to the Start Menu.
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}