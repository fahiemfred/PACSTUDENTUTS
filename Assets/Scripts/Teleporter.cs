using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public bool teleportToRight = true;

    // Detect when the monkey enters the teleporter trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check whether the collider belongs to the monkey.
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        // Ignore everything except the monkey.
        if (monkey == null)
        {
            return;
        }

        // Move the monkey to the opposite entrance.
        monkey.TeleportMonkey(teleportToRight);
    }
}