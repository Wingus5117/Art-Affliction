using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;

    Vector3 Movedirection;
    Transform cameraObject;
    PlayerCombatManager playerCombatManager;
    PlayerAnimationManager playerAnimationManager;
    ActionManager actionManager;
    Rigidbody rb;

    public float normalmovmentSpeed = 4;
    public float LockOnMovmentSpeed = 3;
    private float movmentSpeed;
    public float RotationSpeed = 15;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
        playerCombatManager = GetComponent<PlayerCombatManager>();
        playerAnimationManager = GetComponent<PlayerAnimationManager>();
        actionManager = GetComponent<ActionManager>();
    }

    private void Update()
    {
        if (playerCombatManager.isLockedOn)
        {
            movmentSpeed = LockOnMovmentSpeed;
        }
        else
        {
            movmentSpeed = normalmovmentSpeed;
        }
    }
    private void HandleMovment()
    {
        Movedirection = cameraObject.forward * inputManager.verticalInput;
        Movedirection = Movedirection + cameraObject.right * inputManager.horizontalInput;
        Movedirection.Normalize();
        Movedirection.y = 0;
        Movedirection *= movmentSpeed;

        Vector3 movmentVelocity = Movedirection;
        
        rb.velocity = movmentVelocity;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        if (playerCombatManager.isLockedOn)
        {
            if (playerCombatManager.enemy != null)
            {
                targetDirection = playerCombatManager.enemy.transform.position - transform.position;
                targetDirection.y = 0;
                targetDirection.Normalize();
                
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                transform.rotation = finalRotation;
            }
        }
        else 
        {

            targetDirection = cameraObject.forward * inputManager.verticalInput;
            targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion playerRoation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            transform.rotation = playerRoation;
        }
        
    }
    public void HandleAllMovment()
    {
        //dont move if we are doing an action
        if (!actionManager.actionQueueSlot_CurrentisFull && !playerCombatManager.isDead) 
        {
             HandleMovment();
            
        }
        if (!playerCombatManager.isDead)
        {
            HandleRotation();
        }
    }
}
