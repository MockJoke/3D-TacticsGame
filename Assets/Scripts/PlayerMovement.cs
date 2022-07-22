using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int x;
    public int y; 
    
    public enum team
    {
        Player,
        Enemy
    }
    public team teamNo;
    
    public GameObject tileBeingOccupied;

    public MapGenerator map;
    
    // Location for positional updates
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeedTime = 1f;
    private float journeyLength;

    public bool charSelected = false;

    public GameObject selectedChar;
    
    // Enum for character states
    public enum movementStates
    {
        Unselected,
        Selected,
        Moved,
        Wait
    }
    public movementStates characterMoveState;
    
    // Pathfinding
    public List<Node> path = null; 
    // Nodes along the path of the shortest path from the pathfinding
    public List<Node> currentPath = null;

    // Path for moving character's transform
    public List<Node> pathForMovement = null;
    public bool completedMovement = false; 

    public movementStates getMovementStates(int i)
    {
        switch (i)
        {
            case 0:
                return movementStates.Unselected;
                break;
            case 1:
                return movementStates.Selected;
                break;
            case 2:
                return movementStates.Moved;
                break;
            case 3:
                return movementStates.Wait;
                break;
            default:
                return movementStates.Unselected; 
                break;
        }
    }
    public void setMovementStates(int i)
    {
        switch (i)
        {
            case 0:
                characterMoveState = movementStates.Unselected;
                break;
            case 1:
                characterMoveState = movementStates.Selected;
                break;
            case 2:
                characterMoveState = movementStates.Moved;
                break;
            case 3:
                characterMoveState = movementStates.Wait;
                break;
            default:
                characterMoveState = movementStates.Unselected; 
                break;
        }
    }
}
