using UnityEngine;
using System.Collections;

public class BananaController : MonoBehaviour
{
    [Header("Banana Prefab")]
    public GameObject bananaPrefab;

    [Header("Maze Boundaries")]
    public float leftBoundary = -10f;
    public float rightBoundary = 10f;
    public float centreY = 0f;

    [Header("Movement")]
    public float travelDuration = 8f;
    public float spawnDelay = 5f;
    public float outsideOffset = 2f;

    private GameObject currentBanana;

    void Start()
    {
        StartCoroutine(BananaSpawnLoop());
    }

    IEnumerator BananaSpawnLoop()
    {
        while (true)
        {
            // Wait 5 seconds before spawning
            yield return new WaitForSeconds(spawnDelay);

            if (bananaPrefab == null)
            {
                Debug.LogError("Assign the Banana prefab!");
                yield break;
            }

            // Randomly choose which side to spawn from
            bool spawnFromLeft = Random.value < 0.5f;

            Vector3 startPosition;
            Vector3 endPosition;

            if (spawnFromLeft)
            {
                startPosition = new Vector3(
                    leftBoundary - outsideOffset,
                    centreY,
                    0f
                );

                endPosition = new Vector3(
                    rightBoundary + outsideOffset,
                    centreY,
                    0f
                );
            }
            else
            {
                startPosition = new Vector3(
                    rightBoundary + outsideOffset,
                    centreY,
                    0f
                );

                endPosition = new Vector3(
                    leftBoundary - outsideOffset,
                    centreY,
                    0f
                );
            }

            // Spawn banana outside the maze
            currentBanana = Instantiate(
                bananaPrefab,
                startPosition,
                Quaternion.identity
            );

            float elapsedTime = 0f;

            // Move using linear lerping
            while (elapsedTime < travelDuration &&
                   currentBanana != null)
            {
                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsedTime / travelDuration
                );

                currentBanana.transform.position =
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        progress
                    );

                yield return null;
            }

            // Destroy banana after travelling across
            if (currentBanana != null)
            {
                Destroy(currentBanana);
            }

            // Wait until it is destroyed
            yield return new WaitUntil(
                () => currentBanana == null
            );
        }
    }
}