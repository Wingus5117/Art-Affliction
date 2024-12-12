using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyWeaponHitbox : MonoBehaviour
{
    private PlayerCombatManager player;
    private ActionManager actionManager;
    public float HitBoxDamageValue;
    public Action TakeDamageAction;
    private float RandomAttackID;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponentInParent<PlayerCombatManager>() != null)
        {
           
            player = collision.gameObject.GetComponentInParent<PlayerCombatManager>();
            actionManager = collision.gameObject.GetComponentInParent<ActionManager>();
            player.CheckAttackID(HitBoxDamageValue, RandomAttackID);
            if (player.ValidHit == true)
            {
                actionManager.AddAction(TakeDamageAction);
            }
            player = null;
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
