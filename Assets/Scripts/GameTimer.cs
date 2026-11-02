using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    // Reference to the existing timer text on the HUD.
    [Header("HUD")]
    public TextMeshProUGUI gameTimerText;

    // Stores the total elapsed gameplay time in seconds.
    [Header("Timer")]
    public float elapsedTime = 0f;

    // Determines whether the timer is currently running.
    private bool isRunning = true;

    void Start()
    {
        // Begin the game with the timer at zero.
        elapsedTime = 0f;

        // Start counting immediately.
        isRunning = true;

        // Update the HUD to display the starting time.
        UpdateTimerDisplay();
    }

    void Update()
    {
        // Only increase the timer while gameplay is active.
        if (isRunning)
        {
            // Add the time passed since the previous frame.
            elapsedTime += Time.deltaTime;

            // Refresh the timer displayed on the HUD.
            UpdateTimerDisplay();
        }
    }

    // Converts elapsed seconds into minutes, seconds and centiseconds.
    void UpdateTimerDisplay()
    {
        // Do nothing if the HUD text has not been assigned.
        if (gameTimerText == null)
        {
            return;
        }

        // Calculate the number of complete minutes.
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);

        // Calculate the remaining seconds.
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        // Calculate centiseconds (hundredths of a second).
        int centiseconds =
            Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        // Display the time in MM:SS:CS format.
        gameTimerText.text =
            minutes.ToString("D2") + ":" +
            seconds.ToString("D2") + ":" +
            centiseconds.ToString("D2");
    }

    // Pauses the timer without resetting its elapsed time.
    public void PauseTimer()
    {
        isRunning = false;
    }

    // Resumes counting from the previously recorded time.
    public void ResumeTimer()
    {
        isRunning = true;
    }

    // Stops the timer and resets it to zero.
    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;

        UpdateTimerDisplay();
    }

    // Stops the timer until resumed or reset.
    public void StopTimer()
    {
        isRunning = false;
    }

    // Returns the current elapsed gameplay time in seconds.
    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    // Called when the monkey successfully completes the level.
    public void CompleteLevel()
    {
        // Stop counting when the level is completed.
        StopTimer();

        // Check whether the completion time is a new record.
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.CheckBestTime(elapsedTime);
        }

        // Print the completion time for testing.
        Debug.Log("Level completed in " + elapsedTime + " seconds!");
    }
}