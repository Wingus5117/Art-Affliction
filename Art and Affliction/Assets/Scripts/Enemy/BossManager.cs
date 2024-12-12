using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public Enemy BossEnemy;
    public GameObject BossWalls;
    public GameObject BossUI;
    public Image BossHealthBar;
    private bool BossIsActive;
    public bool BossIsAlive;
    private bool once = true;
    private PlayerCombatManager PlayerCombatManager;
    private void Start()
    {
        BossIsAlive = true;
        BossUI.gameObject.SetActive(false);
        BossEnemy.isBoss = true;
    }
    private void Update()
    {
        if (!BossIsAlive && once)
        {
            once = false;
            OnKillBoss();
            return;
        }
        if (BossIsActive)
        { 
            BossHealthBar.fillAmount = BossEnemy.CurrentHealth / BossEnemy.MaxHealth;
            if (BossEnemy.CurrentHealth <= 0)
            {
                BossIsAlive = false;
                return;
            }
        }
    }
    private void OnKillBoss()
    {
        Debug.Log("BossDead");
        BossUI.gameObject.SetActive(false);
        BossIsActive = false;
        
        gameObject.SetActive(false);
            
    }
    private void OnBossStart()
    {
        if (!BossIsActive)
        {
            BossEnemy.isIdle = false;
            BossEnemy.isAttacking = true;
            BossUI.gameObject.SetActive(true);
            BossIsActive = true;

            BossWalls.SetActive(true);
            PlayerCombatManager.CurrentHealth = PlayerCombatManager.MaxHealth;
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerCombatManager = other.GetComponent<PlayerCombatManager>();
            OnBossStart();
            
        }
    }
}
