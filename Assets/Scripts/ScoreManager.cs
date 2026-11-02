using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // Allows other scripts to access the ScoreManager.
    public static ScoreManager Instance;

    // Stores the monkey's current score.
    [Header("Score")]
    public int currentScore = 0;

    // Reference to the existing ScoreText on the HUD.
    [Header("HUD")]
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        // Prevent multiple ScoreManagers from existing.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set this object as the active ScoreManager.
        Instance = this;
    }

    void Start()
    {
        // Display the starting score when the game begins.
        UpdateScoreDisplay();
    }

    // Called whenever the monkey collects a scoring item.
    public void AddScore(int points)
    {
        // Increase the score by the specified amount.
        currentScore += points;

        // Update the score shown on the HUD.
        UpdateScoreDisplay();

        // Check whether the current score beats the saved high score.
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.CheckHighScore(currentScore);
        }

        // Print the score in the Console for testing.
        Debug.Log("Score: " + currentScore.ToString("D6"));
    }

    // Updates the existing ScoreText.
    void UpdateScoreDisplay()
    {
        // Only update the text if it has been assigned.
        if (scoreText != null)
        {
            // D6 displays the score using at least six digits.
            scoreText.text = "SCORE: " + currentScore.ToString("D6");
        }
    }
}