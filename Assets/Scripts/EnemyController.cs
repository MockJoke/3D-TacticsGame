using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CharacterController, IAIInterface
{
    public EnemyController()
    {
        this.teamNo = Team.Enemy;
        this.charName = "Enemy"; 
        this.moveSpeed = 100;
        this.attackRange = 1;
        this.attackDamage = 1;
        this.maxHealthPoints = 5;
    }

    public PlayerController player;
    public int targetX;
    public int targetY;
    void Start()
    {
        player = player.GetComponent<PlayerController>();

        _mapManager = map.GetComponent<MapManager>();
    }
    
    public void FindTargetToMoveTo()
    {
        targetX = player.x;
        targetY = player.y;
        
        _mapManager.GeneratePathTo(targetX, targetY);
    }

    public override void MoveToNextTile()
    {
        if (Path.Count == 0)
        {
            return;
        }
       
        StartCoroutine(MoveOverSeconds(transform.gameObject, Path[Path.Count - 2]));
    }

    protected override IEnumerator MoveOverSeconds(GameObject objectToMove, Node endNode)
    {
        MovementQueue.Enqueue(1);
        
        // Remove the first thing on path because its the tile we're standing on 
        Path.RemoveAt(0);

        while (Path.Count -1 != 0)
        {
            Vector3 endPos = _mapGenerator.TileCoordToWorldCoord(Path[0].X, Path[0].Y);
            objectToMove.transform.position = Vector3.Lerp(transform.position, endPos, visualMoveSpeed);
            if ((transform.position - endPos).sqrMagnitude < 0.001)
            {
                Path.RemoveAt(0);
            }
            yield return new WaitForEndOfFrame();
        }

        transform.position = _mapGenerator.TileCoordToWorldCoord(endNode.X, endNode.Y);

        x = endNode.X;
        y = endNode.Y;

        tileBeingOccupied.GetComponent<Tile>().charOnTile = null;
        tileBeingOccupied = _mapGenerator.TilesOnMap[x, y];
        MovementQueue.Dequeue();
    }
}
