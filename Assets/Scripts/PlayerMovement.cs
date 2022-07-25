using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class PlayerMovement : MonoBehaviour
{
    public int x;
    public int y;
    
    public enum Team
    {
        Player,
        Enemy
    }
    public Team teamNo;

    public GameObject tileBeingOccupied; 

    // To increase the char's movement speed when travelling on board
    public float visualMoveSpeed = .15f; 
    
    [Header("Character Stats")] 
    public string charName;
    public int moveSpeed = 2;
    public int attackRange = 1;
    public int attackDamage = 1;
    public int maxHealthPoints = 5;
    public int currHeathPoints;

    // Location for positional updates
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeedTime = 1f;
    //private float _journeyLength;
    
    // Enum for character states
    public enum MovementStates
    {
        Unselected,
        Selected,
        Moved,
        Wait
    }
    public MovementStates charMoveState;
    
    public MapGenerator map;
    private MapManager _mapManager;
    
    // to define the play
    public Queue<int> MovementQueue;
    public Queue<int> CombatQueue;

    // Pathfinding
    public List<Node> Path = null; 
    // Nodes along the path of the shortest path from the pathfinding
    public List<Node> CurrentPath = null;

    // Path for moving character's transform
    public List<Node> PathForMovement = null;
    public bool completedMovement = false; 
    
    public MovementStates GetMovementStates(int i)
    {
        return i switch
        {
            0 => MovementStates.Unselected,
            1 => MovementStates.Selected,
            2 => MovementStates.Moved,
            3 => MovementStates.Wait,
            _ => MovementStates.Unselected
        };
    }
    public void SetMovementStates(int i)
    {
        charMoveState = i switch
        {
            0 => MovementStates.Unselected,
            1 => MovementStates.Selected,
            2 => MovementStates.Moved,
            3 => MovementStates.Wait,
            _ => MovementStates.Unselected
        };
    }

    void Awake()
    {
        MovementQueue = new Queue<int>();
        CombatQueue = new Queue<int>();

        x = (int) transform.position.x;
        y = (int) transform.position.z;
        charMoveState = MovementStates.Unselected;
    }

    void Start()
    {
        map = map.GetComponent<MapGenerator>();
        _mapManager = map.GetComponent<MapManager>();
    }

    public void MoveNextTile()
    {
        if (Path.Count == 0)
        {
            return;
        }
        else
        {
            StartCoroutine(MoveOverSeconds(transform.gameObject, Path[Path.Count - 1])); 
        }
    }

    public IEnumerator MoveCharAndFinalise()
    {
        while (MovementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }

        FinaliseMovementPos();
    }
    
    // Finalises the movement & sets the tile char moved to as occupied
    public void FinaliseMovementPos()
    {
        map.TilesOnMap[x, y].GetComponent<Tile>().charOnTile = _mapManager.selectedChar;
        
        SetMovementStates(2);
        
        _mapManager.HighlightCharAttackOptionsFromPos();
        _mapManager.HighlightTileCharIsOccupying();
    }

    public IEnumerator MoveOverSeconds(GameObject objectToMove, Node endNode)
    {
        MovementQueue.Enqueue(1);
        
        // Remove the first thing on path because its the tile we're standing on 
        Path.RemoveAt(0);
        while (Path.Count != 0)
        {
            Vector3 endPos = map.TileCoordToWorldCoord(Path[0].X, Path[0].Y);
            objectToMove.transform.position = Vector3.Lerp(transform.position, endPos, visualMoveSpeed);
            if ((transform.position - endPos).sqrMagnitude < 0.001)
            {
                Path.RemoveAt(0);
            }
            yield return new WaitForEndOfFrame();
        }

        transform.position = map.TileCoordToWorldCoord(endNode.X, endNode.Y);

        x = endNode.X;
        y = endNode.Y;

        tileBeingOccupied.GetComponent<Tile>().charOnTile = null;
        tileBeingOccupied = map.TilesOnMap[x, y];
        MovementQueue.Dequeue();
    }
}
