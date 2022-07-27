using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapManager : MonoBehaviour
{
    private MapGenerator _map;
    public PlayerMovement player;
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

    //Raycast for the update for mouseHover 
    private Ray _ray;
    private RaycastHit _hit;

    [Header("Materials")] 
    public Material greenUIMat;
    public Material blueUIMat;
    public Material redUIMat;
    
    void Start()
    {
        _map = GetComponent<MapGenerator>();
        player = player.GetComponent<PlayerMovement>();
        _gameManager = GetComponent<GameManager>();
    }

    void Update()
    {
        // If input is left mouse down then selects the character
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedChar == null)
            {
                MouseClickToSelectChar();
            }
            
            // Once the char has been selected, need to check if the unit has entered the selection state (1) 'Selected'; if yes then move the unit
            else if (player.charMoveState == player.GetMovementStates(1) && player.MovementQueue.Count == 0)
            {
                if (SelectTileToMoveTo())
                {
                    Debug.Log("Movement path has been selected");
                    charSelectedPrevX = player.x;
                    charSelectedPrevY = player.y;
                    prevOccupiedTile = player.tileBeingOccupied;
                    _gameManager.MoveChar();

                    StartCoroutine(player.MoveCharAndFinalise()); 
                }
                // Finalise the movement
                else if(player.charMoveState == player.GetMovementStates(2))
                {
                    finaliseOption();
                }
            }
        }
        // Deselect the char with the right click
        if (Input.GetMouseButtonDown(1))
        {
            if (selectedChar != null)
            {
                if (selectedChar.GetComponent<PlayerMovement>().MovementQueue.Count == 0)
                {
                    if (selectedChar.GetComponent<PlayerMovement>().charMoveState !=
                        selectedChar.GetComponent<PlayerMovement>().GetMovementStates(3))
                    {
                        DeselectChar();
                    }
                }
                else if (selectedChar.GetComponent<PlayerMovement>().MovementQueue.Count == 1)
                {
                    selectedChar.GetComponent<PlayerMovement>().visualMoveSpeed = 0.5f;
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

                if (tempSelectedChar.GetComponent<PlayerMovement>().charMoveState == tempSelectedChar
                        .GetComponent<PlayerMovement>().GetComponent<PlayerMovement>().GetMovementStates(0))
                {
                    selectedChar = tempSelectedChar;
                    selectedChar.GetComponent<PlayerMovement>().map = _map;
                    selectedChar.GetComponent<PlayerMovement>().SetMovementStates(1);
                    charSelected = true;
                    HighlightCharRange();
                }
            }
        }
    }
    
    // Checks if the tile that has been clicked is movable for the selected char
    public bool SelectTileToMoveTo()
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
            if (hit.transform.parent.GetComponent<PlayerMovement>().teamNo !=
                selectedChar.GetComponent<PlayerMovement>().teamNo)
            {
                Debug.Log("Clicked an enemy");
            }
            else if (hit.transform.parent.gameObject == selectedChar)
            {
                GeneratePathTo(selectedChar.GetComponent<PlayerMovement>().x, selectedChar.GetComponent<PlayerMovement>().y);
                
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
                int charX = charOnTeam.GetComponent<PlayerMovement>().x;
                int charY = charOnTeam.GetComponent<PlayerMovement>().y;
                charOnTeam.GetComponent<PlayerMovement>().tileBeingOccupied = _map.TilesOnMap[charX, charY];
                _map.TilesOnMap[charX, charY].GetComponent<Tile>().charOnTile = charOnTeam.gameObject;
            }
        }
    }
    
    // If the tile isn't occupied by another team & if the tile is walkable then you can walk through 
    public bool CharCanEnterTile(int x, int y)
    {
        if (_map.TilesOnMap[x, y].GetComponent<Tile>().charOnTile != null)
        {
            if (_map.TilesOnMap[x, y].GetComponent<Tile>().charOnTile.GetComponent<PlayerMovement>().teamNo !=
                selectedChar.GetComponent<PlayerMovement>().teamNo)
            {
                return false;
            }
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
        if (selectedChar.GetComponent<PlayerMovement>().x == x &&
            selectedChar.GetComponent<PlayerMovement>().y == y)
        {
            Debug.Log("Clicked the same tile that the character currently standing on"); 
        }

        selectedChar.GetComponent<PlayerMovement>().Path = null;
        player.CurrentPath = null; 
        
        // Path finding algorithm
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = _map.Graph[selectedChar.GetComponent<PlayerMovement>().x,
            selectedChar.GetComponent<PlayerMovement>().y];
        Node target = _map.Graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        List<Node> unvisited = new List<Node>(); // unchecked Nodes
        
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
                break;

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

        player.CurrentPath = new List<Node>();
        Node curr = target;
        
        // Step through the current path and add it to the chain
        while (curr != null)
        {
            player.CurrentPath.Add(curr);
            curr = prev[curr];
        }
        
        // Currently currPath is from target to our source, need to reverse it from source to target
        player.CurrentPath.Reverse();

        selectedChar.GetComponent<PlayerMovement>().Path = player.CurrentPath; 
    }
    
    // In:  || Out: returns a set of nodes of the tile that the character is occupying
    public HashSet<Node> GetTileCharIsOccupying()
    {
        int x = selectedChar.GetComponent<PlayerMovement>().x;
        int y = selectedChar.GetComponent<PlayerMovement>().y;
        HashSet<Node> charTile = new HashSet<Node>();
        charTile.Add(_map.Graph[x, y]);
        return charTile;
    }
    
    // In:  || Out: returns the hashset of nodes that the character can reach from its position
    public HashSet<Node> GetCharMovementOptions()
    {
        float[,] cost = new float[_map.mapSizeX, _map.mapSizeY];
        
        HashSet<Node> uiHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();

        int moveSpeed = selectedChar.GetComponent<PlayerMovement>().moveSpeed;

        Node charInitialNode = _map.Graph[selectedChar.GetComponent<PlayerMovement>().x,
            selectedChar.GetComponent<PlayerMovement>().y];
        
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

                uiHighlight = tempUIHighlight;
                finalMovementHighlight.UnionWith(uiHighlight);
                tempUIHighlight = new HashSet<Node>();
            }
        }

        Debug.Log("The total amount of movable space for this character is: " + finalMovementHighlight.Count);
        return finalMovementHighlight;
    }
    
    // In:  || Out: returns a set of nodes that are all the attackable tiles from the char's current position
    public HashSet<Node> GetCharAttackOptionsFromPos()
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node initialNode = _map.Graph[selectedChar.GetComponent<PlayerMovement>().x,
            selectedChar.GetComponent<PlayerMovement>().y];
        int attRange = selectedChar.GetComponent<PlayerMovement>().attackRange;

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
    public HashSet<Node> GetCharTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node charInitialNode)
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
    public HashSet<Node> GetCharTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
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
    public void HighlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            _map.QuadOnMap[n.X, n.Y].GetComponent<Renderer>().material = blueUIMat;
            _map.QuadOnMap[n.X, n.Y].GetComponent<MeshRenderer>().enabled = true;
        }
    }
    
    // Highlights char's range options
    public void HighlightCharRange()
    {
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyInMoveRange = new HashSet<Node>();
        
        int attRange = selectedChar.GetComponent<PlayerMovement>().attackRange;
        int moveSpeed = selectedChar.GetComponent<PlayerMovement>().moveSpeed;

        Node charInitialNode = _map.Graph[selectedChar.GetComponent<PlayerMovement>().x,
            selectedChar.GetComponent<PlayerMovement>().y];
        finalMovementHighlight = GetCharMovementOptions();
        totalAttackableTiles = GetCharTotalAttackableTiles(finalMovementHighlight, attRange, charInitialNode);

        foreach (Node n in totalAttackableTiles)
        {
            if (_map.TilesOnMap[n.X, n.Y].GetComponent<Tile>().charOnTile != null)
            {
                GameObject charOnCurrSelectedTile = _map.TilesOnMap[n.X, n.Y].GetComponent<Tile>().charOnTile;
                if (charOnCurrSelectedTile.GetComponent<PlayerMovement>().teamNo !=
                    player.GetComponent<PlayerMovement>().teamNo)
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
    public void HighlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            _map.QuadOnMap[n.X, n.Y].GetComponent<Renderer>().material = redUIMat;
            _map.QuadOnMap[n.X, n.Y].GetComponent<MeshRenderer>().enabled = true;

        }
    }
    
    // de-selects the char
    public void DeselectChar()
    {
        if (selectedChar != null)
        {
            if (selectedChar.GetComponent<PlayerMovement>().charMoveState ==
                selectedChar.GetComponent<PlayerMovement>().GetMovementStates(1))
            {
                DisableHighlightCharRange();
                DisableCharUIRoute();
                selectedChar.GetComponent<PlayerMovement>().SetMovementStates(0);

                selectedChar = null;
                charSelected = false;
            }
            else if (selectedChar.GetComponent<PlayerMovement>().charMoveState == selectedChar.GetComponent<PlayerMovement>().GetMovementStates(3))
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
                _map.TilesOnMap[selectedChar.GetComponent<PlayerMovement>().x,
                    selectedChar.GetComponent<PlayerMovement>().y].GetComponent<Tile>().charOnTile = null;
                _map.TilesOnMap[charSelectedPrevX, charSelectedPrevY].GetComponent<Tile>().charOnTile = selectedChar;

                selectedChar.GetComponent<PlayerMovement>().x = charSelectedPrevX;
                selectedChar.GetComponent<PlayerMovement>().y = charSelectedPrevY;
                selectedChar.GetComponent<PlayerMovement>().tileBeingOccupied = prevOccupiedTile;
                selectedChar.transform.position = _map.TileCoordToWorldCoord(charSelectedPrevX, charSelectedPrevY);
                selectedChar.GetComponent<PlayerMovement>().SetMovementStates(0);
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
    
    // de-selects the selected char after the action has been taken
    public IEnumerator DeselectAfterMovements(GameObject charPlayer, GameObject charEnemy) 
    {
        selectedChar.GetComponent<PlayerMovement>().SetMovementStates(3);
        DisableHighlightCharRange();
        DisableCharUIRoute();

        yield return new WaitForSeconds(.25f);

        while (charPlayer.GetComponent<PlayerMovement>().CombatQueue.Count > 0) 
        {
            yield return new WaitForEndOfFrame();
        }

        while (charEnemy.GetComponent<PlayerMovement>().CombatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        DeselectChar();
    }
    
    // Finalises the player's option 
    public void finaliseOption()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = GetCharAttackOptionsFromPos();

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<Tile>().charOnTile != null)
                {
                    GameObject charOnTile = hit.transform.GetComponent<Tile>().charOnTile;
                    int charX = charOnTile.GetComponent<PlayerMovement>().x;
                    int charY = charOnTile.GetComponent<PlayerMovement>().y;

                    if (charOnTile == selectedChar)
                    {
                        DisableHighlightCharRange();
                        Debug.Log("It's the same character, just wait");
                        selectedChar.GetComponent<PlayerMovement>().SetMovementStates(3);
                        DeselectChar();
                    }
                    else if (charOnTile.GetComponent<PlayerMovement>().teamNo !=
                             selectedChar.GetComponent<PlayerMovement>().teamNo &&
                             attackableTiles.Contains(_map.Graph[charX, charY]))
                    {
                        if (charOnTile.GetComponent<PlayerMovement>().currHeathPoints > 0)
                        {
                            Debug.Log("Clicked an enemy that should be attacked");

                            StartCoroutine(DeselectAfterMovements(selectedChar, charOnTile));
                        }
                    }
                }
            }
            else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Player"))
            {
                GameObject charClicked = hit.transform.parent.gameObject;
                int charX = charClicked.GetComponent<PlayerMovement>().x;
                int charY = charClicked.GetComponent<PlayerMovement>().y;

                if (charClicked == selectedChar)
                {
                    DisableHighlightCharRange();
                    Debug.Log("It's the same unit, just wait"); 
                    selectedChar.GetComponent<PlayerMovement>().SetMovementStates(3);
                    DeselectChar();
                }
                else if (charClicked.GetComponent<PlayerMovement>().teamNo !=
                         selectedChar.GetComponent<PlayerMovement>().teamNo &&
                         attackableTiles.Contains(_map.Graph[charX, charY]))
                {
                    if (charClicked.GetComponent<PlayerMovement>().currHeathPoints > 0)
                    {
                        Debug.Log("Clicked an enemy that should be attacked");

                        StartCoroutine(DeselectAfterMovements(selectedChar, charClicked));
                    }
                }
            }
        }
    }
}
