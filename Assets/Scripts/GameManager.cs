using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
    public GameObject map;
    private MapGenerator _mapGenerator;
    private MapManager _mapManager;
    public PlayerMovement player;
    
    // Raycast for the update of charHover info
    private Ray _ray;
    private RaycastHit _hit;
    
    // Cursor Info
    public int cursorX;
    public int cursorY;
    // Current tile being moused over
    public int selectedXTile;
    public int selectedYTile;
    
    public GameObject tileBeingDisplayed;

    public bool charPathExists;
    
    // var for charPotentialMovementRoute
    private List<Node> currPathForCharRoute;
    private List<Node> charPathToCursor;

    [Header("Materials")] 
    public Material UICharRoute;
    public Material UICharRouteCurve;
    public Material UICharRouteArrow;
    public Material UICursor;

    public int routeToX;
    public int routeToY;

    void Start()
    {
       _mapManager = map.GetComponent<MapManager>();
       _mapGenerator = map.GetComponent<MapGenerator>();
       player = player.GetComponent<PlayerMovement>();

       charPathToCursor = new List<Node>();
       charPathExists = false;
    }

    void Update()
    {
       // Always trying to see where the mouse is pointing 
       _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
       if (Physics.Raycast(_ray, out _hit))
       {
          CursorUIUpdate();
          
          // If the char is selected, highlight the current path with the UI
          if (_mapManager.selectedChar != null &&
              _mapManager.selectedChar.GetComponent<PlayerMovement>().GetMovementStates(1) ==
              _mapManager.selectedChar.GetComponent<PlayerMovement>().charMoveState)
          {
              // check if the cursor is in range, can't show movement outside the range 
              if (_mapManager.SelectedCharMoveRange.Contains(_mapGenerator.Graph[cursorX, cursorY]))
              {
                  if (cursorX != _mapManager.selectedChar.GetComponent<PlayerMovement>().x ||
                      cursorY != _mapManager.selectedChar.GetComponent<PlayerMovement>().y)
                  {
                      if (!charPathExists &&
                          _mapManager.selectedChar.GetComponent<PlayerMovement>().MovementQueue.Count == 0)
                      {
                          charPathToCursor = GenerateCursorRouteTo(cursorX, cursorY);

                          routeToX = cursorX;
                          routeToY = cursorY;

                          if (charPathToCursor.Count != 0)
                          {
                              for (int i = 0; i < charPathToCursor.Count; i++)
                              {
                                  int nodeX = charPathToCursor[i].X;
                                  int nodeY = charPathToCursor[i].Y;
                                  
                                  if(i == 0)
                                  {
                                      GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
                                      quadToUpdate.GetComponent<Renderer>().material = UICursor;
                                  }
                                  else if((i+1) != charPathToCursor.Count)
                                  {
                                      // Set the indicator for tiles excluding the first/last tile
                                      SetRouteWithIO(nodeX, nodeY, i);
                                  }
                                  else if (i == charPathToCursor.Count - 1)
                                  {
                                      // Set the indicator for final tile
                                      SetRouteFinalTile(nodeX, nodeY, i);
                                  }

                                  _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY].GetComponent<Renderer>()
                                      .enabled = true;
                              }
                          }
                          charPathExists = true;
                      }
                      else if (routeToX != cursorX || routeToY != cursorY)
                      {
                          if (charPathToCursor.Count != 0)
                          {
                              for (int i = 0; i < charPathToCursor.Count; i++)
                              {
                                  int nodeX = charPathToCursor[i].X;
                                  int nodeY = charPathToCursor[i].Y;

                                  _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY].GetComponent<Renderer>()
                                      .enabled = false;
                              }
                          }
                          charPathExists = false; 
                      }
                      else if (cursorX == _mapManager.selectedChar.GetComponent<PlayerMovement>().x &&
                               cursorY == _mapManager.selectedChar.GetComponent<PlayerMovement>().y)
                      {
                          _mapManager.DisableCharUIRoute();
                          charPathExists = false;
                      }
                  } 
              }
          }
       }
    }
   
    // Updates the cursor for the UI
    private void CursorUIUpdate() 
    { 
        // If hovering mouse over a tile, highlight it
        if (_hit.transform.CompareTag("Tile")) 
        { 
            if (tileBeingDisplayed == null) 
            { 
                selectedXTile = _hit.transform.GetComponent<Tile>().tileX; 
                selectedYTile = _hit.transform.GetComponent<Tile>().tileY; 
                cursorX = selectedXTile; 
                cursorY = selectedYTile; 
                _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true; 
                tileBeingDisplayed = _hit.transform.gameObject; 
            }
            else if (tileBeingDisplayed != _hit.transform.gameObject) 
            { 
                selectedXTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileX; 
                selectedYTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileY; 
                _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
                
                selectedXTile = _hit.transform.GetComponent<Tile>().tileX; 
                selectedYTile = _hit.transform.GetComponent<Tile>().tileY; 
                cursorX = selectedXTile; 
                cursorY = selectedYTile; 
                _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true; 
                tileBeingDisplayed = _hit.transform.gameObject;
            }
        } 
        // If hovering mouse over a character, highlight a tile that the character is occupying
        else if (_hit.transform.CompareTag("Player")) 
        { 
            if (tileBeingDisplayed == null) 
            { 
                selectedXTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().x; 
                selectedYTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().y; 
                cursorX = selectedXTile; 
                cursorY = selectedYTile; 
                _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true; 
                tileBeingDisplayed = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().tileBeingOccupied;
            }
            else if (tileBeingDisplayed != _hit.transform.gameObject)
            {
                if (_hit.transform.parent.gameObject.GetComponent<PlayerMovement>().MovementQueue.Count == 0)
                {
                    selectedXTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileX;
                    selectedYTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileY;
                    _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
 
                    selectedXTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().x; 
                    selectedYTile = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().y;
                    cursorX = selectedXTile;
                    cursorY = selectedYTile;
                    _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                    tileBeingDisplayed = _hit.transform.parent.gameObject.GetComponent<PlayerMovement>().tileBeingOccupied;
                }
            }
        }
        // If not pointing at anything
        else
        { 
            _mapGenerator.QuadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }
    
    // In: x & y location to go to
    // Out: List of nodes to traverse 
    // generate the cursor route to a position x, y
    public List<Node> GenerateCursorRouteTo(int x, int y)
    {
        if (_mapManager.selectedChar.GetComponent<PlayerMovement>().x == x &&
            _mapManager.selectedChar.GetComponent<PlayerMovement>().y == y)
        {
            Debug.Log("Clicked the same tile as the character is standing on");
            currPathForCharRoute = new List<Node>();

            return currPathForCharRoute;
        }
        // can't move into something so just return
        if (_mapManager.CharCanEnterTile(x, y) == false)
        {  
            return null;
        }

        currPathForCharRoute = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = _mapGenerator.Graph[_mapManager.selectedChar.GetComponent<PlayerMovement>().x,
            _mapManager.selectedChar.GetComponent<PlayerMovement>().y];
        Node target = _mapGenerator.Graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        // Unchecked nodes
        List<Node> unvisited = new List<Node>();
        
        // Initialise 
        foreach (Node n in _mapGenerator.Graph)
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
                float alt = dist[u] + _mapManager.CostToEnterTile(n.X, n.Y);
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
            return null;    
        }

        currPathForCharRoute = new List<Node>();    
        Node curr = target;
        
        // Step through the current path and add it to the chain
        while (curr != null)
        {
            currPathForCharRoute.Add(curr);
            curr = prev[curr];
        }
        
        // Currently currPath is from target to our source, need to reverse it from source to target
        currPathForCharRoute.Reverse();

        return currPathForCharRoute;
    }

    // In: two gameobjects curr vector & next one in the list
    // Out: vector which is the dir bw the two inputs
    // the dir from curr to the next vector is returned
    public Vector2 DirectionBetween(Vector2 currVector, Vector2 nextVector)
    {
        Vector2 vectorDir = (nextVector - currVector).normalized;

        if (vectorDir == Vector2.right)
            return Vector2.right;
        else if(vectorDir == Vector2.left)
            return Vector2.left;
        else if(vectorDir == Vector2.up)
            return Vector2.up;
        else if (vectorDir == Vector2.down)
            return Vector2.down;
        else 
        {
            Vector2 vectorToReturn = new Vector2();
            return vectorToReturn;
        }
    } 
    
    // In: two nodes that are being checked and i is the pos in the path 
    // Out: void 
    // orients the quads to display proper info
    public void SetRouteWithIO(int nodeX, int nodeY, int i)
    {
        Vector2 prevTile = new Vector2(charPathToCursor[i - 1].X + 1, charPathToCursor[i - 1].Y + 1); 
        Vector2 currTile = new Vector2(charPathToCursor[i].X + 1, charPathToCursor[i].Y + 1);
        Vector2 nextTile = new Vector2(charPathToCursor[i + 1].X + 1, charPathToCursor[i + 1].Y + 1);

        Vector2 backToCurrVector = DirectionBetween(prevTile, currTile);
        Vector2 currToFrontVector = DirectionBetween(currTile, nextTile);
        
        // Right to [Right/Up/Down]
        if (backToCurrVector == Vector2.right && currToFrontVector == Vector2.right)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UICharRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.right && currToFrontVector == Vector2.up)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.right && currToFrontVector == Vector2.down)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        // Left to [Left/Up/Down]
        else if (backToCurrVector == Vector2.left && currToFrontVector == Vector2.left)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UICharRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.left && currToFrontVector == Vector2.up)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.left && currToFrontVector == Vector2.down)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        // Up to [Up/Right/Left]
        else if (backToCurrVector == Vector2.up && currToFrontVector == Vector2.up)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UICharRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.up && currToFrontVector == Vector2.right)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.up && currToFrontVector == Vector2.left)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        // Down to [Down/Right/Left]
        else if (backToCurrVector == Vector2.down && currToFrontVector == Vector2.down)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UICharRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.down && currToFrontVector == Vector2.right)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.down && currToFrontVector == Vector2.left)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }
    
    // In: two nodes that are being checked and i is the pos in the path
    // Out: void
    // orients the quad for the final node in the list to display proper info
    public void SetRouteFinalTile(int nodeX, int nodeY, int i)
    {
        Vector2 prevTile = new Vector2(charPathToCursor[i - 1].X + 1, charPathToCursor[i - 1].Y + 1); 
        Vector2 currTile = new Vector2(charPathToCursor[i].X + 1, charPathToCursor[i].Y + 1);
        Vector2 backToCurrVector = DirectionBetween(prevTile, currTile);

        if (backToCurrVector == Vector2.right)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.left)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.up)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrVector == Vector2.down)
        {
            GameObject quadToUpdate = _mapGenerator.QuadOnMapForCharMovement[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UICharRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }
    // Moves the char
    public void MoveChar()
    {
       if (_mapManager.selectedChar != null)
       {
          _mapManager.selectedChar.GetComponent<PlayerMovement>().MoveNextTile();
       }
    }
}
