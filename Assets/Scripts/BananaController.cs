using UnityEngine;
using System.Collections;

public class BananaController : MonoBehaviour
{
    // Prefab used to create the moving banana.
    [Header("Banana Prefab")]
    public GameObject bananaPrefab;

    // Defines the horizontal boundaries of the maze.
    [Header("Maze Boundaries")]
    public float leftBoundary = -10f;
    public float rightBoundary = 10f;

    // Controls the vertical position of the banana's path.
    public float centreY = 0f;

    // Controls the banana's movement and spawning.
    [Header("Movement")]
    public float travelDuration = 8f;
    public float spawnDelay = 5f;

    // Distance outside the maze where bananas start and finish.
    public float outsideOffset = 2f;

    // Reference to the currently active banana.
    private GameObject currentBanana;

    void Start()
    {
        // Begin repeatedly spawning bananas.
        StartCoroutine(BananaSpawnLoop());
    }

    IEnumerator BananaSpawnLoop()
    {
        // Continue spawning bananas while the controller is active.
        while (true)
        {
            // Wait five seconds before spawning a banana.
            yield return new WaitForSeconds(spawnDelay);

            // Check that the banana prefab has been assigned.
            if (bananaPrefab == null)
            {
                Debug.LogError("Assign the Banana prefab!");
                yield break;
            }

            // Randomly choose whether the banana starts on the left or right.
            bool spawnFromLeft = Random.value < 0.5f;

            Vector3 startPosition;
            Vector3 endPosition;

            if (spawnFromLeft)
            {
                // Spawn outside the left edge of the maze.
                startPosition = new Vector3(
                    leftBoundary - outsideOffset,
                    centreY,
                    0f
                );

                // Move towards the opposite side.
                endPosition = new Vector3(
                    rightBoundary + outsideOffset,
                    centreY,
                    0f
                );
            }
            else
            {
                // Spawn outside the right edge of the maze.
                startPosition = new Vector3(
                    rightBoundary + outsideOffset,
                    centreY,
                    0f
                );

                // Move towards the opposite side.
                endPosition = new Vector3(
                    leftBoundary - outsideOffset,
                    centreY,
                    0f
                );
            }

            // Create the banana at its starting position.
            currentBanana = Instantiate(
                bananaPrefab,
                startPosition,
                Quaternion.identity
            );

            // Tracks how long the banana has been moving.
            float elapsedTime = 0f;

            // Continue moving until the banana reaches the opposite side or is collected by the monkey.
            while (elapsedTime < travelDuration &&
                   currentBanana != null)
            {
                // Increase elapsed time each frame.
                elapsedTime += Time.deltaTime;

                // Calculate movement progress between 0 and 1.
                float progress = Mathf.Clamp01(
                    elapsedTime / travelDuration
                );

                // Move the banana in a straight line using linear lerping.
                currentBanana.transform.position =
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        progress
                    );

                // Wait until the next frame.
                yield return null;
            }

            // Destroy the banana when it finishes travelling.
            if (currentBanana != null)
            {
                Destroy(currentBanana);
            }

            // Wait until the banana has been destroyed. Handles bananas collected by the monkey.
            yield return new WaitUntil(
                () => currentBanana == null
            );
        }
    }
}