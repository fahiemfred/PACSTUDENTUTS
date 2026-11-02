using UnityEngine;
using System.Collections;

public class CatState : MonoBehaviour
{
    // The four possible states for a cat.
    public enum State
    {
        Normal,
        Scared,
        Recovering,
        Dead
    }

    [Header("Cat State")]

    // Current state of this cat.
    public State currentState = State.Normal;

    [Header("Animation")]

    // Animator containing normal, scared and death animations.
    public Animator catAnimator;

    [Header("Cat Eaten")]

    // Time before an eaten cat returns.
    public float returnDelay = 3f;

    // Optional sound when the monkey eats a cat.
    public AudioClip catEatSound;

    // Stores the cat's original position.
    private Vector3 startingPosition;

    // Reference to the existing movement script.
    private CatMovement catMovement;

    // Reference to the AudioSource.
    private AudioSource audioSource;

    // Tracks the respawn coroutine.
    private Coroutine returnCoroutine;

    private void Start()
    {
        // Find the Animator if it was not assigned.
        if (catAnimator == null)
        {
            catAnimator = GetComponent<Animator>();
        }

        // Get the existing movement and audio components.
        catMovement = GetComponent<CatMovement>();
        audioSource = GetComponent<AudioSource>();

        // Save the cat's starting position.
        startingPosition = transform.position;

        // Start with normal animations.
        if (catAnimator != null)
        {
            catAnimator.SetBool("IsScared", false);
            catAnimator.SetBool("IsDead", false);
        }
    }

    // Returns true when the monkey can eat this cat.
    public bool CanBeEaten()
    {
        return currentState == State.Scared ||
               currentState == State.Recovering;
    }

    // Activates the cat's scared animations.
    public void SetScared()
    {
        // Do not change cats that are currently dead.
        if (currentState == State.Dead)
        {
            return;
        }

        currentState = State.Scared;

        // Switch from normal to scared movement.
        if (catAnimator != null)
        {
            catAnimator.SetBool("IsScared", true);
        }
    }

    // Marks the cat as recovering.
    public void SetRecovering()
    {
        // Only scared cats can begin recovering.
        if (currentState != State.Scared)
        {
            return;
        }

        currentState = State.Recovering;
    }

    // Returns the cat to its normal animations.
    public void ReturnToNormal()
    {
        // Dead cats must finish their respawn sequence.
        if (currentState == State.Dead)
        {
            return;
        }

        currentState = State.Normal;

        if (catAnimator != null)
        {
            catAnimator.SetBool("IsScared", false);
        }
    }

    // Called when the monkey touches a scared cat.
    public void GetEaten()
    {
        // Prevent normal or dead cats from being eaten.
        if (!CanBeEaten())
        {
            return;
        }

        // Change the cat to its dead state.
        currentState = State.Dead;

        // Award 300 points.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(300);
        }

        // Play the eating sound if assigned.
        if (audioSource != null && catEatSound != null)
        {
            audioSource.PlayOneShot(catEatSound);
        }

        // Stop the cat from moving.
        if (catMovement != null)
        {
            catMovement.enabled = false;
        }

        // Play the existing death animation.
        if (catAnimator != null)
        {
            catAnimator.SetBool("IsDead", true);
        }

        // Stop any previous respawn coroutine.
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        // Start the respawn countdown.
        returnCoroutine = StartCoroutine(ReturnAfterDelay());

        Debug.Log("Cat eaten! +300 points");
    }

    // Waits before respawning the eaten cat.
    private IEnumerator ReturnAfterDelay()
    {
        // Allow the death animation to play.
        yield return new WaitForSeconds(returnDelay);

        // Return the cat to its starting position.
        transform.position = startingPosition;

        // Check whether scared mode is still active.
        bool stillScared =
            CatScaredManager.Instance != null &&
            CatScaredManager.Instance.IsScaredModeActive;

        // Restore the correct state.
        currentState = stillScared ? State.Scared : State.Normal;

        if (catAnimator != null)
        {
            // Choose the appropriate movement animations.
            catAnimator.SetBool("IsScared", stillScared);

            // Exit the death animation.
            catAnimator.SetBool("IsDead", false);
        }

        // Resume normal cat movement.
        if (catMovement != null)
        {
            catMovement.enabled = true;
        }

        returnCoroutine = null;
    }
}