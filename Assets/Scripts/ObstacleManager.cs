using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager instance;

    [SerializeField] private GameObject map;
    private MapGenerator _mapGenerator;
    private MapManager _mapManager;
    private GameManager _gameManager;
    [SerializeField] private ObstacleSO obstacleSo;
    public GameObject ObstaclesContainer;
    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _mapGenerator = map.GetComponent<MapGenerator>();
        _mapManager = map.GetComponent<MapManager>();
        _gameManager = map.GetComponent<GameManager>();
    }

    public void GenerateObstacle(int posX, int posY)
    {
        if (_mapGenerator.TilesOnMap[posX, posY].GetComponent<Tile>().isTileOccupied)
        {
            Debug.Log("Someone is standing there, can't create obstacle");
        }
        else
        {
            Destroy(_mapGenerator.TilesOnMap[posX, posY]);
            
            GameObject newObstacle =
                Instantiate(obstacleSo.obstacleType, new Vector3(posX, 0, posY), Quaternion.identity);
            //obstacleSo.obstacleType = _mapGenerator.tileTypes[3].tilePrefab;
            //GameObject newObstacle =
                    //Instantiate(obstacleSo.obstacleType, new Vector3(posX, 0, posY), Quaternion.identity);
            newObstacle.GetComponent<Tile>().tileX = posX;
            newObstacle.GetComponent<Tile>().tileY = posY;
            newObstacle.GetComponent<Tile>().map = _mapGenerator;
            newObstacle.transform.SetParent(ObstaclesContainer.transform);
            _mapGenerator.TilesOnMap[posX, posY] = newObstacle;
            _mapGenerator.Tiles[posX, posY] = 3; 
        }
    }
}
