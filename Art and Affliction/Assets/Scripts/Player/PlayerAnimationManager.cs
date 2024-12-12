using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerAnimationManager : MonoBehaviour
{
    internal PlayerCombatManager PlayerCombatManager;
    internal ActionManager ActionManager;
    InputManager InputManager;

    //Animation Variables
    internal Animator PlayerAnimator;
    private AnimationClip LightAttackAnimation;
    public float LightAttackAnimationLength0;
    public float LightAttackAnimationLength1;
    public float LightAttackAnimationLength2;
    public float LightAttackAnimationLength3;

    public float HeavyAttackAnimationLength0;
    public float HeavyAttackAnimationLength1;
    public float HeavyAttackAnimationLength2;

    public float WeaponDrawAnimationLength;

    public float DeathAnimationLength;
    
    internal bool AnimationActive;
    internal bool IsTimerActive;
    
    //Weapon Variables
    private WeaponData WeaponData;
    private Collider WeaponHitBoxCollider;
    private WeaponHitbox CurrentWeaponHitbox;
    public GameObject CurrrentWeapon;
    public MeleeWeaponTrail WeaponTrail;

    //Dash Varibles
    Vector3 PreviousPosition;
    Vector3 direction;
    public float DashForce;
    public Rigidbody playerRigidbody;
    public float DashCooldown;
    internal float DashTimer;
    private bool isDashing;
    private bool DashForceOn;
    public float currentDashDuration;
    public GameObject[] playerModel;
    public GameObject SmokeEffect;

    //walking variables
    internal bool isGettingWalkInput;
    private bool isCurrentlyWalking;
    private bool isWalking;
    private bool isWalkingBackwards;
    private bool isWalkingRight;
    private bool isWalkingLeft;

    //Hitstun Variables
    private string LastAnimation;
    // Start is called before the first frame update
    private void Awake()
    {
        if (InputManager == null)
        {
            InputManager = GetComponent<InputManager>();
        }
        if (ActionManager == null)
        { 
            ActionManager = GetComponent<ActionManager>();
        }
        if (PlayerAnimator == null)
        {
            PlayerAnimator = GetComponent<Animator>();
        }
        if (WeaponHitBoxCollider == null)
        {
            WeaponHitBoxCollider = CurrrentWeapon.GetComponentInChildren<Collider>();
        
        }
        if (PlayerCombatManager == null)
        { 
            PlayerCombatManager = GetComponent<PlayerCombatManager>();
        }
        if (CurrentWeaponHitbox == null)
        {
            CurrentWeaponHitbox = CurrrentWeapon.GetComponentInChildren<WeaponHitbox>();
        }
        if (WeaponData == null)
        { 
            WeaponData = CurrrentWeapon.GetComponentInChildren<WeaponData>();
        }
        //Gets the length of the Animations from the animator
        RuntimeAnimatorController animatorController = PlayerAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in animatorController.animationClips)
        {
            if (clip.name == "Light attack0")
            {
                LightAttackAnimationLength0 = clip.length;
                //Debug.Log(LightAttackAnimationLength0);
            }
            if (clip.name == "Light attack1")
            {
                LightAttackAnimationLength1 = clip.length;
            }
            if (clip.name == "Light attack2")
            {
                LightAttackAnimationLength2 = clip.length;
            }
            if (clip.name == "Light attack3")
            {
                LightAttackAnimationLength3 = clip.length;
            }
            if (clip.name == "Weapon summon ")
            { 
                WeaponDrawAnimationLength = clip.length;
            }
            if (clip.name == "Heavy attack 0")
            {
                HeavyAttackAnimationLength0 = clip.length;
            }
            if (clip.name == "Heavy attack 1")
            {
                HeavyAttackAnimationLength1 = clip.length;
            }
            if (clip.name == "Heavy attack 2")
            {
                HeavyAttackAnimationLength2 = clip.length;
            }
            if (clip.name == "LeftDeath")
            {
                DeathAnimationLength = clip.length;
            }
        }
    }
    void Update()
    {
        //Get Dash Direction
        if (!isDashing)
        {
            direction = GetMovementDirection();
            
            PreviousPosition = transform.position;
        }
        
        // if we are getting a walk input and we are not doing an action right now handle the walk animaiton
        if (!ActionManager.actionQueueSlot_CurrentisFull && !PlayerCombatManager.isDead)
        {
            //PlayerAnimator.applyRootMotion = false;
            HandleWalkAnimation();
            HandleReleasingWalkAnimation();
        }
        // if we are not getting a walk input and not doing and action stop the walk animation
        else if (isCurrentlyWalking && !ActionManager.actionQueueSlot_CurrentisFull && !isGettingWalkInput)
        {
            //PlayerAnimator.applyRootMotion = true;
            isCurrentlyWalking = false;
            PlayerAnimator.SetBool("WalkForwards", false);
            
            isWalkingBackwards = false;
            isWalking = false ;
            isWalkingLeft = false;
            isWalkingRight = false;
        }
        if (ActionManager.actionQueueSlot_CurrentisFull)
        {
            DisableAllWalkingAnimations();
        }
        // enable and disable weapon trail
        if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Light attack0") ||
            PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Light attack1") ||
            PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Light attack2") ||
            PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Light attack3") ||
            PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Heavy attack 0") ||
            PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Heavy attack 1") ||
            PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Heavy attack 2"))
        {
            WeaponTrail.Emit = true;
        }
        else { WeaponTrail.Emit = false; }
    }
    private void FixedUpdate()
    {
        if (DashForceOn)
        {
            playerRigidbody.AddForce(direction.x * DashForce, 0, direction.z * DashForce, ForceMode.Force);
        }
    }
    private void DisableAllWalkingAnimations()
    {
        PlayerAnimator.SetBool("WalkForwards", false);
        PlayerAnimator.SetBool("WalkBack", false);
        PlayerAnimator.SetBool("WalkLeft", false);
        PlayerAnimator.SetBool("WalkRight", false);

        isWalkingBackwards = false;
        isWalking = false;
        isWalkingLeft = false;
        isWalkingRight = false;
    }
    private void HandleReleasingWalkAnimation()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            if (isWalkingRight)
            {
                isWalkingRight = false;
                PlayerAnimator.SetBool("WalkRight", false);
            }
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            if (isWalkingLeft)
            {
                isWalkingLeft = false;
                PlayerAnimator.SetBool("WalkLeft", false);
            }
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            if (isWalkingBackwards)
            {
                isWalkingBackwards = false;
                PlayerAnimator.SetBool("WalkBack", false);
            }
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            if (isWalking)
            {
                isWalking = false;
                PlayerAnimator.SetBool("WalkForwards", false);
            }
        }
    }
    private void HandleWalkAnimation()
    {
        if (!PlayerCombatManager.isLockedOn && isGettingWalkInput)
        {
            if (!PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                isCurrentlyWalking = true;
                PlayerAnimator.SetBool("WalkForwards", true);
            }
        }
        else
        {
            // 1 = right, -1 = left, 1 = forwards, -1 = backwards
            if (InputManager.horizontalInput == 1)
            {
                if (!isWalkingBackwards && !isWalking)
                {
                    isCurrentlyWalking = true;
                    isWalkingRight = true;
                    PlayerAnimator.SetBool("WalkRight", true);
                }
            }
            if (InputManager.horizontalInput == -1)
            {
                if (!isWalkingBackwards && !isWalking)
                {
                    isCurrentlyWalking = true;
                    isWalkingLeft = true;
                    PlayerAnimator.SetBool("WalkLeft", true);
                }
            }
            if (InputManager.verticalInput == 1)
            {
                if (!isWalkingLeft && !isWalkingRight)
                {
                    isCurrentlyWalking = true;
                    isWalking = true;
                    PlayerAnimator.SetBool("WalkForwards", true);
                }
            }
            if (InputManager.verticalInput == -1)
            {
                if (!isWalkingLeft && !isWalkingRight)
                {
                    isCurrentlyWalking = true;
                    isWalkingBackwards = true;
                    PlayerAnimator.SetBool("WalkBack", true);
                }
            }
            if (InputManager.verticalInput == 0)
            {
                
                PlayerAnimator.SetBool("WalkBack", false);
                PlayerAnimator.SetBool("WalkForwards", false);
                isWalkingBackwards = false;
                isWalking = false;
            }
            if (InputManager.horizontalInput == 0)
            {
                PlayerAnimator.SetBool("WalkLeft", false);
                PlayerAnimator.SetBool("WalkRight", false);
                isWalkingRight = false;
                isWalkingLeft = false;
            }
        }
    }
    public void HandleLightAttack(float LightAttackChainValue)
    {
        isCurrentlyWalking = false;
        if (LightAttackChainValue == 0)
        { 
            StartCoroutine(LightAttack0());
        }
        if (LightAttackChainValue == 1)
        {
            StartCoroutine(LightAttack1());
        }
        if (LightAttackChainValue == 2)
        {
            StartCoroutine(LightAttack2());
        }
        if (LightAttackChainValue == 3)
        {
            StartCoroutine(LightAttack3());
        }
    }
    public void HandleHeavyAttack(float HeavyAttackChainValue)
    {
        isCurrentlyWalking = false;
        if (HeavyAttackChainValue == 0)
        {
            StartCoroutine(HeavyAttack0());
        }
        if (HeavyAttackChainValue == 1)
        {
            StartCoroutine(HeavyAttack1());
        }
        if (HeavyAttackChainValue == 2)
        {
            StartCoroutine(HeavyAttack2());
        }
    }
    public IEnumerator LightAttack0()
    {
        
        Debug.Log("LightAttack0");
        PlayerAnimator.SetTrigger("Swing");
        ActionManager.SetCurrentActionDuration(LightAttackAnimationLength0);
        yield return new WaitForSeconds(LightAttackAnimationLength0 - .1f);
    }
    public IEnumerator LightAttack1()
    {
        Debug.Log("LightAttack1");
        PlayerAnimator.SetTrigger("Swing1");
        ActionManager.SetCurrentActionDuration(LightAttackAnimationLength1);
        yield return new WaitForSeconds(LightAttackAnimationLength0 - .1f);
    }
    public IEnumerator LightAttack2()
    {
        Debug.Log("LightAttack2");
        PlayerAnimator.SetTrigger("Swing2");
        ActionManager.SetCurrentActionDuration(LightAttackAnimationLength2);
        yield return new WaitForSeconds(LightAttackAnimationLength0 - .1f);
    }
    public IEnumerator LightAttack3()
    {
        Debug.Log("LightAttack3");
        PlayerAnimator.SetTrigger("Swing3");
        ActionManager.SetCurrentActionDuration(LightAttackAnimationLength2);
        yield return new WaitForSeconds(LightAttackAnimationLength0 - .1f);
    }
    public IEnumerator HeavyAttack0()
    {
        Debug.Log("HeavyAttack0");
        PlayerAnimator.SetTrigger("SwingHeavy");
        ActionManager.SetCurrentActionDuration(HeavyAttackAnimationLength0);
        yield return new WaitForSeconds(HeavyAttackAnimationLength0 - .1f);
    }
    public IEnumerator HeavyAttack1()
    {
        Debug.Log("HeavyAttack1");
        PlayerAnimator.SetTrigger("SwingHeavy1");
        ActionManager.SetCurrentActionDuration(HeavyAttackAnimationLength1);
        yield return new WaitForSeconds(HeavyAttackAnimationLength1 - .1f);
    }
    public IEnumerator HeavyAttack2()
    {
        Debug.Log("HeavyAttack2");
        PlayerAnimator.SetTrigger("SwingHeavy2");
        ActionManager.SetCurrentActionDuration(HeavyAttackAnimationLength2);
        yield return new WaitForSeconds(HeavyAttackAnimationLength2 - .1f);
    }
    public void HandleDash()
    {
        StartCoroutine(Dash());
        ActionManager.SetCurrentActionDuration(.5f);
    }
    public IEnumerator Dash()
    {
        Debug.Log(direction);
        isDashing = true;
        DashTimer = Time.time + DashCooldown;
        float elapsedTime = 0f;
        float duration = currentDashDuration;
        bool effect1spawned = false;
        bool effect2spawned = false;
        while (elapsedTime < duration)
        {
            DashForceOn = true;
            if (elapsedTime / duration > .15f)
            {
                foreach (GameObject meash in playerModel)
                {
                    meash.SetActive(false);
                    if (effect1spawned == false)
                    {
                        GameObject Effect1 = Instantiate(SmokeEffect, gameObject.transform);
                        Destroy(Effect1, 1);
                        effect1spawned = true;
                    }
                    
                }
            }
            if (elapsedTime / duration > .80f)
            {
                foreach (GameObject meash in playerModel)
                {
                    meash.SetActive(true);
                    if (effect2spawned == false)
                    {
                        GameObject Effect2 = Instantiate(SmokeEffect, gameObject.transform);
                        Destroy(Effect2, 1);
                        effect2spawned = true;
                    }
                }
            }
            elapsedTime += Time.deltaTime; // Increment elapsed time
            float t = elapsedTime / duration; // Calculate the normalized time [0, 1]

            // Move the object using Lerp
            
            //playerRigidbody.AddForce(direction * DashForce, ForceMode.Force);
            //playerRigidbody.AddForce(direction.x * DashForce, 0, direction.z * DashForce, ForceMode.Force);
            yield return null; // Wait for the next frame
            
        }
        isDashing = false;
        effect1spawned = false;
        effect2spawned = false;
        DashForceOn = false;
        yield return null;
    }
    Vector3 GetMovementDirection()
    {
        // Calculate the movement vector
        Vector3 movement = transform.position - PreviousPosition;

        // If the movement vector is not zero, normalize it to get the direction
        if (movement != Vector3.zero)
        {
            return movement.normalized;
        }
        // If there's no movement, return a zero vector
        return Vector3.zero;
    }
    public void HandleHitstun()
    {
        ActionManager.SetCurrentActionDuration(1.25f);
        if (LastAnimation == null || LastAnimation == "HitstunRight")
        {
            PlayerAnimator.SetTrigger("HitstunLeft");
            LastAnimation = "HitstunLeft";
            return;
        }
        else if (LastAnimation == "HitstunLeft")
        {
            PlayerAnimator.SetTrigger("HitstunRight");
            LastAnimation = "HitstunRight";
        }
        
    }
    public IEnumerator DrawWeapon()
    {
        isCurrentlyWalking = false;
        ActionManager.SetCurrentActionDuration(WeaponDrawAnimationLength);
        PlayerAnimator.SetTrigger("DrawWeapon");
        float elapsedTime = 0f;
        float duration = WeaponDrawAnimationLength;
        while (elapsedTime < duration)
        {
            if (elapsedTime / duration > .6f)
            {
                CurrrentWeapon.SetActive(true);
            }
            elapsedTime += Time.deltaTime; // Increment elapsed time
            yield return null;
        }
    }
    public IEnumerator DeathAnimation()
    {
        ActionManager.SetCurrentActionDuration(DeathAnimationLength);
        PlayerAnimator.SetTrigger("DeathLeft");
        
        yield return null;  
    }
}
