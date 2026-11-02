using UnityEngine;

public class BananaCollectible : MonoBehaviour
{
    // Prevents the same banana from being collected multiple times.
    private bool collected = false;

    // Called when another Collider2D enters the banana's trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the collision if the banana was already collected.
        if (collected)
        {
            return;
        }

        // Check whether the object touching the banana is the monkey.
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        // Ignore collisions with objects other than the monkey.
        if (monkey == null)
        {
            return;
        }

        // Mark the banana as collected.
        collected = true;

        // Award 100 points.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
        }

        // Display a message for testing.
        Debug.Log("Banana Collected! +100 Points");

        // Destroy the collected banana.
        Destroy(gameObject);
    }
}