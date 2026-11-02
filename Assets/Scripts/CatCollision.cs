using UnityEngine;

public class CatCollision : MonoBehaviour
{
    // Reference to this cat's state.
    private CatState catState;

    // Prevents repeated life loss during one collision.
    private bool touchingMonkey = false;

    private void Start()
    {
        // Find the CatState component.
        catState = GetComponent<CatState>();
    }

    // Called when the monkey enters the cat's trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    // Called while the monkey remains inside the trigger.
    private void OnTriggerStay2D(Collider2D other)
    {
        HandleCollision(other);
    }

    // Determines whether the monkey loses a life or eats the cat.
    private void HandleCollision(Collider2D other)
    {
        // Check whether the object is the monkey.
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        if (monkey == null || catState == null)
        {
            return;
        }

        // Ignore cats that have already been eaten.
        if (catState.currentState == CatState.State.Dead)
        {
            return;
        }

        // Scared and recovering cats can be eaten.
        if (catState.CanBeEaten())
        {
            catState.GetEaten();
            return;
        }

        // Normal cats remove one life.
        if (!touchingMonkey)
        {
            touchingMonkey = true;

            if (LifeManager.Instance != null)
            {
                LifeManager.Instance.LoseLife();
            }
        }
    }

    // Reset collision tracking when the monkey leaves.
    private void OnTriggerExit2D(Collider2D other)
    {
        MonkeyStudentController monkey =
            other.GetComponentInParent<MonkeyStudentController>();

        if (monkey != null)
        {
            touchingMonkey = false;
        }
    }
}