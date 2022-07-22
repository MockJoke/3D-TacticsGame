using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private MapGenerator map;
    private PlayerMovement player; 
    
    public GameObject tileBeingDisplayed;
    
    // Cursor Info
    public int cursorX;
    public int cursorY;
    // Current tile being moused over
    public int selectedXTile;
    public int selectedYTile;
    
    //Raycast for the update for mouseHover 
    private Ray ray;
    private RaycastHit hit;

    void Start()
    {
        map = GetComponent<MapGenerator>();
        player = GetComponent<PlayerMovement>(); 
    }

    void Update()
    {
        // Always trying to see where the mouse is pointing 
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            cursorUIUpdate();
        }
    }

    // Updates the cursor for the UI
    private void cursorUIUpdate()
    {
        // If hovering mouse over a tile, highlight it
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject; 
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileX;
                selectedYTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileY;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
            }
        }
        // If hovering mouse over a character, highlight a tile that the character is occupying
        else if (hit.transform.CompareTag("Player"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileX;
                selectedYTile = tileBeingDisplayed.transform.GetComponent<Tile>().tileY;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = hit.transform.GetComponent<Tile>().tileX;
                selectedYTile = hit.transform.GetComponent<Tile>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
            }
        }
        // If not pointing at anything
        else
        {
            map.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }
    
    // Selects a character based on the cursor click
    public void mouseCLickToSelectChar()
    {
        if (player.charSelected == false && tileBeingDisplayed != null)
        {
            if (tileBeingDisplayed.GetComponent<Tile>().charOnTile != null)
            {
                GameObject tempSelectedChar = tileBeingDisplayed.GetComponent<Tile>().charOnTile;

                if (tempSelectedChar.GetComponent<PlayerMovement>().characterMoveState == tempSelectedChar
                        .GetComponent<PlayerMovement>().GetComponent<PlayerMovement>().getMovementStates(0))
                {
                    player.selectedChar = tempSelectedChar;
                    player.selectedChar.GetComponent<PlayerMovement>().map = map;
                    player.selectedChar.GetComponent<PlayerMovement>().setMovementStates(1);
                    player.charSelected = true;
                }
            }
        }
    }
    
    // If the tile isn't occupied by another team & if the tile is walkable then you can walk through 
    public bool charCanEnterTile(int x, int y)
    {
        if (map.tilesOnMap[x, y].GetComponent<Tile>().charOnTile != null)
        {
            if (map.tilesOnMap[x, y].GetComponent<Tile>().charOnTile.GetComponent<PlayerMovement>().teamNo !=
                player.selectedChar.GetComponent<PlayerMovement>().teamNo)
            {
                return false;
            }
        }
        return map.tileTypes[map.tiles[x, y]].isWalkable;
    }
    // checks the cost of a tile for a unit to enter 
    public float costToEnterTile(int x, int y)
    {
        if (charCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;
        }

        TileType t = map.tileTypes[map.tiles[x, y]];
        float dist = t.movementCost;

        return dist; 
    }
    
    // Generates path for the selected character
    public void generatePathTo(int x, int y)
    {
        if (player.selectedChar.GetComponent<PlayerMovement>().x == x &&
            player.selectedChar.GetComponent<PlayerMovement>().y == y)
        {
            Debug.Log("Clicked the same tile that the character currently standing on"); 
        }

        player.selectedChar.GetComponent<PlayerMovement>().path = null;
        player.currentPath = null; 
        
        // Path finding algorithm
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = map.Graph[player.selectedChar.GetComponent<PlayerMovement>().x,
            player.selectedChar.GetComponent<PlayerMovement>().y];
        Node target = map.Graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        List<Node> unvisited = new List<Node>(); // unchecked Nodes
        
        // Initialise 
        foreach (Node n in map.Graph)
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
                float alt = dist[u] + costToEnterTile(n.x, n.y);
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

        player.currentPath = new List<Node>();
        Node curr = target;
        
        // Step through the current path and add it to the chain
        while (curr != null)
        {
            player.currentPath.Add(curr);
            curr = prev[curr];
        }
        
        // Currently currPath is from target to our source, need to reverse it from source to target
        player.currentPath.Reverse();

        player.selectedChar.GetComponent<PlayerMovement>().path = player.currentPath; 
    }
}
