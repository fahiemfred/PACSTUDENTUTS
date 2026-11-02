using UnityEngine;
using TMPro;

public class CatScaredManager : MonoBehaviour
{
    // Allows other scripts to access this manager.
    public static CatScaredManager Instance;

    [Header("Scared Mode")]

    // Total time cats remain scared.
    public float scaredDuration = 10f;

    // Final seconds when cats begin recovering.
    public float recoveringDuration = 3f;

    [Header("Existing HUD")]

    // Assign your existing GhostTimerText here.
    public TextMeshProUGUI catTimerText;

    [Header("Cats")]

    // References to the four cats.
    public CatState[] cats;

    [Header("Optional Music")]

    // Audio source used for background music.
    public AudioSource musicSource;

    // Music played during normal gameplay.
    public AudioClip normalMusic;

    // Music played while cats are scared.
    public AudioClip scaredMusic;

    // Remaining scared-mode time.
    private float timeRemaining = 0f;

    // Tracks whether scared mode is active.
    private bool scaredModeActive = false;

    // Allows other scripts to check scared mode.
    public bool IsScaredModeActive => scaredModeActive;

    private void Awake()
    {
        // Store the manager reference.
        Instance = this;
    }

    private void Start()
    {
        // Hide the countdown until a Power Pellet is collected.
        if (catTimerText != null)
        {
            catTimerText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Do nothing if scared mode is inactive.
        if (!scaredModeActive)
        {
            return;
        }

        // Reduce the countdown.
        timeRemaining -= Time.deltaTime;

        // End scared mode when the timer reaches zero.
        if (timeRemaining <= 0f)
        {
            EndScaredMode();
            return;
        }

        // Update the existing HUD countdown.
        if (catTimerText != null)
        {
            catTimerText.text =
                "CAT: " + Mathf.CeilToInt(timeRemaining);
        }

        // Change cats to recovering mode during the final seconds.
        if (timeRemaining <= recoveringDuration)
        {
            foreach (CatState cat in cats)
            {
                if (cat != null)
                {
                    cat.SetRecovering();
                }
            }
        }
    }

    // Called when the monkey collects a Power Pellet.
    public void ActivateScaredMode()
    {
        // Reset the countdown.
        timeRemaining = scaredDuration;
        scaredModeActive = true;

        // Display the countdown.
        if (catTimerText != null)
        {
            catTimerText.gameObject.SetActive(true);

            catTimerText.text =
                "CAT: " + Mathf.CeilToInt(timeRemaining);
        }

        // Change all cats to their scared state.
        foreach (CatState cat in cats)
        {
            if (cat != null)
            {
                cat.SetScared();
            }
        }

        // Play scared music if assigned.
        PlayMusic(scaredMusic);

        Debug.Log("Cat scared mode activated!");
    }

    // Returns cats to normal after 10 seconds.
    private void EndScaredMode()
    {
        scaredModeActive = false;
        timeRemaining = 0f;

        // Hide the countdown.
        if (catTimerText != null)
        {
            catTimerText.gameObject.SetActive(false);
        }

        // Restore normal cat states.
        foreach (CatState cat in cats)
        {
            if (cat != null)
            {
                cat.ReturnToNormal();
            }
        }

        // Restore normal music.
        PlayMusic(normalMusic);

        Debug.Log("Cat scared mode ended!");
    }

    // Changes background music if an AudioSource and clip exist.
    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}