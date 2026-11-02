using UnityEngine;
using UnityEngine.Tilemaps;

public class MonkeyStudentController : MonoBehaviour
{
    [Header("Maze Tilemaps")]
    public Tilemap[] levelTilemaps;

    [Header("Tile Types")]
    public TileBase[] wallTiles;
    public TileBase ghostWallTile;
    public TileBase[] pelletTiles;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer monkeySprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pelletMoveSound;
    public AudioClip emptyMoveSound;

    [Header("Particles")]
    public ParticleSystem dustParticles;

    // Most recently pressed direction
    private Vector2Int lastInput = Vector2Int.zero;

    // Current movement direction
    private Vector2Int currentInput = Vector2Int.zero;

    // Positions used for linear lerping
    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float moveProgress = 0f;
    private bool isMoving = false;

    // Reference Tilemap
    private Tilemap gridTilemap;

    void Start()
    {
        // Check that the Tilemaps are assigned
        if (levelTilemaps == null ||
            levelTilemaps.Length == 0 ||
            levelTilemaps[0] == null)
        {
            Debug.LogError(
                "Assign TilemapTopRight to Element 0!"
            );

            enabled = false;
            return;
        }

        // Use the unmirrored Tilemap as the reference
        gridTilemap = levelTilemaps[0];

        // Find the monkey's starting cell
        Vector3Int startCell =
            gridTilemap.WorldToCell(transform.position);

        // Align the monkey with the centre of the cell
        Vector3 centre =
            gridTilemap.GetCellCenterWorld(startCell);

        centre.z = transform.position.z;
        transform.position = centre;

        startPosition = transform.position;
        targetPosition = transform.position;

        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;

        // Find the SpriteRenderer if not assigned
        if (monkeySprite == null)
        {
            monkeySprite =
                GetComponentInChildren<SpriteRenderer>();
        }

        // Do not rotate or flip the monkey
        transform.rotation = Quaternion.identity;

        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        // Start facing right with animation paused
        if (animator != null)
        {
            animator.SetFloat("MoveX", 1f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsMoving", false);

            animator.speed = 0f;
        }

        // Stop dust particles at the beginning
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }
    }

    void Update()
    {
        // Read WASD input
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = Vector2Int.up;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = Vector2Int.left;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = Vector2Int.down;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = Vector2Int.right;
        }

        // Finish moving to the next tile
        if (isMoving)
        {
            Move();
            return;
        }

        // Try the newest direction first
        if (CanMove(lastInput))
        {
            currentInput = lastInput;
            StartMoving(currentInput);
        }

        // Otherwise continue in the previous direction
        else if (CanMove(currentInput))
        {
            StartMoving(currentInput);
        }

        // Stop if both directions are blocked
        else
        {
            StopMoving();
        }
    }

    void StartMoving(Vector2Int direction)
    {
        // Save current world position
        startPosition = transform.position;

        // Calculate next position in world space
        Vector3 nextPosition = startPosition +
            new Vector3(direction.x, direction.y, 0f);

        // Find destination cell
        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        // Find destination cell centre
        targetPosition =
            gridTilemap.GetCellCenterWorld(nextCell);

        targetPosition.z = startPosition.z;

        // Begin linear movement
        moveProgress = 0f;
        isMoving = true;

        // Update directional animation
        FaceDirection(direction);

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.speed = 1f;
        }

        // Play appropriate movement sound
        PlayMovementSound(nextCell);

        // Play running dust
        if (dustParticles != null &&
            !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }
    }

    void Move()
    {
        // Move at the assigned speed
        moveProgress += Time.deltaTime * moveSpeed;

        // Linear lerping between tiles
        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            Mathf.Clamp01(moveProgress)
        );

        // Finish exactly at the destination
        if (moveProgress >= 1f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
    }

    void StopMoving()
    {
        // Pause animation on its current frame
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.speed = 0f;
        }

        // Stop movement sound
        if (audioSource != null &&
            audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Stop dust particles
        if (dustParticles != null &&
            dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }

    bool CanMove(Vector2Int direction)
    {
        // Do not move before a key is pressed
        if (direction == Vector2Int.zero)
        {
            return false;
        }

        // Calculate destination in world coordinates
        Vector3 nextPosition = transform.position +
            new Vector3(direction.x, direction.y, 0f);

        // Find destination cell
        Vector3Int nextCell =
            gridTilemap.WorldToCell(nextPosition);

        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        bool foundWalkableTile = false;

        // Check all four Tilemaps
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            // Convert world position to this Tilemap's cell
            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null)
            {
                continue;
            }

            // Normal walls block movement
            if (wallTiles != null)
            {
                foreach (TileBase wall in wallTiles)
                {
                    if (wall != null && tile == wall)
                    {
                        return false;
                    }
                }
            }

            // Ghost walls also block movement
            if (ghostWallTile != null &&
                tile == ghostWallTile)
            {
                return false;
            }

            // A walkable tile was found
            foundWalkableTile = true;
        }

        // Allow normal walkable Tilemap cells
        if (foundWalkableTile)
        {
            return true;
        }

        // Check separate animated super pellet GameObjects
        GameObject[] superPellets =
            GameObject.FindGameObjectsWithTag("SuperPellet");

        foreach (GameObject pellet in superPellets)
        {
            // Find the super pellet's reference grid cell
            Vector3Int pelletCell =
                gridTilemap.WorldToCell(
                    pellet.transform.position
                );

            // Allow movement onto the super pellet's cell
            if (pelletCell == nextCell)
            {
                return true;
            }
        }

        // Otherwise block movement
        return false;
    }

    void FaceDirection(Vector2Int direction)
    {
        // Do not flip because every direction
        // has its own animation
        if (monkeySprite != null)
        {
            monkeySprite.flipX = false;
        }

        // Update Blend Tree parameters
        if (animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }

    void PlayMovementSound(Vector3Int nextCell)
    {
        if (audioSource == null)
        {
            return;
        }

        // Find destination world position
        Vector3 nextWorld =
            gridTilemap.GetCellCenterWorld(nextCell);

        bool hasPellet = false;

        // Check all four Tilemaps for regular pellets
        foreach (Tilemap tilemap in levelTilemaps)
        {
            if (tilemap == null)
            {
                continue;
            }

            Vector3Int cell =
                tilemap.WorldToCell(nextWorld);

            TileBase tile = tilemap.GetTile(cell);

            if (tile == null || pelletTiles == null)
            {
                continue;
            }

            foreach (TileBase pellet in pelletTiles)
            {
                if (pellet != null && tile == pellet)
                {
                    hasPellet = true;
                    break;
                }
            }

            if (hasPellet)
            {
                break;
            }
        }

        // Choose movement sound
        AudioClip selectedSound =
            hasPellet ? pelletMoveSound : emptyMoveSound;

        // Stop if no clip is assigned
        if (selectedSound == null)
        {
            audioSource.Stop();
            return;
        }

        // Switch clips only when needed
        if (audioSource.clip != selectedSound ||
            !audioSource.isPlaying)
        {
            audioSource.clip = selectedSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}