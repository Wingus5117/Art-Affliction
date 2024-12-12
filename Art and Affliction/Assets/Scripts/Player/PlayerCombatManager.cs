using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCombatManager : MonoBehaviour
{
    public bool isLockedOn;
    public Enemy enemy;
    public Transform PlayerLockOnTransform;
    public CameraManager cameraManager;
    private InputManager inputManager;
    internal PlayerAnimationManager playerAnimationManager;
    public GameObject CurrentWeapon;
    internal WeaponData CurrentWeaponData;
    public Action DeathAction;
    private ActionManager ActionManager;
    
    //Player Stats
    public float CurrentHealth;
    public float MaxHealth;
    internal bool isDead = false;

    //Hit Check Variables
    private float LastHitRecivedID;
    internal bool ValidHit;
    public List<float> HitIDs;
    public float InvicibilityTimerOnHit;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerAnimationManager == null)
        {
            playerAnimationManager = GetComponent<PlayerAnimationManager>();
        }
        if (inputManager == null)
        { 
            inputManager = GetComponent<InputManager>();
        }
        if (ActionManager == null)
        {
            ActionManager = GetComponent<ActionManager>();
        }
        
    }
    public void SetTarget(Enemy targetedEnemy)
    {
        if (targetedEnemy != null)
        {
            enemy = targetedEnemy;
        }
        else
        {
            enemy = null;
        }
    }
    public void DealDamage(Enemy enemy,float WeaponAnimationValue)
    {
        CurrentWeaponData = CurrentWeapon.GetComponent<WeaponData>();
        enemy.TakeDamage(CurrentWeaponData.Damage * WeaponAnimationValue);
    }
    public bool CheckAttackID(float DamageTaken, float HitID)
    {

        if (HitID == LastHitRecivedID)
        {
            //Debug.Log("InvalidHit: " + HitID);
            return ValidHit = false;
        }
        else
        {
            bool HitIsOnTheList = false;
            foreach (float ID in HitIDs)
            {
                if (ID == HitID)
                {
                    HitIsOnTheList = true;
                }
            }
            if (!HitIsOnTheList)
            {
                LastHitRecivedID = HitID;
                takeDamage(DamageTaken);
                StartCoroutine(AddHitIDToList(HitID));
                //Debug.Log("ValidHit: " + HitID);
                return ValidHit = true;
            }
            //Debug.Log("InvalidHit: " + HitID);
            
            return ValidHit = false;
        }
        
    }
    private void takeDamage(float DamageTaken)
    {
        CurrentHealth -= DamageTaken;
        if (CurrentHealth <= 0)
        {
            isDead = true;
            playerAnimationManager.PlayerAnimator.SetBool("isDead", true);
            ActionManager.AddAction(DeathAction);
            StartCoroutine(ResetScene());
        }
    }
    private IEnumerator AddHitIDToList(float HitID)
    {
        HitIDs.Add(HitID);
        yield return new WaitForSeconds(5);
        HitIDs.Remove(HitID);
    }
    private IEnumerator ResetScene()
    { 
        yield return new WaitForSeconds(4);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
