using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SimpleQLearningAgent : MonoBehaviour
{
    public Tilemap environmentTilemap;
    public Tilemap wallTilemap;
    public GameObject hostage;
    public Tilemap exitTilemap;
    public TileBase fireTile;
    private float previousDistanceToHostage;
    public int timeLimit = 20; // Time limit in steps
    private int timeLeft;
    public float health = 100f; // Starting health
    public float healthDecayNearFire = 30f; // Health decay per step near fire
    private List<Vector3Int> exploredTilesOrder = new List<Vector3Int>();
    public float learningRate = 0.1f;
    public float discountRate = 0.99f;
    public float explorationRate = 1.0f;
    public float minExplorationRate = 0.01f;
    public float explorationDecayRate = 0.75f;
    private HashSet<Vector3Int> exploredTiles = new HashSet<Vector3Int>();
    private Dictionary<string, Dictionary<string, float>> qTable = new Dictionary<string, Dictionary<string, float>>();
    private string[] actions = { "Up", "Down", "Left", "Right" };
    private bool isActive = true;
    private bool hostageFound = false;

    void Start()
    {
        InitializeQTable();
        timeLeft = timeLimit;
        previousDistanceToHostage = Vector3.Distance(transform.position, hostage.transform.position);
    }

    void InitializeQTable()
    {
        // Extended states considering both fire proximity and hostage distance
        var states = new List<string>();
        var healthLevels = new[] { "Safe", "CloseToFire", "OnFire", "LowHealth" };
        var distances = new[] { "VeryClose", "Close", "Medium", "Far" };

        foreach (var distance in distances)
        {
            foreach (var health in healthLevels)
            {
                string state = distance + "_" + health;
                qTable[state] = new Dictionary<string, float>();
                foreach (var action in actions)
                {
                    qTable[state].Add(action, 0f); // Initialize all actions for this state with a Q-value of 0.
                }
            }
        }
    }

    string GetCurrentState()
    {
        // Calculate the agent's distance to the hostage and categorize it.
        float distance = Vector3.Distance(transform.position, hostage.transform.position);
        string distanceCategory;
        if (distance < 1) // Adjust thresholds according to your game's scale
            distanceCategory = "VeryClose";
        else if (distance < 4)
            distanceCategory = "Close";
        else if (distance < 7)
            distanceCategory = "Medium";
        else
            distanceCategory = "Far";

        // Check proximity to fire and health level
        if (health <= 20)
            return distanceCategory + "_LowHealth";
        else if (IsOnFire())
            return distanceCategory + "_OnFire";
        else if (IsCloseToFire())
            return distanceCategory + "_CloseToFire";
        else
            return distanceCategory + "_Safe";
    }

    void Update()
    {
        if (!isActive || IsInvoking(nameof(PerformMovement))) return;
        Invoke(nameof(PerformMovement), 0.5f); // Adjust the delay as needed
    }
    void PerformMovement()
    {
        if (!isActive) return;

        Vector3Int currentPosition = environmentTilemap.WorldToCell(transform.position);
        bool allowRevisit = IsAgentStuck(currentPosition);
        string currentState = GetCurrentState();
        string action = ChooseAction(currentState, allowRevisit);
        MoveAgent(action);

        timeLeft--; // Decrement the time left
       
        UpdateHealth(currentPosition); // Decrease health if near fire

    }

void UpdateReward(Vector3Int currentPosition, string action, bool moveSuccessful, Vector3Int newPosition)
{
    float reward = -0.04f; // Base movement penalty.
    float currentDistanceToObjective = hostageFound ? Vector3.Distance(transform.position, FindNearestExitPosition()) :
                                                      Vector3.Distance(transform.position, hostage.transform.position);
    float distanceImprovement = previousDistanceToHostage - currentDistanceToObjective;
    if (distanceImprovement > 0)
    {
        reward += distanceImprovement * 5.0f; // Reward for moving closer to the objective
    }

    if (!moveSuccessful)
    {
        reward -= 5.0f; // Penalty for hitting an obstacle.
    }

    if (IsOnFire())
    {
        reward -= 10.0f; // Large penalty for being on fire.
    }
    else if (IsCloseToFire())
    {
        reward -= 2.0f; // Smaller penalty for being close to fire.
    }

    if (IsExit(newPosition))
    {
        reward += 100.0f; // Large reward for finding the exit.
    }

    if (timeLeft <= 0)
    {
        reward -= 50.0f; // Large penalty for timeout.
        isActive = false;
        Debug.Log("Time out, applying penalty and stopping agent.");
    }

    string currentState = GetCurrentState();
    string nextState = GetCurrentState();
    UpdateQTable(currentState, action, reward, nextState);

    explorationRate = Mathf.Max(minExplorationRate, explorationRate * explorationDecayRate);
    previousDistanceToHostage = currentDistanceToObjective; // Update for next calculation
}



    void UpdateHealth(Vector3Int currentPosition)
    {
        if (IsOnFire())
            health -= healthDecayNearFire * 2; // Higher health decay when on fire
        else if (IsCloseToFire())
            health -= healthDecayNearFire; // Regular health decay when close to fire
    }

    
    bool IsAgentStuck(Vector3Int currentPosition)
    {
        bool stuck = true;
        foreach (string action in actions)
        {
            Vector3Int direction = ActionToDirection(action);
            Vector3Int newPosition = currentPosition + direction;
            if (!exploredTiles.Contains(newPosition))
            {
                stuck = false; // Found an unexplored neighboring tile, so the agent isn't stuck.
                break;
            }
        }
        return stuck;
    }

    bool IsCloseToFire()
    {
        // Check the tiles adjacent to the current position for fire tiles
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in directions)
        {
            if (environmentTilemap.GetTile(environmentTilemap.WorldToCell(transform.position + (Vector3)dir)) == fireTile)
                return true;
        }
        return false;
    }
    

   void UpdateQTable(string state, string action, float reward, string nextState)
    {
        // Add dynamic state to Q-table if it doesn't exist
        if (!qTable.ContainsKey(nextState))
        {
            qTable[nextState] = new Dictionary<string, float>();
            foreach (var act in actions)
            {
                qTable[nextState].Add(act, 0f);
            }
        }

        float currentQ = qTable[state][action];
        float maxFutureQ = GetMaxQValue(nextState);
        float newQ = currentQ + learningRate * (reward + discountRate * maxFutureQ - currentQ);
        qTable[state][action] = newQ;
    }

    float GetMaxQValue(string state)
    {
        float maxQ = float.MinValue;
        foreach (var action in actions)
        {
            if (qTable[state].ContainsKey(action))
            {
                maxQ = Mathf.Max(maxQ, qTable[state][action]);
            }
        }
        return maxQ;
    }

    string ChooseAction(string state, bool allowRevisit)
    {
        List<string> possibleActions = new List<string>();
        Vector3Int currentPosition = environmentTilemap.WorldToCell(transform.position);

        foreach (var action in actions)
        {
            Vector3Int direction = ActionToDirection(action);
            Vector3Int newPosition = currentPosition + direction;
            if (!exploredTiles.Contains(newPosition) || allowRevisit)
            {
                possibleActions.Add(action);
            }
        }

        if (possibleActions.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleActions.Count);
            return possibleActions[randomIndex];
        }
        else
        {
            // If no unexplored neighboring tiles and not allowing revisit, pick a random action.
            // This could be refined to choose the least detrimental action based on your game's logic.
            return actions[Random.Range(0, actions.Length)];
        }
    }

    Vector3 FindNearestExitPosition()
    {
        // Initialize variables to keep track of the nearest exit.
        Vector3 nearestExitPosition = Vector3.positiveInfinity;
        float nearestExitDistance = float.MaxValue;

        // Iterate over each cell within the bounds of the exitTilemap.
        foreach (var position in exitTilemap.cellBounds.allPositionsWithin)
        {
            // Check if there is a tile at the current position.
            if (exitTilemap.HasTile(position))
            {
                // Convert the cell position to world coordinates.
                Vector3 worldPos = exitTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0); // Center of the tile

                // Calculate the distance from the agent's current position to the exit position.
                float distance = Vector3.Distance(transform.position, worldPos);

                // Update the nearest exit position if this one is closer.
                if (distance < nearestExitDistance)
                {
                    nearestExitPosition = worldPos;
                    nearestExitDistance = distance;
                }
            }
        }

        // Return the world position of the nearest exit.
        return nearestExitPosition;
    }



    string DetermineActionForTile(Vector3Int currentPosition, Vector3Int targetPosition)
    {
        // Calculate the direction vector
        Vector3Int direction = targetPosition - currentPosition;

        // Translate the direction vector into an action string
        if (direction == Vector3Int.up) return "Up";
        if (direction == Vector3Int.down) return "Down";
        if (direction == Vector3Int.left) return "Left";
        if (direction == Vector3Int.right) return "Right";

        return "NoOp"; // Fallback if no matching direction is found
    }

    string GetMaxQAction(string state)
    {
        string bestAction = actions[0];
        float maxQ = float.MinValue;
        foreach (var action in actions)
        {
            float qValue = qTable[state][action];
            if (qValue > maxQ)
            {
                maxQ = qValue;
                bestAction = action;
            }
        }
        return bestAction;
    }

   void MoveAgent(string action)
    {
        Vector3Int direction = ActionToDirection(action);
        Vector3Int currentPosition = environmentTilemap.WorldToCell(transform.position);
        Vector3Int newPosition = currentPosition + direction;

        bool moveSuccessful = CanMove(newPosition);
        if (moveSuccessful)
        {
            transform.position = environmentTilemap.CellToWorld(newPosition) + new Vector3(0.5f, 0.5f, 0); // Centering adjustment
            if (!exploredTiles.Contains(newPosition))
            {
                exploredTiles.Add(newPosition);
                exploredTilesOrder.Add(newPosition); // Track the exploration order
            }

            if (FoundHostage())
            {
                hostageFound = true; // Update state to indicate the hostage has been found.
                previousDistanceToHostage = Vector3.Distance(transform.position, FindNearestExitPosition()); // Update target to nearest exit
            }

            if (IsExit(newPosition))
            {
                Debug.Log("Exit found, mission completed.");
                isActive = false; // Deactivate the agent as the mission is complete.
            }
        }
        UpdateReward(currentPosition, action, moveSuccessful, newPosition);
    }


    Vector3Int ActionToDirection(string action)
    {
        // Example implementation - adjust as necessary for your coordinate system and map layout.
        switch (action)
        {
            case "Up": return new Vector3Int(0, 1, 0);
            case "Down": return new Vector3Int(0, -1, 0);
            case "Left": return new Vector3Int(-1, 0, 0);
            case "Right": return new Vector3Int(1, 0, 0);
            default: return Vector3Int.zero;
        }
    }
    bool IsExit(Vector3Int position)
    {
        return exitTilemap.HasTile(position);
    }




    bool CanMove(Vector3Int newPosition)
    {
        // Checks the wallTilemap for walls at the newPosition.
        if (wallTilemap.HasTile(newPosition) || exitTilemap.HasTile(newPosition))
        {
            return false; // newPosition has a wall tile, indicating the agent can't move there.
        }
        return true; // No wall tile at newPosition, movement is allowed.
    }

    bool IsOnFire()
    {
        Vector3Int currentPosition = environmentTilemap.WorldToCell(transform.position);
        return environmentTilemap.GetTile(currentPosition) == fireTile;
    }

    bool FoundHostage()
    {
        Vector3Int agentPosition = environmentTilemap.WorldToCell(transform.position);
        Vector3Int hostagePosition = environmentTilemap.WorldToCell(hostage.transform.position);
        return agentPosition == hostagePosition;
    }
}