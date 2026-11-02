using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("Saved Records")]

    // Existing Start Menu text for the high score.
    public TextMeshProUGUI highScoreText;

    // Existing Start Menu text for the best time.
    public TextMeshProUGUI bestTimeText;

    private void Start()
    {
        // Load saved records.
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", -1f);

        // Display the saved high score.
        if (highScoreText != null)
        {
            highScoreText.text =
                "HIGH SCORE: " + highScore.ToString("D6");
        }

        // Display the saved best time.
        if (bestTimeText != null)
        {
            if (bestTime < 0f)
            {
                // No completed level has been recorded yet.
                bestTimeText.text = "BEST TIME: --:--";
            }
            else
            {
                // Convert the time into minutes and seconds.
                int minutes = Mathf.FloorToInt(bestTime / 60f);
                int seconds = Mathf.FloorToInt(bestTime % 60f);

                bestTimeText.text =
                    "BEST TIME: " +
                    minutes.ToString("00") + ":" +
                    seconds.ToString("00");
            }
        }
    }
}