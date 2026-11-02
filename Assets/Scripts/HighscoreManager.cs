using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    // Allows other scripts to access this manager.
    public static HighScoreManager Instance;

    // Stores the highest score achieved.
    private int highScore;

    // Stores the fastest completed level time.
    private float bestTime;

    private void Awake()
    {
        // Set this object as the manager.
        Instance = this;

        // Load the saved high score, or use 0 if none exists.
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Load the saved best time, or -1 if none exists.
        bestTime = PlayerPrefs.GetFloat("BestTime", -1f);
    }

    // Checks whether the player's score is a new record.
    public void CheckHighScore(int currentScore)
    {
        if (currentScore > highScore)
        {
            // Update the highest score.
            highScore = currentScore;

            // Save the new high score.
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();

            Debug.Log("New High Score: " + highScore);
        }
    }

    // Checks whether the player completed the level faster.
    public void CheckBestTime(float completionTime)
    {
        // Ignore invalid completion times.
        if (completionTime <= 0f)
        {
            return;
        }

        // Save the time if it is the first or fastest completion.
        if (bestTime < 0f || completionTime < bestTime)
        {
            bestTime = completionTime;

            // Save the new best time.
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();

            Debug.Log("New Best Time: " + bestTime);
        }
    }
}