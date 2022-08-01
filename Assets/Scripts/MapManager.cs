using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapManager : MonoBehaviour
{
    private MapGenerator _map;
    public CharacterController[] character;
    private GameManager _gameManager;

    public GameObject charsOnBoard; 
    
    [Header("Selected Char Info")] 
    public GameObject selectedChar;
    public HashSet<Node> SelectedCharTotalRange; 
    public HashSet<Node> SelectedCharMoveRange;
    
    public bool charSelected = false;

    public int charSelectedPrevX;
    public int charSelectedPrevY;

    public GameObject prevOccupiedTile;
    
    [Header("Materials")]
    public Material moveRangeUIMat;
    public Material attackRangeUIMat;

    void Awake()
    {
        _map = GetComponent<MapGenerator>();
        character = FindObjectsOfType(typeof(CharacterController)) as CharacterController[];
        _gameManager = GetComponent<GameManager>();
    }

    private void Start()
    {
        character[0] = FindObjectOfType(typeof(PlayerController)) as CharacterController; 
        character[1] = FindObjectOfType(typeof(EnemyController)) as CharacterController;
    }

    void Update()
    {
        for (int i = 0; i < character.Length; i++)
        { 
            // If input is left mouse down then selects the character
            if (Input.GetMouseButtonDown(0))
            {
                if (selectedChar == null)
                {
                    MouseClickToSelectChar();
                }
                
                // Once the char has been selected, need to check if the character has entered the selection state (1) 'Selected'; if yes then move the character
                else if (character[i].charMoveState == character[i].GetMovementStates(1) && character[i].MovementQueue.Count == 0)
                {
                    if (selectedChar.GetComponent<CharacterController>().teamNo == CharacterController.Team.Player)
                    {
                        if (SelectTileToMoveTo())
                        {
                            Debug.Log("Movement path has been selected");
                            charSelectedPrevX = character[i].x;
                            charSelectedPrevY = character[i].y;
                            prevOccupiedTile = character[i].tileBeingOccupied;
                            
                            _gameManager.MoveChar();
                            StartCoroutine(character[i].MoveCharAndFinalise()); 
                        }
                    }
                    else if (selectedChar.GetComponent<CharacterController>().teamNo == CharacterController.Team.Enemy)
                    {
                        Debug.Log("Movement path has been selected");
                        charSelectedPrevX = character[i].x;
                        charSelectedPrevY = character[i].y;
                        prevOccupiedTile = character[i].tileBeingOccupied;
                        
                        selectedChar.GetComponent<EnemyController>().FindTargetToMoveTo();
                        
                        _gameManager.MoveChar();
                        StartCoroutine(character[i].MoveCharAndFinalise());
                    }
                }
            }
            // Deselect the char with the right click
            if (Input.GetMouseButtonDown(1))
            {
                if (selectedChar != null)
                {
                    if (selectedChar.GetComponent<CharacterController>().MovementQueue.Count == 0)
                    {
                        if (selectedChar.GetComponent<CharacterController>().charMoveState !=
                        selectedChar.GetComponent<CharacterController>().GetMovementStates(3))
                        {
                            DeselectChar();
                        }
                    }
                    else if (selectedChar.GetComponent<CharacterController>().MovementQueue.Count == 1)
                    {
                        selectedChar.GetComponent<CharacterController>().visualMoveSpeed = 0.5f;
                    }
                }
            }
        }
    }

    // Selects a character based on the cursor click
    private void MouseClickToSelectChar()
    {
        if (charSelected == false && _gameManager.tileBeingDisplayed != null)
        {
            if (_gameManager.tileBeingDisplayed.GetComponent<Tile>().charOnTile != null)
            {
                GameObject tempSelectedChar = _gameManager.tileBeingDisplayed.GetComponent<Tile>().charOnTile;

                if (tempSelectedChar.GetComponent<CharacterController>().charMoveState == tempSelectedChar
                        .GetComponent<CharacterController>().GetComponent<CharacterController>().GetMovementStates(0))
                {
                    selectedChar = tempSelectedChar;
                    selectedChar.GetComponent<CharacterController>()._mapGenerator = _map;
                    selectedChar.GetComponent<CharacterController>().SetMovementStates(1);
                    charSelected = true;
                    HighlightCharRange();
                }
            }
        }
    }
    
    // Checks if the tile that has been clicked is movable for the selected char
    private bool SelectTileToMoveTo()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                int clickedTileX = hit.transform.GetComponent<Tile>().tileX;
                int clickedTileY = hit.transform.GetComponent<Tile>().tileY;
                Node nodeToCheck = _map.Graph[clickedTileX, clickedTileY];

                if (SelectedCharMoveRange.Contains(nodeToCheck))
                {
                    if (hit.transform.gameObject.GetComponent<Tile>().charOnTile == null ||
                        hit.transform.gameObject.GetComponent<Tile>().charOnTile == selectedChar)
                    {
                        Debug.Log("The tile to move to has been selected");
                        GeneratePathTo(clickedTileX, clickedTileY);
                        
                        return true;
                    }
                }
            }
        }
        else if(hit.transform.gameObject.CompareTag("Player"))
        {
            if (hit.transform.parent.GetComponent<CharacterController>().teamNo !=
                selectedChar.GetComponent<CharacterController>().teamNo)
            {
                Debug.Log("Clicked an enemy");
            }
            else if (hit.transform.parent.gameObject == selectedChar)
            {
                GeneratePathTo(selectedChar.GetComponent<CharacterController>().x, selectedChar.GetComponent<CharacterController>().y);
                
                return true;
            }
        }
        
        return false;
    }
    
    // Sets the tile as occupied if a char is on the tile
    public void SetIfTileIsOccupied()
    {
        foreach (Transform team in charsOnBoard.transform)
        {
            foreach (Transform charOnTeam in team)
            {
                int charX = charOnTeam.GetComponent<CharacterController>().x;
                int charY = charOnTeam.GetComponent<CharacterController>().y;
                charOnTeam.GetComponent<CharacterController>().tileBeingOccupied = _map.TilesOnMap[charX, charY];
                _map.TilesOnMap[charX, charY].GetComponent<Tile>().charOnTile = charOnTeam.gameObject;
                _map.TilesOnMap[charX, charY].GetComponent<Tile>().isTileOccupied = true;
            }
        }
    }
    
    // If the tile isn't occupied by another team & if the tile is walkable then you can walk through 
    public bool CharCanEnterTile(int x, int y)
    {
        if (_map.TilesOnMap[x, y].GetComponent<Tile>().charOnTile != null)
        {
            if (_map.TilesOnMap[x, y].GetComponent<Tile>().charOnTile.GetComponent<CharacterController>().teamNo ==
                CharacterController.Team.Enemy)
            {
                return false;
            }
            // if (_map.TilesOnMap[x, y].GetComponent<Tile>().charOnTile.GetComponent<CharacterController>().teamNo !=
            //     selectedChar.GetComponent<CharacterController>().teamNo)
            // {
            //     return false;
            // }
        }
        return _map.tileTypes[_map.Tiles[x, y]].isWalkable;
    }
    
    // checks the cost of a tile for a unit to enter 
    public float CostToEnterTile(int x, int y)
    {
        if (CharCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;
        }

        TileType t = _map.tileTypes[_map.Tiles[x, y]];
        float dist = t.movementCost;

        return dist; 
    }
    
    // Generates path for the selected character
    public void GeneratePathTo(int x, int y)
    {
        if (selectedChar.GetComponent<CharacterController>().x == x &&
            selectedChar.GetComponent<CharacterController>().y == y)
        {
            Debug.Log("Clicked the same tile that the character currently standing on");
            selectedChar.GetComponent<CharacterController>().CurrentPath = new List<Node>();
            selectedChar.GetComponent<CharacterController>().Path =
                selectedChar.GetComponent<CharacterController>().CurrentPath;
            
            return;
        }

        if (CharCanEnterTile(x, y) == false)
        {
            // can't move onto that position so can't set it as an endpoint so just return
            return;
        }
        
        selectedChar.GetComponent<CharacterController>().Path = null;
        selectedChar.GetComponent<CharacterController>().CurrentPath = null;
        
        // Path finding algorithm
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = _map.Graph[selectedChar.GetComponent<CharacterController>().x,
            selectedChar.GetComponent<CharacterController>().y];
        Node target = _map.Graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        List<Node> unvisited = new List<Node>(); // unchecked Nodes

        Debug.Log(CostToEnterTile(target.X, target.Y));

        // Initialise 
        foreach (Node n in _map.Graph)
        {
            // Initialise to infinite distance as we don't know the answer 
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        // If there's a node in the unvisited list then check it
        while (unvisited.Count > 0)
        {
            // u will be the unvisited node with the shortest distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.Neighbours)
            {
            
                float alt = dist[u] + CostToEnterTile(n.X, n.Y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        
        // If were here then found the shortest path or no path exists
        if (prev[target] == null)
        {
            // No route
            return;
        }

        selectedChar.GetComponent<CharacterController>().CurrentPath = new List<Node>();
        Node curr = target;
        
        Debug.Log(curr.X + "," + curr.Y);
        // Step through the current path and add it to the chain
        while (curr != null)
        {
            selectedChar.GetComponent<CharacterController>().CurrentPath.Add(curr);
            curr = prev[curr];
        }

        // Currently currPath is from target to our source, need to reverse it from source to target
        selectedChar.GetComponent<CharacterController>().CurrentPath.Reverse();

        selectedChar.GetComponent<CharacterController>().Path = selectedChar.GetComponent<CharacterController>().CurrentPath; 
        
        Debug.Log("The movement path has been generated: " + selectedChar.GetComponent<CharacterController>().Path.Count);
    }
    
    // In:  || Out: returns a set of nodes of the tile that the character is occupying
    private HashSet<Node> GetTileCharIsOccupying()
    {
        int x = selectedChar.GetComponent<CharacterController>().x;
        int y = selectedChar.GetComponent<CharacterController>().y;
        HashSet<Node> charTile = new HashSet<Node>();
        charTile.Add(_map.Graph[x, y]);
        return charTile;
    }
    
    // In:  || Out: returns the hashset of nodes that the character can reach from its position
    private HashSet<Node> GetCharMovementOptions()
    {
        float[,] cost = new float[_map.mapSizeX, _map.mapSizeY];
        
        HashSet<Node> uiHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();

        int moveSpeed = selectedChar.GetComponent<CharacterController>().moveSpeed;

        Node charInitialNode = _map.Graph[selectedChar.GetComponent<CharacterController>().x,
            selectedChar.GetComponent<CharacterController>().y];
        
        // Setup the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(charInitialNode);
        foreach (Node n in charInitialNode.Neighbours)
        {
            cost[n.X, n.Y] = CostToEnterTile(n.X, n.Y);
            if (moveSpeed - cost[n.X, n.Y] >= 0)
            {
                uiHighlight.Add(n);
            }
        }
        finalMovementHighlight.UnionWith(uiHighlight);

        while (uiHighlight.Count != 0) 
        {
            foreach (Node n in uiHighlight)
            {
                foreach (Node neighbour in n.Neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        cost[neighbour.X, neighbour.Y] = CostToEnterTile(neighbour.X, neighbour.Y) + cost[n.X, n.Y];
                        if (moveSpeed - cost[neighbour.X, neighbour.Y] >= 0)
                        {
                            tempUIHighlight.Add(neighbour); 
                        }
                    }
                }
            }
            uiHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(uiHighlight);
            tempUIHighlight = new HashSet<Node>();
        }

        Debug.Log("The total amount of movable space for this character is: " + finalMovementHighlight.Count);
        return finalMovementHighlight;
    }
    
    // In:  || Out: returns a set of nodes that are all the attackable tiles from the char's current position
    private HashSet<Node> GetCharAttackOptionsFromPos()
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node initialNode = _map.Graph[selectedChar.GetComponent<CharacterController>().x,
            selectedChar.GetComponent<CharacterController>().y];
        int attRange = selectedChar.GetComponent<CharacterController>().attackRange;

        neighbourHash = new HashSet<Node>();
        neighbourHash.Add(initialNode);
        for (int i = 0; i < attRange; i++)
        {
            foreach (Node t in neighbourHash)
            {
                foreach (Node tn in t.Neighbours)
                {
                    tempNeighbourHash.Add(tn);
                }
            }
            neighbourHash = tempNeighbourHash;
            tempNeighbourHash = new HashSet<Node>();
            if (i < attRange - 1)
            {
                seenNodes.UnionWith(neighbourHash);
            }
        }
        neighbourHash.ExceptWith(seenNodes);
        neighbourHash.Remove(initialNode);

        return neighbourHash;
    }
    
    // In: finalMovement highlight, the attack range of the char, initial node that the char was standing on
    // Out: returns a set of nodes that represent the char's total attackable tiles
    private HashSet<Node> GetCharTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node charInitialNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();

        foreach (Node n in finalMovementHighlight)
        {
            neighbourHash = new HashSet<Node>();
            neighbourHash.Add(n);
            for (int i = 0; i < attRange; i++)
            {
                foreach (Node t in neighbourHash)
                {
                    foreach (Node tn in t.Neighbours)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }
                neighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                if (i < attRange - 1)
                {
                    seenNodes.UnionWith(neighbourHash);
                }
            }
            neighbourHash.ExceptWith(seenNodes);
            tempNeighbourHash = new HashSet<Node>();
            totalAttackableTiles.UnionWith(neighbourHash);
        }

        totalAttackableTiles.Remove(charInitialNode);

        return totalAttackableTiles;
    }
    
    // In: finalMovmentHighlight, totalAttackableTiles
    // Out: returns a hashset combination of two inputs
    private HashSet<Node> GetCharTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> unionTiles = new HashSet<Node>();
        unionTiles.UnionWith(finalMovementHighlight);
        unionTiles.UnionWith(totalAttackableTiles);

        return unionTiles;
    }
    
    // Highlights the selected char's options
    public void HighlightTileCharIsOccupying()
    {
        if (selectedChar != null)
        {
            HighlightMovementRange(GetTileCharIsOccupying()); 
        }
    }
    
    // Highlights the selected char's movement range to visualise
    private void HighlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            _map.QuadOnMap[n.X, n.Y].GetComponent<Renderer>().material = moveRangeUIMat;
            _map.QuadOnMap[n.X, n.Y].GetComponent<MeshRenderer>().enabled = true;
        }
    }
    
    // Highlights char's range options
    private void HighlightCharRange()
    {
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyInMoveRange = new HashSet<Node>();
        
        int attRange = selectedChar.GetComponent<CharacterController>().attackRange;
        int moveSpeed = selectedChar.GetComponent<CharacterController>().moveSpeed;

        Node charInitialNode = _map.Graph[selectedChar.GetComponent<CharacterController>().x,
            selectedChar.GetComponent<CharacterController>().y];
        finalMovementHighlight = GetCharMovementOptions();
        totalAttackableTiles = GetCharTotalAttackableTiles(finalMovementHighlight, attRange, charInitialNode);

        foreach (Node n in totalAttackableTiles)
        {
            if (_map.TilesOnMap[n.X, n.Y].GetComponent<Tile>().charOnTile != null)
            {
                GameObject charOnCurrSelectedTile = _map.TilesOnMap[n.X, n.Y].GetComponent<Tile>().charOnTile;
                if (charOnCurrSelectedTile.GetComponent<CharacterController>().teamNo !=
                    selectedChar.GetComponent<CharacterController>().teamNo)
                {
                    finalEnemyInMoveRange.Add(n);
                }
            }
        }
        
        HighlightEnemiesInRange(totalAttackableTiles);
        HighlightMovementRange(finalMovementHighlight);

        SelectedCharMoveRange = finalMovementHighlight;
        SelectedCharTotalRange = GetCharTotalRange(finalMovementHighlight, totalAttackableTiles);
    }
    
    // Highlights the selected char's attackOptions from its position
    public void HighlightCharAttackOptionsFromPos()
    {
        if (selectedChar != null)
        {
            HighlightEnemiesInRange(GetCharAttackOptionsFromPos());
        }
    }

    // Highlights the enemies in range once they've been added to a hashset 
    private void HighlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            _map.QuadOnMap[n.X, n.Y].GetComponent<Renderer>().material = attackRangeUIMat;
            _map.QuadOnMap[n.X, n.Y].GetComponent<MeshRenderer>().enabled = true;
        }
    }
    
    // de-selects the char
    private void DeselectChar()
    {
        if (selectedChar != null)
        {
            if (selectedChar.GetComponent<CharacterController>().charMoveState ==
                selectedChar.GetComponent<CharacterController>().GetMovementStates(1))
            {
                DisableHighlightCharRange();
                DisableCharUIRoute();
                selectedChar.GetComponent<CharacterController>().SetMovementStates(0);

                selectedChar = null;
                charSelected = false;
            }
            else if (selectedChar.GetComponent<CharacterController>().charMoveState == selectedChar.GetComponent<CharacterController>().GetMovementStates(2))
            {
                DisableHighlightCharRange();
                DisableCharUIRoute();
                selectedChar.GetComponent<CharacterController>().SetMovementStates(0);

                selectedChar = null;
                charSelected = false;
            }
            else if (selectedChar.GetComponent<CharacterController>().charMoveState == selectedChar.GetComponent<CharacterController>().GetMovementStates(3))
            {
                DisableHighlightCharRange();
                DisableCharUIRoute();

                selectedChar = null;
                charSelected = false;
            }
            else
            {
                DisableHighlightCharRange();
                DisableCharUIRoute();
                _map.TilesOnMap[selectedChar.GetComponent<CharacterController>().x,
                    selectedChar.GetComponent<CharacterController>().y].GetComponent<Tile>().charOnTile = null;
                _map.TilesOnMap[charSelectedPrevX, charSelectedPrevY].GetComponent<Tile>().charOnTile = selectedChar;
            
                selectedChar.GetComponent<CharacterController>().x = charSelectedPrevX;
                selectedChar.GetComponent<CharacterController>().y = charSelectedPrevY;
                selectedChar.GetComponent<CharacterController>().tileBeingOccupied = prevOccupiedTile;
                selectedChar.transform.position = _map.TileCoordToWorldCoord(charSelectedPrevX, charSelectedPrevY);
                selectedChar.GetComponent<CharacterController>().SetMovementStates(0);
                selectedChar = null;
                charSelected = false;
            }
        }
    }
    
    // disables the highlight
    public void DisableHighlightCharRange()
    {
        foreach (GameObject quad in _map.QuadOnMap)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }
    
    // disables the quads that are being used to highlight position 
    public void DisableCharUIRoute()
    {
        foreach (GameObject quad in _map.QuadOnMapForCharMovement)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }
    
}
