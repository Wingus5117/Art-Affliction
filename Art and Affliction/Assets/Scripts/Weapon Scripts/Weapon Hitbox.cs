using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private Enemy enemy;
    public float HitBoxDamageValue;
    private float RandomAttackID;


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponentInParent<Enemy>() != null && collision.gameObject.tag == "EnemyHurtbox")
        {
            
            enemy = collision.gameObject.GetComponentInParent<Enemy>();
            enemy.CheckAttackID(HitBoxDamageValue, RandomAttackID);
            PlayerCombatManager playerCombatManager = GetComponentInParent<PlayerCombatManager>();
            if (enemy.ValidHit)
            {
                playerCombatManager.DealDamage(enemy, HitBoxDamageValue);
            }
            enemy = null;
            
        }
    }
    private void GenerateNewAttackID()
    {
        //Debug.Log("Regenerate ID"); 
        RandomAttackID = Random.Range(1, 1000);
    }
    private void OnEnable()
    {
        GenerateNewAttackID();
    }
}
