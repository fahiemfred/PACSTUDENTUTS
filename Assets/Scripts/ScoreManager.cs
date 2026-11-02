using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Allows other scripts to access the ScoreManager.
    public static ScoreManager Instance;

    // Stores the player's current score.
    [Header("Score")]
    public int currentScore = 0;

    void Awake()
    {
        // Prevent multiple ScoreManagers from existing.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register this object as the active ScoreManager.
        Instance = this;
    }

    // Adds points to the player's current score.
    public void AddScore(int points)
    {
        currentScore += points;

        // Display the score using six digits.
        Debug.Log("Score: " + currentScore.ToString("D6"));
    }
}