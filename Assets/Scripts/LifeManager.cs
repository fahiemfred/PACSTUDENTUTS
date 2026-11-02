using UnityEngine;
using System.Collections;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;

    [Header("Player Lives")]
    public int startingLives = 3;
    public int currentLives;

    [Header("HUD Life Icons")]
    public GameObject life1;
    public GameObject life2;
    public GameObject life3;

    [Header("Player")]
    public MonkeyStudentController monkey;

    [Header("Cats")]
    public CatMovement[] cats;

    [Header("Death Settings")]
    public float deathAnimationDuration = 1.5f;

    private bool isDying = false;
    private bool isGameOver = false;

    public bool IsGameOver
    {
        get { return isGameOver; }
    }

    // Allows other scripts to check whether the monkey is currently dying.
    public bool IsDying
    {
        get { return isDying; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentLives = startingLives;
        UpdateLifeDisplay();
    }

    private void Update()
    {
        // Temporary Q key for testing life loss.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q key pressed");
            LoseLife();
        }
    }

    // Called when the monkey collides with a normal cat.
    public void LoseLife()
    {
        // Prevent multiple deaths at the same time.
        if (isDying || isGameOver)
        {
            return;
        }

        StartCoroutine(DeathSequence());
    }

    // Handles the monkey's death animation and respawning.
    private IEnumerator DeathSequence()
    {
        isDying = true;

        GameTimer timer = FindAnyObjectByType<GameTimer>();

        // Pause the timer during the death animation.
        if (timer != null)
        {
            timer.PauseTimer();
        }

        // Stop the cats while the monkey dies.
        SetCatsMovement(false);

        // Play the monkey's death animation.
        if (monkey != null)
        {
            monkey.PlayDeathAnimation();
        }

        // Wait until the death animation finishes.
        yield return new WaitForSeconds(deathAnimationDuration);

        // Remove one life.
        currentLives--;

        currentLives = Mathf.Max(currentLives, 0);

        UpdateLifeDisplay();

        Debug.Log("Lives Remaining: " + currentLives);

        // Check whether the monkey has any lives remaining.
        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn the monkey.
            if (monkey != null)
            {
                monkey.RespawnMonkey();
            }

            // Allow cats to move again.
            SetCatsMovement(true);

            // Resume the timer.
            if (timer != null)
            {
                timer.ResumeTimer();
            }
        }

        isDying = false;
    }

    // Enable or disable movement for all four cats.
    private void SetCatsMovement(bool canMove)
    {
        if (cats == null)
        {
            return;
        }

        foreach (CatMovement cat in cats)
        {
            if (cat != null)
            {
                cat.enabled = canMove;
            }
        }
    }

    // Update the three life icons in the HUD.
    private void UpdateLifeDisplay()
    {
        if (life1 != null)
        {
            life1.SetActive(currentLives >= 1);
        }

        if (life2 != null)
        {
            life2.SetActive(currentLives >= 2);
        }

        if (life3 != null)
        {
            life3.SetActive(currentLives >= 3);
        }
    }

    // Called when the monkey loses its final life.
    private void GameOver()
    {
        isGameOver = true;

        // Stop all cats.
        SetCatsMovement(false);

        // Stop the timer.
        GameTimer timer = FindAnyObjectByType<GameTimer>();

        if (timer != null)
        {
            timer.StopTimer();
        }

        // Disable monkey movement.
        if (monkey != null)
        {
            monkey.enabled = false;
        }

        // Display the Game Over Panel.
        if (GameEndUI.Instance != null)
        {
            GameEndUI.Instance.ShowGameOver();
        }

        Debug.Log("GAME OVER!");
    }

    // Reset lives when starting a new game.
    public void ResetLives()
    {
        currentLives = startingLives;

        isGameOver = false;
        isDying = false;

        UpdateLifeDisplay();

        if (monkey != null)
        {
            monkey.RespawnMonkey();
            monkey.enabled = true;
        }

        SetCatsMovement(true);
    }
}