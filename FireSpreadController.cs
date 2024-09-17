using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class FireSpreadController : MonoBehaviour
{
    public Tilemap wallTilemap; // Assign your wall Tilemap here in the Inspector
    public Tilemap fireTilemap;
    public Tilemap exitsTilemap; // Assign your fire Tilemap here in the Inspector
    public TileBase fireTile; // Assign your fire Tile asset here in the Inspector
    public float spreadInterval = 5.0f; // Time in seconds between fire spread attempts

    void Start()
    {
        // Starting from the center for demonstration, adjust as necessary
        Vector3Int startTilePosition = new Vector3Int(-2, -5, 0);
        StartCoroutine(SpreadFire(startTilePosition));
    }

    IEnumerator SpreadFire(Vector3Int startTilePosition)
    {
        HashSet<Vector3Int> firePositions = new HashSet<Vector3Int> { startTilePosition };
        // Set the starting tile on fire
        fireTilemap.SetTile(startTilePosition, fireTile);

        while (firePositions.Count > 0)
        {
            HashSet<Vector3Int> newFirePositions = new HashSet<Vector3Int>();

            foreach (var firePosition in firePositions)
            {
                bool spreadSuccessfully = false;
                List<Vector2Int> directionsTried = new List<Vector2Int>();

                while (!spreadSuccessfully && directionsTried.Count < 4)
                {
                    Vector2Int direction = ChooseRandomDirection(directionsTried);
                    Vector3Int newPosition = firePosition + new Vector3Int(direction.x, direction.y, 0);

                    // If newPosition is blocked by a wall, choose a new direction in the next iteration
                    if (!wallTilemap.HasTile(newPosition) && !exitsTilemap.HasTile(newPosition))
                    {
                        fireTilemap.SetTile(newPosition, fireTile); // Set a fire tile at the new position
                        newFirePositions.Add(newPosition);
                        spreadSuccessfully = true;
                    }
                    directionsTried.Add(direction);
                }
            }

            firePositions = new HashSet<Vector3Int>(newFirePositions);
            yield return new WaitForSeconds(spreadInterval);
        }
    }

    Vector2Int ChooseRandomDirection(List<Vector2Int> excludeDirections)
    {
        Vector2Int[] allDirections = {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0)  // Left
        };
        Vector2Int chosenDirection;
        do
        {
            chosenDirection = allDirections[Random.Range(0, allDirections.Length)];
        } while (excludeDirections.Contains(chosenDirection));

        return chosenDirection;
    }
}