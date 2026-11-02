using UnityEngine;

public class SuperPellet : MonoBehaviour
{
    // Prevents the same super pellet from being collected twice.
    private bool collected = false;

    // Called when another Collider2D enters this object's trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the trigger if the pellet was already collected.
        if (collected)
        {
            return;
        }

        // Check whether the object touching the pellet is the monkey.
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        // Ignore collisions with other objects.
        if (monkey == null)
        {
            return;
        }

        // Mark the pellet as collected.
        collected = true;

        // Award 50 points.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(50);
        }

        // Display a message for testing.
        Debug.Log("Super Pellet Collected! +50 Points");

        // Remove the collected super pellet.
        Destroy(gameObject);
    }
}