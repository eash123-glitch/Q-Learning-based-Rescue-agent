using UnityEngine;
using UnityEngine.Tilemaps;

public class ManualMovementController : MonoBehaviour
{
    public Tilemap environmentTilemap;
    public Tilemap wallTilemap;
    public float moveSpeed = 1.0f; // Speed at which the agent moves units per second

    private Vector3 targetPosition;

    void Start()
    {
        // Initialize the target position to be the current position
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleManualInput();
        MoveTowardsTarget();
    }

    void HandleManualInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && CanMove(Vector3.up))
            targetPosition += Vector3.up;
        if (Input.GetKeyDown(KeyCode.S) && CanMove(Vector3.down))
            targetPosition += Vector3.down;
        if (Input.GetKeyDown(KeyCode.A) && CanMove(Vector3.left))
            targetPosition += Vector3.left;
        if (Input.GetKeyDown(KeyCode.D) && CanMove(Vector3.right))
            targetPosition += Vector3.right;
    }

    void MoveTowardsTarget()
    {
        // Smoothly move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    bool CanMove(Vector3 direction)
    {
        Vector3Int newPosition = environmentTilemap.WorldToCell(transform.position + direction);
        // Checks if the newPosition is a wall or not.
        return !wallTilemap.HasTile(newPosition);
    }
}
