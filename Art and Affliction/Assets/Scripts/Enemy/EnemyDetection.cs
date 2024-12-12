using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("DetectedPlayer");
            Enemy enemy = GetComponentInParent<Enemy>();
            enemy.isPatrolling = false;
            enemy.isIdle = false;
            enemy.isAttacking = true;
        }
    }
}
