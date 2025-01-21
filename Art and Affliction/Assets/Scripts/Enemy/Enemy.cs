using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    public bool isDead = false;
    public Transform EnemyLockOnPoint;
    public float CurrentHealth;
    public float MaxHealth;

    internal GameObject Player;

    internal float DistanceToPlayer;
    internal NavMeshAgent NavMeshAgent;
    internal bool IsInAttackRange = false;
    internal bool isFacingPlayer;
    internal bool tryToLookAtPlayer = false;
    public float AttackRange;
    public float Lookspeed = 2.5f;
    internal bool DoAttackStuff;
    public bool PlayerisInAttackBox;
    //Health
    public Image HealthBar;

    //States
    public bool isInCombat;
    public bool isIdle;
    public bool isPatrolling;
    internal bool SubState_isAttacking;


    //Animation
    public Animator Animator;
    private bool WeaponDrawPerformed;

    //Patrol
    public Transform[] PatrolPoints;
    internal bool patrolActive;
    //HitID Logic
    internal float LastHitRecivedID;
    internal bool ValidHit;

    //PlayerDetectionBox
    public Collider DetectionBox;

    //Boss Logic
    internal bool isBoss;
    public void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();

        //Animator = GetComponent<Animator>();

        PlayerManager player = FindObjectOfType<PlayerManager>();
        Player = player.gameObject;

        NavMeshAgent.updateRotation = false;
    }
    public void Update()
    {
        
        
        NewUpdate();
        if (CurrentHealth <= 0)
        {
            HealthBar.fillAmount = 0;
            isDead = true;
        }
        if (isDead)
        {
            Debug.Log("Dead");
            OnDeath();
            //Play Death Animation
            Destroy(gameObject, 3);
            return;
        }
        HealthBar.fillAmount = CurrentHealth / MaxHealth;
        ProcessEnemyAi();

        //If we should be rotating to look at the player
        if (tryToLookAtPlayer)
        {
            RotateEnemyOverTime();
        }
        
    }
    public void ProcessEnemyAi()
    {
        if (isIdle)
        {
            return;
        }
        if (isPatrolling)
        {
            NavMeshAgent.updateRotation = true;
            //Animator.SetBool("Idle", false);
            if (!patrolActive)
            {
                patrolActive = true;
                StartCoroutine(Patrolling());
            }
            return;
        }
        if (isInCombat)
        {
            NavMeshAgent.updateRotation = false;
        }
    }
    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (!isDead)
        {
            OnTakeDamage();
        }
    }
    public void MoveToPlayer()
    {
        NavMeshAgent.SetDestination(Player.transform.position);
    }
    public void StopMove()
    {
        NavMeshAgent.SetDestination(gameObject.transform.position);
        Debug.Log("StopMove");
    }
    public void RotateEnemyOverTime()
    {
        Vector3 dir = Player.transform.position - transform.position;
        dir.y = 0; // keep the direction strictly horizontal
        Quaternion rot = Quaternion.LookRotation(dir);
        // slerp to the desired rotation over time
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Lookspeed * Time.deltaTime);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerisInAttackBox = true;
            Debug.Log("InBox");
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerisInAttackBox = false;
            Debug.Log("InBox");
        }
    }
    public IEnumerator Patrolling()
    {
        //Animator.SetBool("Idle", false);
        foreach (Transform patrolpoint in PatrolPoints)
        {
            
            Vector3 PatrolpointVector = patrolpoint.position;
            NavMeshAgent.SetDestination(PatrolpointVector);

            float DistanceToDestination = Vector3.Distance(gameObject.transform.position, patrolpoint.transform.position);

            while (DistanceToDestination > 1.2)
            {
                DistanceToDestination = Vector3.Distance(gameObject.transform.position, patrolpoint.transform.position);
                yield return null;
            }
            yield return new WaitForSeconds(1);
        }
        if (isPatrolling)
        {
            StartCoroutine(Patrolling());
        }
    }
    public bool CheckAttackID(float DamageTaken, float HitID)
    {

        if (HitID == LastHitRecivedID)
        {
            Debug.Log("InvalidHit: " + HitID);
            return ValidHit = false;
        }
        else
        {
            LastHitRecivedID = HitID;
            Debug.Log("ValidHit: " + HitID);
            return ValidHit = true;
        }

    }
    public abstract void NewUpdate();
    public abstract void OnDeath();
    public abstract void OnTakeDamage();
    
}
