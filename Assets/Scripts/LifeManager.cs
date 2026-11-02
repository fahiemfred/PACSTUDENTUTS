using UnityEngine;
using System.Collections;

public class LifeManager : MonoBehaviour
{
    // Allows other scripts to access the LifeManager.
    public static LifeManager Instance;

    [Header("Player Lives")]

    // Number of lives the monkey starts with.
    public int startingLives = 3;

    // Current number of remaining lives.
    public int currentLives;

    [Header("HUD Life Icons")]

    // References to the existing three life icons.
    public GameObject life1;
    public GameObject life2;
    public GameObject life3;

    [Header("Player")]

    // Reference to the monkey's movement controller.
    public MonkeyStudentController monkey;

    [Header("Death Settings")]

    // How long the monkey's death animation takes.
    public float deathAnimationDuration = 1.5f;

    // Prevents losing multiple lives during one death sequence.
    private bool isDying = false;

    // Tracks whether the game has ended.
    private bool isGameOver = false;

    void Awake()
    {
        // Prevent multiple LifeManagers from existing.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register this object as the active LifeManager.
        Instance = this;
    }

    void Start()
    {
        // Give the monkey its starting lives.
        currentLives = startingLives;

        // Display the correct number of life icons.
        UpdateLifeDisplay();
    }

    void Update()
    {
        // Temporary testing shortcut. Press Q to simulate losing one life.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q key pressed");

            LoseLife();
        }
    }

    // Called when the monkey loses a life.
    public void LoseLife()
    {
        // Ignore additional collisions during death or after game over.
        if (isDying || isGameOver)
        {
            return;
        }

        // Begin the death and respawn sequence.
        StartCoroutine(DeathSequence());
    }

    // Controls the death animation, life loss and respawning.
    IEnumerator DeathSequence()
    {
        // Prevent the death sequence from starting multiple times.
        isDying = true;

        // Find the existing game timer.
        GameTimer timer = FindAnyObjectByType<GameTimer>();

        // Pause the timer while the monkey is dying.
        if (timer != null)
        {
            timer.PauseTimer();
        }

        // Stop monkey movement and play the death animation.
        if (monkey != null)
        {
            monkey.PlayDeathAnimation();
        }

        // Wait for the death animation to finish.
        yield return new WaitForSeconds(deathAnimationDuration);

        // Remove one life.
        currentLives--;

        // Prevent the number of lives from becoming negative.
        currentLives = Mathf.Max(currentLives, 0);

        // Update the three life icons.
        UpdateLifeDisplay();

        Debug.Log("Lives Remaining: " + currentLives);

        // Check whether the monkey has run out of lives.
        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn the monkey at its starting position.
            if (monkey != null)
            {
                monkey.RespawnMonkey();
            }

            // Resume the game timer.
            if (timer != null)
            {
                timer.ResumeTimer();
            }
        }

        // Allow another death sequence if the game is still active.
        isDying = false;
    }

    // Updates the existing HUD life icons.
    void UpdateLifeDisplay()
    {
        // Show Life1 if at least one life remains.
        if (life1 != null)
        {
            life1.SetActive(currentLives >= 1);
        }

        // Show Life2 if at least two lives remain.
        if (life2 != null)
        {
            life2.SetActive(currentLives >= 2);
        }

        // Show Life3 if all three lives remain.
        if (life3 != null)
        {
            life3.SetActive(currentLives >= 3);
        }
    }

    // Called when the monkey loses its final life.
    void GameOver()
    {
        // Mark the game as finished.
        isGameOver = true;

        Debug.Log("GAME OVER!");

        // The timer remains stopped. Monkey remains in its death state.
    }

    // Restores the monkey's lives when restarting the game.
    public void ResetLives()
    {
        // Restore the starting number of lives.
        currentLives = startingLives;

        // Reset the death and game-over flags.
        isGameOver = false;
        isDying = false;

        // Restore all three life icons.
        UpdateLifeDisplay();

        // Return the monkey to its starting position.
        if (monkey != null)
        {
            monkey.RespawnMonkey();
        }
    }
}