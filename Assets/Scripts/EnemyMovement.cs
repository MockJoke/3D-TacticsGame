using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : CharacterController, IAIInterface
{
    public GameObject target; 
    
    public GameObject player; 
    void Start()
    {
         
    }

    public override void MoveNextTile()
    {
        
    }
    
    public void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject t in targets)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);

            if (dist < distance)
            {
                distance = dist;
                nearest = t;
            }
        }
        target = nearest;
    }
}
