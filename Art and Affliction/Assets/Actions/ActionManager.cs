using System.Collections;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    PlayerCombatManager PlayerCombatManager;
    PlayerAnimationManager PlayerAnimationManager;
    Animator animator;
    
    public Action actionQueueSlot_Current;
    public Action actionQueueSlot_Next;

    private float CurrentActionDuration;
    public bool ActionisHalfDone;
    
    public bool actionQueueSlot_CurrentisFull;
    public bool actionQueueSlot_NextisFull;
    
    private Coroutine ActionQueueCurrent;

    public float LightattackChainValue = 0;
    public float HeavyattackChainValue = 0;

    public Action HitStun;
    public Action DrawWeapon;

    private void Start()
    {
        PlayerCombatManager = GetComponent<PlayerCombatManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        animator = GetComponentInChildren<Animator>();
        AddAction(DrawWeapon);
    }
    private void Update()
    {
        MoveNextAction();

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("IdleState"))
        {
            ResetAttackChain();
        }
        

        if (Input.GetKeyDown(KeyCode.P))
        {
            AddAction(HitStun);
        }
        
    }
    public void MoveNextAction()
    {
        //if current action slot is empty move the next action into the current slot
        if (!actionQueueSlot_CurrentisFull && actionQueueSlot_Next != null)
        {
            actionQueueSlot_Current = actionQueueSlot_Next;
            actionQueueSlot_Next = null;
            actionQueueSlot_NextisFull = false;
            StartCoroutine(actionQueueCurrent(actionQueueSlot_Current));
        }
    }
    //add an action to the current slot if it is open, if not, add it to the next slot
    public void AddAction(Action action)
    {
        if (PlayerCombatManager.isDead)
        {
            return;
        }
        if (!actionQueueSlot_CurrentisFull)
        {
            StartCoroutine(actionQueueCurrent(action));
        }
        else if (action.isInteruptAction)
        {
            StopAllCoroutines();
            actionQueueSlot_Current = null;
            actionQueueSlot_CurrentisFull = false;
            StartCoroutine(actionQueueCurrent(action));
        }
        else if (!actionQueueSlot_NextisFull)
        {
            if (ActionisHalfDone)
            {
                actionQueueSlot_Next = action;
                actionQueueSlot_NextisFull = true;
            }
        }
    }
    //Process the current action and wait for it to end before clearing the current action
    public IEnumerator actionQueueCurrent(Action action)
    {
        ActionisHalfDone = false;
        actionQueueSlot_Current = action;
        actionQueueSlot_CurrentisFull = true;
        
        ProcessAction(action);
        yield return new WaitForSeconds(CurrentActionDuration / 2);
        ActionisHalfDone = true;
        yield return new WaitForSeconds(CurrentActionDuration / 2 - 0.1f);

        actionQueueSlot_CurrentisFull = false;
        actionQueueSlot_Current = null;
        ProcessEndOfAction(action);
    }
    public void SetCurrentActionDuration(float Length)
    { 
        CurrentActionDuration = Length;
    }
    public void ProcessEndOfAction(Action action)
    {
        
    }
    public void ProcessAction(Action action)
    {
        if (action.ActionType == "Attack")
        {
            GameObject CurrentWeapon = PlayerCombatManager.CurrentWeapon;
            WeaponData weaponData = CurrentWeapon.GetComponent<WeaponData>();
            if (action.ActionName == "LightAttack")
            {
                PlayerAnimationManager.HandleLightAttack(LightattackChainValue);
                if (LightattackChainValue < weaponData.MaxLightAttackChainValue)
                {
                    LightattackChainValue++;
                     
                }
                else
                { 
                    LightattackChainValue = 0;
                }
            }
            if (action.ActionName == "HeavyAttack")
            {
                PlayerAnimationManager.HandleHeavyAttack(HeavyattackChainValue);
                if (HeavyattackChainValue < weaponData.MaxHeavyAttackChainValue)
                {
                    HeavyattackChainValue++;
                }
                else
                {
                    HeavyattackChainValue = 0;
                }
            }
        }
        if (action.ActionType == "Other")
        {
            if (action.ActionName == "Dash")
            {
                PlayerAnimationManager.HandleDash();
            }
            if (action.ActionName == "HitStun")
            {
                PlayerAnimationManager.HandleHitstun();
            
            }
            if (action.ActionName == "DrawWeapon")
            { 
                StartCoroutine(PlayerAnimationManager.DrawWeapon());
            }
            if (action.ActionName == "Death")
            {
                StartCoroutine(PlayerAnimationManager.DeathAnimation());
            }
        }
    }

    public void ResetAttackChain()
    {
        LightattackChainValue = 0;
        HeavyattackChainValue = 0;
    }
            
}