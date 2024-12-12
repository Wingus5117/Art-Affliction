using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PlayerCombatManager PlayerCombatManager;
    public Image HealthBar;
    public PlayerAnimationManager PlayerAnimationManager;
    public Image DashCooldownBar;

    // Update is called once per frame
    void Update()
    {
        HealthBar.fillAmount = PlayerCombatManager.CurrentHealth / PlayerCombatManager.MaxHealth;
        float DashTimer = PlayerAnimationManager.DashTimer - Time.time;
        float ClampedDashTimer = Mathf.Clamp(1 - DashTimer, 0, 1);
        DashCooldownBar.fillAmount = ClampedDashTimer;
    }
}
