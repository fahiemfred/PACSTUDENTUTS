using UnityEngine;
using TMPro;
using System.Collections;

public class GameCountdown : MonoBehaviour
{
    [Header("Countdown UI")]

    // Text that displays 3, 2, 1, GO!
    public TextMeshProUGUI countdownText;

    // Small background panel behind the countdown text.
    public GameObject countdownBackground;

    [Header("Player")]

    // Reference to the monkey's movement script.
    public MonkeyStudentController monkey;

    [Header("Cats")]

    // References to the four cats' movement scripts.
    public CatMovement[] cats;

    [Header("Game Timer")]

    // Reference to the existing game timer.
    public GameTimer gameTimer;

    private void Start()
    {
        // Show the countdown background.
        if (countdownBackground != null)
        {
            countdownBackground.SetActive(true);
        }

        // Stop the monkey from moving during the countdown.
        if (monkey != null)
        {
            monkey.enabled = false;
        }

        // Stop all four cats from moving.
        if (cats != null)
        {
            foreach (CatMovement cat in cats)
            {
                if (cat != null)
                {
                    cat.enabled = false;
                }
            }
        }

        // Pause the game timer.
        if (gameTimer != null)
        {
            gameTimer.enabled = false;
        }

        // Begin the countdown.
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // Display 3, 2, 1.
        for (int number = 3; number > 0; number--)
        {
            // Update the countdown text.
            if (countdownText != null)
            {
                countdownText.text = number.ToString();
            }

            // Wait one second before the next number.
            yield return new WaitForSeconds(1f);
        }

        // Display GO!
        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }

        // Keep GO! visible for one second.
        yield return new WaitForSeconds(1f);

        // Hide the background and its child countdown text.
        if (countdownBackground != null)
        {
            countdownBackground.SetActive(false);
        }
        else if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        // Allow the monkey to move.
        if (monkey != null)
        {
            monkey.enabled = true;
        }

        // Allow all four cats to move.
        if (cats != null)
        {
            foreach (CatMovement cat in cats)
            {
                if (cat != null)
                {
                    cat.enabled = true;
                }
            }
        }

        // Start the game timer.
        if (gameTimer != null)
        {
            gameTimer.enabled = true;
        }

        Debug.Log("Game started!");
    }
}