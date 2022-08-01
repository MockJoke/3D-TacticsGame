using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterController
{
    public PlayerController()
    {
        this.teamNo = Team.Player; 
        this.charName = "Player"; 
        this.moveSpeed = 5;
        this.attackRange = 1;
        this.attackDamage = 1;
        this.maxHealthPoints = 10;
    }

    public override void MoveToNextTile()
    {
        base.MoveToNextTile();
    }
}
