using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SwordEnemy : Enemy
{
    private float EnemiesAttacking;
    private bool StrafeRight;
    public EnemyAgroManager AgroManager;
    private bool isInAttackAnimation;
    private bool isStrafing;
    private bool hasDied;

    private float Overhead1AnimationLength;
    private float Overhead2AnimationLength;
    private float SliceComboAniamtionLength;
    private float BigSwingAnimationlength;

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
            if (clip.name == "Overhead1")
            {
                Overhead1AnimationLength = clip.length;

            }
            if (clip.name == "Overhead2")
            {
                Overhead2AnimationLength = clip.length;

            }
            if (clip.name == "2swing")
            {
                SliceComboAniamtionLength = clip.length;

            }
            if (clip.name == "BigSwing ")
            {
                BigSwingAnimationlength = clip.length;
            }
        }
    }
    public override void NewUpdate()
    {
        
        if (isInCombat && !isDead)
        {
             
            if (AgroManager.agressiveEnemyCounter >= 3 && !SubState_isAttacking)
            {
                //Strafe
                if (!isStrafing)
                {
                    tryToLookAtPlayer = true;
                    StartCoroutine(SubState_Strafing());
                }
            }
            else
            {
                Animator.SetBool("WalkLeft", false);
                Animator.SetBool("WalkRight", false);
                if (!SubState_isAttacking)
                {
                    AgroManager.agressiveEnemyCounter++;
                    StartCoroutine(DrawWeapon());
                    StopMove();
                    Animator.SetTrigger("DrawWeapon");
                    Animator.SetBool("WalkForwards", false);
                    StopMove();
                    SubState_isAttacking = true;
                }

                if (HasDrawnWeapon)
                {
                    DistanceToPlayer = Vector3.Distance(gameObject.transform.position, Player.transform.position);
                    //Do an attack if player in in range and is facing player
                    if (IsInAttackRange && PlayerisInAttackBox)
                    {
                        StopMove();
                        Animator.SetBool("WalkForwards", false);
                        tryToLookAtPlayer = false;
                        isInCombat = true;
                        if (!isInAttackAnimation)
                        {
                            StartCoroutine(SubState_Attacking());
                        }
                        return;
                    }

                    // if we are in attack range
                    if (DistanceToPlayer <= AttackRange && !isInAttackAnimation)
                    {
                        StopMove();
                        Animator.SetBool("WalkForwards", false);
                        IsInAttackRange = true;
                    }
                    //if we are far away from the player move towards him
                    if (DistanceToPlayer >= AttackRange && !isInAttackAnimation)
                    {
                        tryToLookAtPlayer = true;
                        MoveToPlayer();
                        Animator.SetBool("WalkForwards", true);
                    }
                    //if we are in attack range but not facing the player look at him
                    else if (!PlayerisInAttackBox && !isInAttackAnimation)
                    {
                        StopMove();
                        Animator.SetBool("WalkForwards", false);
                        tryToLookAtPlayer = true;
                        IsInAttackRange = true;
                    }
                }
            }
            return;
        }
        if (!isDead && isPatrolling)
        {
            Animator.SetBool("WalkForwards", true);
        }
        else if (!isDead)
        {
            Animator.SetBool("WalkForwards", false);
        }
    }
    private IEnumerator SubState_Strafing()
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
    }
    private IEnumerator SubState_Attacking()
    {
        isInAttackAnimation = true;
        float AttackValue = Random.Range(0f, 100f);
        if (AttackValue < 33)
        {
            Animator.SetTrigger("Overhead1");

            float overheadcoinflip = Random.Range(0f, 100f);
            if (overheadcoinflip > 50)
            {
                Animator.SetTrigger("Overhead2");
                transform.LookAt(Player.transform.position);
                yield return new WaitForSeconds(Overhead1AnimationLength);

                yield return new WaitForSeconds(Overhead2AnimationLength);
            }
            else
            {
                yield return new WaitForSeconds(Overhead1AnimationLength);
            }
            
        }
        else if (AttackValue > 33 && AttackValue < 66)
        {
            Animator.SetTrigger("BigSwing");
            yield return new WaitForSeconds(BigSwingAnimationlength);
        }
        else if (AttackValue > 66)
        {
            Animator.SetTrigger("2Swing");
            yield return new WaitForSeconds(SliceComboAniamtionLength);
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
        yield return new WaitForSeconds(3f);
        Debug.Log("Weapon Droawn");
        HasDrawnWeapon = true;
    }
}
