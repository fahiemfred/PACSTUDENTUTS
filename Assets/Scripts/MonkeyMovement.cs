using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MonkeyMovement : MonoBehaviour
{
    public float speed = 2f;

    //Coordinates for the four corners that the monkey needs to move to
    Vector3[] corners =
    {
        new Vector3(-9.5f, 3.5f, 0f), // top left
        new Vector3(-4.5f, 3.5f, 0f), // top right
        new Vector3(-4.5f, -0.5f, 0f), // bottom right
        new Vector3(-9.5f, -0.5f, 0f) // bottom left
    };

    int currentCorner = 0;
    int nextCorner = 1;

    Vector3 startPosition;
    Vector3 targetPosition;

    float timer = 0f;

    Animator animator;
    AudioSource audioSource;

    // monkey setup for start
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        transform.position = corners[0];

        startPosition = transform.position;

        SetAnimation();
        // finds next tile to move to
        targetPosition = GetNextTile();
    }

    void Update()
    {
        //finds distance to next tile
        float distance = Vector3.Distance(startPosition, targetPosition);

        timer += Time.deltaTime * speed / distance;

        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            timer
        );

        //checks if monkey has reached next tile and plays movement sound
        if (timer >= 1f)
        {
            transform.position = targetPosition;
            timer = 0f;

            audioSource.Play();

            startPosition = transform.position;

            // checks if monkey has reached a corner and moves it to the next
            if (Vector3.Distance(
                transform.position,
                corners[nextCorner]) < 0.01f)
            {
                currentCorner = nextCorner;

                nextCorner++;

                // checks if last corner was reached and goes back to the first corner
                if (nextCorner >= corners.Length)
                {
                    nextCorner = 0;
                }

                // changes anim at each corner
                SetAnimation();
            }

            // finds next tile
            targetPosition = GetNextTile();
        }
    }

    // finds the next tile the monkey should move to
    Vector3 GetNextTile()
    {
        Vector3 direction;

        if (currentCorner == 0)
        {
            direction = Vector3.right;
        }
        else if (currentCorner == 1)
        {
            direction = Vector3.down;
        }
        else if (currentCorner == 2)
        {
            direction = Vector3.left;
        }
        else
        {
            direction = Vector3.up;
        }

        return transform.position + direction;
    }

    // changes walking animation based on corner, this is done like this due to troubles after one loop. this was the only fix
    void SetAnimation()
    {
        if (currentCorner == 0)
        {
            animator.Play("MonkeyRight");
        }
        else if (currentCorner == 1)
        {
            animator.Play("Monkey_Forward");
        }
        else if (currentCorner == 2)
        {
            animator.Play("MonkeyLeft");
        }
        else if (currentCorner == 3)
        {
            animator.Play("MonkeyUp");
        }
    }
}