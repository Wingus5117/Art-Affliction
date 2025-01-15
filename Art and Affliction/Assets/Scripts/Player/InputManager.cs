using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public PlayerControls playercontrols;
    PlayerCombatManager PlayerCombatManager;
    CameraManager CameraManager;
    PlayerAnimationManager PlayerAnimationManager;
    
    //Movment Inputs
    public Vector2 MovmentInput;
    public float verticalInput;
    public float horizontalInput;

    //Camera Inputs
    public Vector2 cameraInput;

    public float cameraInputX;
    public float cameraInputY;

    //Lock on Input
    private bool lockOnInput;
    internal bool LeftLockOnInput;
    internal bool RightLockOnInput;
    private float LockOnTimer;
    public float LockOnTargetChangeInputAmountRequired;

    //Weapon Inputs
    internal bool LightAttackInput;
    internal bool HeavyAttackInput;
    private ActionManager ActionManager;
   
    public Action LightAttackAction0;
    public Action LightAttackAction1;
    public Action LightAttackAction2;
    public Action LightAttackAction3;
    
    public Action HeavyAttackAction0;
    public Action HeavyAttackAction1;
    public Action HeavyAttackAction2;

    //Dash Input
    //private bool DashInput;
    //public Action DashAction;

    private void OnEnable()
    {
        if (ActionManager == null)
        { 
            ActionManager = GetComponent<ActionManager>();
        }
        if (playercontrols == null)
        {
            playercontrols = new PlayerControls();

            //Read WASD
            playercontrols.PlayerMovement.Movement.performed += i => MovmentInput = i.ReadValue<Vector2>();
            
            //Read Mouse Movment for camera/Chaning Lock on target while locked on
            playercontrols.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
           
            //Read Middle Mouse Press
            playercontrols.PlayerMovement.LockOn.performed += i => lockOnInput = true;

            //Read Left Click
            playercontrols.PlayerMovement.Attack.performed += i => LightAttackInput = true;

            //Read Left Click
            playercontrols.PlayerMovement.HeavyAttack.performed += i => HeavyAttackInput = true;

            //Read Shift Button
            //playercontrols.PlayerMovement.Dash.performed += i => DashInput = true;
        }
        if (PlayerCombatManager == null)
        {
            PlayerCombatManager = GetComponent<PlayerCombatManager>();
        }
        if (CameraManager == null)
        { 
            CameraManager = FindObjectOfType<CameraManager>();
        }
        if (PlayerAnimationManager == null)
        {
            PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        }
        
        playercontrols.Enable();
    }

    private void OnDisable()
    {
        playercontrols.Disable();
    }
    
    private void HandleAttackInput()
    {
        if (LightAttackInput)
        {
            
            LightAttackInput = false;
            if (ActionManager.LightattackChainValue == 0)
            {
                ActionManager.AddAction(LightAttackAction0);
            }
            else if (ActionManager.LightattackChainValue == 1)
            {
                ActionManager.AddAction(LightAttackAction1);
            }
            else if (ActionManager.LightattackChainValue == 2)
            {
                ActionManager.AddAction(LightAttackAction2);
            }
            else if (ActionManager.LightattackChainValue == 3)
            {
                ActionManager.AddAction(LightAttackAction3);
            }

        }
        if (HeavyAttackInput)
        {
            HeavyAttackInput= false;
            if (ActionManager.HeavyattackChainValue == 0)
            {
                ActionManager.AddAction(HeavyAttackAction0);
            }
            if (ActionManager.HeavyattackChainValue == 1)
            {
                ActionManager.AddAction(HeavyAttackAction1);
            }
            if (ActionManager.HeavyattackChainValue == 2)
            {
                ActionManager.AddAction(HeavyAttackAction2);
            }

        }
    }

    private void HandleLockOnInput()
    {
        
        //Are we locked on
        if (PlayerCombatManager.isLockedOn)
        {
            
            //does our target exist
            if (PlayerCombatManager.enemy == null)
            {
                return;
            }

            //is our current target dead 
            if (PlayerCombatManager.enemy.isDead)
            {
                PlayerCombatManager.isLockedOn = false;
            }
        }



        if (lockOnInput && PlayerCombatManager.isLockedOn)
        {
            //Disable Lock On
            lockOnInput = false;
            PlayerCombatManager.isLockedOn = false;
            CameraManager.ClearLockOnTargets();
            return;
        }

        if (lockOnInput && !PlayerCombatManager.isLockedOn)
        {
            
            lockOnInput = false;
            CameraManager.HandleLocatingLockOnTargets();
            if (CameraManager.nearestLockOnTarget != null)
            {
                PlayerCombatManager.isLockedOn = true;
                //Set Camera Height up
                // Assign Target
            }
            else
            {
                PlayerCombatManager.isLockedOn = false;
                CameraManager.ClearLockOnTargets();
            }

        }

        //Changing Lock On Target
        if (cameraInput.x >= LockOnTargetChangeInputAmountRequired && PlayerCombatManager.isLockedOn && LockOnTimer < Time.time)
        {
            cameraInput.x = 0;
            RightLockOnInput = true;
            //Debug.Log("RightInput");
            HandleLockOnSwitchTargetInputs();
            LockOnTimer = Time.time + .25f;
        
        }
        if (cameraInput.x <= -LockOnTargetChangeInputAmountRequired && PlayerCombatManager.isLockedOn && LockOnTimer < Time.time)
        {
            cameraInput.x = 0;
            LeftLockOnInput = true;
            //Debug.Log("LeftInput");
            HandleLockOnSwitchTargetInputs();
            LockOnTimer = Time.time + .25f;

        }
    }
    private void HandleLockOnSwitchTargetInputs()
    {
        if (RightLockOnInput)
        {
            RightLockOnInput = false;
            CameraManager.HandleLocatingNewLockOnTarget();
            CameraManager.HandleChangingLockOnTargets();
            if (CameraManager.RightLockOnTarget != null)
            {
                PlayerCombatManager.SetTarget(CameraManager.RightLockOnTarget);
            }
        }
        if (LeftLockOnInput)
        {
            LeftLockOnInput = false;
            CameraManager.HandleLocatingNewLockOnTarget();
            CameraManager.HandleChangingLockOnTargets();
            if (CameraManager.LeftLockOnTarget != null)
            {
                PlayerCombatManager.SetTarget(CameraManager.LeftLockOnTarget);
            }
        }
    }
    private void HandleMovmentInput()
    {
        //Handle player movement input
        verticalInput = MovmentInput.y;
        horizontalInput = MovmentInput.x;
        
        //Handle camera movment
        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        //Check if we are trying to move
        if (verticalInput == 0 && horizontalInput == 0)
        {
            PlayerAnimationManager.isGettingWalkInput = false;
        }
        else
        {
            PlayerAnimationManager.isGettingWalkInput = true;
        }
    }

    /*private void HandleDashInput()
    {
        if (DashInput)
        {
            DashInput = false;
            if (PlayerAnimationManager.DashTimer < Time.time && !ActionManager.actionQueueSlot_CurrentisFull)
            {
                ActionManager.AddAction(DashAction);
            }
        }
    }*/

    public void HandleAllInput()
    {
        HandleMovmentInput();
        HandleLockOnInput();
        HandleAttackInput();
        //HandleDashInput();
    }
}
