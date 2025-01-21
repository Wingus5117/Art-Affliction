using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FireEnemy2 : Enemy
{
    private float EnemiesAttacking;
    private bool StrafeRight;
    public EnemyAgroManager AgroManager;
    private bool isInAttackAnimation;
    private bool isStrafing;
    private bool hasDied;

    private float SpinAttackAnimationLength;
    private float ConeAttackAnimationLength;
    private float LineAttackAnimationLength;

    public float DelayBetweenAttacks;
    public ParticleSystem DeathParticleEffect;
    public GameObject ParticleSpawnPoint;

    private bool HasDrawnWeapon;
    private void Start()
    {
        SubState_isAttacking = false;
        RuntimeAnimatorController animatorController = Animator.runtimeAnimatorController;
        foreach (AnimationClip clip in animatorController.animationClips)
        {
            if (clip.name == "FireManLineAttack")
            {
                LineAttackAnimationLength = clip.length;
            }
            if (clip.name == "FireManSpinAttack")
            {
                SpinAttackAnimationLength = clip.length;
            }
            if (clip.name == "FireManConeAttack")
            {
                ConeAttackAnimationLength = clip.length;
            }
        }
    }
    public override void NewUpdate()
    {
        if (isInCombat && !isDead)
        {
            //Stop all movemtn animations and play draw weapon for 3 seconds
            ClearMovementAnimationBools();
            StartCoroutine(DrawWeapon());
            AgroManager.agressiveEnemyCounter++;
            
            //once the weapon is drawn begin combat logic
            if (HasDrawnWeapon)
            {
                //find distance to the player
                DistanceToPlayer = Vector3.Distance(gameObject.transform.position, Player.transform.position);
                
                //Do an attack if player in in range and the player in the the attack box (the trigger attached to the enemy gameobject)
                if (IsInAttackRange && PlayerisInAttackBox)
                {
                    StopMove();
                    ClearMovementAnimationBools();
                    //dont rotate to the player
                    tryToLookAtPlayer = false;
                    //if we are not currently in an attack animation, do an attack
                    if (!isInAttackAnimation)
                    {
                        //Begin Attack logic
                        StartCoroutine(SubState_Attacking());
                    }
                    return;
                }

                // Check1 = check if player is inside the enemy attack range
                if (DistanceToPlayer <= AttackRange && !isInAttackAnimation)
                {
                    StopMove();
                    ClearMovementAnimationBools();
                    IsInAttackRange = true;
                    // Check2 = check if the enemy is facing the polayer by referencing the attack box
                    if (PlayerisInAttackBox)
                    {
                        isFacingPlayer = true;
                    }
                    // if the player is not in the attack box rotate to them
                    else
                    {
                        StopMove();
                        ClearMovementAnimationBools();
                        tryToLookAtPlayer = true;
                        IsInAttackRange = true;
                    }
                }
                // if we are not in the attack range look at the player and move to them
                else if (!isInAttackAnimation)
                {
                    tryToLookAtPlayer = true;
                    MoveToPlayer();
                    Animator.SetBool("WalkForwards", true);
                }
            }
        }
    }
    /*private IEnumerator SubState_Strafing()
    {
        isStrafing = true;
        if (StrafeRight)
        {
            Vector3 rightDestination = transform.position + transform.right * 5;
            NavMeshAgent.SetDestination(rightDestination);
            
            Animator.SetBool("WalkRight", true);
            Animator.SetBool("WalkLeft", false);
            // Wait until the agent reaches the target position
            while (NavMeshAgent.remainingDistance > 1.2f)
            {
                
                yield return null; // Wait for next frame
            }
        }
        else 
        {
            // Strafe to the left
            Vector3 leftDestination = transform.position - transform.right * 5;
            NavMeshAgent.SetDestination(leftDestination);

            Animator.SetBool("WalkLeft", true);
            Animator.SetBool("WalkRight", false);
            // Wait until the agent reaches the target position
            while (NavMeshAgent.remainingDistance > 1.2f)
            {
                yield return null; // Wait for next frame
            }
            
        }
        StrafeRight = !StrafeRight;
        isStrafing = false;
        yield return null;
    }*/
    private IEnumerator SubState_Attacking()
    {
        isInAttackAnimation = true;
        float AttackValue = Random.Range(0f, 100f);
        if (AttackValue < 33)
        {
            Animator.SetTrigger("LineAttack");
            yield return new WaitForSeconds(LineAttackAnimationLength);
        }
        else if (AttackValue > 33 && AttackValue < 66 && DistanceToPlayer < 2)
        {
            Animator.SetTrigger("SpinAttack");
            yield return new WaitForSeconds(SpinAttackAnimationLength);
        }
        else if (AttackValue > 66 && DistanceToPlayer < 4)
        {
            Animator.SetTrigger("ConeAttack");
            yield return new WaitForSeconds(ConeAttackAnimationLength);
        }
        yield return new WaitForSeconds(DelayBetweenAttacks);
        isInAttackAnimation = false;
    }
    public override void OnDeath()
    {
        if (!hasDied)
        {
            hasDied = true;
            AgroManager.agressiveEnemyCounter--;
            StartCoroutine(SpawnDeathParticle());
            Animator.SetTrigger("Death1");
            Animator.SetTrigger("Death2");
        }
    }
    public override void OnTakeDamage()
    {
        Animator.SetTrigger("TakeDamage");
        transform.LookAt(Player.transform.position);
    }
    private IEnumerator SpawnDeathParticle()
    {
        yield return new WaitForSeconds(2.5f);
        ParticleSystem particleSystem = Instantiate(DeathParticleEffect);
        particleSystem.transform.position = ParticleSpawnPoint.transform.position;
    }
    private IEnumerator DrawWeapon()
    {
        Animator.SetTrigger("DrawWeapon");
        yield return new WaitForSeconds(3f);
        HasDrawnWeapon = true;
    }
    private void ClearMovementAnimationBools()
    {
        Animator.SetBool("WalkLeft", false);
        Animator.SetBool("WalkRight", false);
        Animator.SetBool("WalkForwards", false);
        Animator.SetBool("WalkBackwards", false);
    }
}
