using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;
    public Transform targetTransform; //Object the camera will follow
    public Transform cameraPivot;
    private PlayerCombatManager playerCombatManager;
    private float defaultPosition;
    public Transform cameraTransform;
    private Vector3 camerafollowVelocity = Vector3.zero;
    public LayerMask collisionLayers; //things the camera will colide with
    public LayerMask EnemyLayers; //Things that can be checked for lock on
    public LayerMask PlayerAndEnvironemnt;
    private Vector3 cameraVectorPosition;

    public float CameraFollowSpeed = .2f; //timer for how long it takes to catch up to the player

    public float lookAngle; //up and down
    public float pivotAngle;  //left and right

    public float cameraLookSpeed = 2;
    public float cameraPivotSpeed = 2;

    public float minPivotAngle = -35f;
    public float maxPivotAngle = 35f;

    public float cameraCollisionRadius = 0.2f;
    public float cameraCollisionOffset = 0.5f; // how much the camera moves away from objects it is touching
    public float minimumCollisionOffset = 0.2f;

    //Lock on
    private float lockOnRadius = 20f;
    public float lockOnTargetFollowSpeed = .2f;
    public List<Enemy> AvalibleTargets = new List<Enemy> ();
    public Enemy nearestLockOnTarget;
    public Enemy LeftLockOnTarget;
    public Enemy RightLockOnTarget;


    private void Awake()
    {
        playerCombatManager = FindObjectOfType<PlayerCombatManager>();
        targetTransform = FindObjectOfType<PlayerManager>().transform;
        inputManager = FindObjectOfType<InputManager>();
        defaultPosition = cameraTransform.localPosition.z;
    }
    private void FollowTarget()
    {
        Vector3 TargetPosition = Vector3.SmoothDamp(transform.position, targetTransform.position, ref camerafollowVelocity, CameraFollowSpeed);
        transform.position = TargetPosition;
    }

    private void RotateCamera()
    {
        PlayerCombatManager combatmanager = FindObjectOfType<PlayerCombatManager>();
        if (combatmanager.isLockedOn)
        {
            Transform enemylockontransform = combatmanager.enemy.EnemyLockOnPoint;
            Vector3 rotationdirection = enemylockontransform.position - transform.position;
            rotationdirection.Normalize();
            rotationdirection.y = 0;

            Quaternion targetrotation = Quaternion.LookRotation(rotationdirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetrotation, lockOnTargetFollowSpeed);

            rotationdirection = enemylockontransform.position - cameraPivot.position;
            rotationdirection.Normalize();

            targetrotation = Quaternion.LookRotation(rotationdirection);
            cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, targetrotation, lockOnTargetFollowSpeed);

            lookAngle = transform.eulerAngles.y;
            pivotAngle = cameraPivot.transform.eulerAngles.x;

        }
        else
        {
            Vector3 rotation;
            

            pivotAngle = Mathf.Clamp(pivotAngle, minPivotAngle, maxPivotAngle);

            rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            transform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;
            targetRotation = Quaternion.Euler(rotation);
            cameraPivot.localRotation = targetRotation;

            lookAngle = lookAngle + (inputManager.cameraInputX * cameraLookSpeed);
            pivotAngle = pivotAngle - (inputManager.cameraInputY * cameraPivotSpeed);
        }
    }

    public void HandleAllCameraMovment()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    public void HandleCameraCollisions()
    {
        float targetposition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivot.transform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetposition), collisionLayers))
        {
            
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetposition =- (distance - cameraCollisionOffset);       
                
        }

        if (Mathf.Abs(targetposition) < minimumCollisionOffset)
        {
            targetposition = targetposition - minimumCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetposition, .2f);
        cameraTransform.localPosition = cameraVectorPosition;   
    }

    public void HandleLocatingLockOnTargets()
    {
        
        float shortestDistance = Mathf.Infinity;
        
        Collider[] colliders = Physics.OverlapSphere(targetTransform.position, lockOnRadius, EnemyLayers);
        
        for (int i = 0; i < colliders.Length; i++)
        {
            
            Enemy enemy = colliders[i].GetComponent<Enemy>();
            if (enemy != null)
            {

                Vector3 lockOnTargetsDirection = enemy.transform.position - targetTransform.position;
                float distanceFromTarget = Vector3.Distance(targetTransform.position, enemy.transform.position);
                float veiwableAngle = Vector3.Angle(lockOnTargetsDirection, cameraTransform.position);

                if (enemy.isDead)
                {
                    playerCombatManager.SetTarget(enemy);
                    continue;
                }

                    RaycastHit hit;
                    
                if (Physics.Linecast(playerCombatManager.PlayerLockOnTransform.position, enemy.EnemyLockOnPoint.position, out hit, PlayerAndEnvironemnt))
                {
                    Debug.Log("Do not have LOS from the player");
                    continue;
                }
                else 
                {
                    //Add current target to the lock on list
                    AvalibleTargets.Add(enemy);
                }

            } 
        }
        
        //Sorting through targets
        for (int k = 0; k < AvalibleTargets.Count; k++)
        {
            if (AvalibleTargets[k] != null)
            {
                float distanceFromTarget = Vector3.Distance(targetTransform.position, AvalibleTargets[k].transform.position);
                Vector3 lockTargetsDirection = AvalibleTargets[k].transform.position - targetTransform.position;

                //Finding Nearest Traget
                if (distanceFromTarget < shortestDistance)
                {
                    shortestDistance = distanceFromTarget;
                    nearestLockOnTarget = AvalibleTargets[k];
                    PlayerCombatManager combatManager = targetTransform.GetComponent<PlayerCombatManager>();
                    combatManager.SetTarget(nearestLockOnTarget);
                    combatManager.isLockedOn = true;
                }

                //Finding Left/Right Target
                
            }
            else
            {
                ClearLockOnTargets();
                playerCombatManager.isLockedOn = false;
            }
            
        }
    }
    public void HandleLocatingNewLockOnTarget()
    {
        float shortDistanceOfRightTarget = Mathf.Infinity;
        float shortDistanceOfLeftTarget = -Mathf.Infinity;
        for (int k = 0; k < AvalibleTargets.Count; k++)
        {
            if (playerCombatManager.isLockedOn)
            {
                Vector3 RelativeEnemyPosition = targetTransform.transform.InverseTransformPoint(AvalibleTargets[k].transform.position);
                var distanceFromLeftTarget = RelativeEnemyPosition.x;
                var distanceFromRightTarget = RelativeEnemyPosition.x;


                if (AvalibleTargets[k] == playerCombatManager.enemy)
                {
                    continue;
                }

                //Check For Left Target
                if (RelativeEnemyPosition.x <= 0.00 && distanceFromLeftTarget > shortDistanceOfLeftTarget)
                {
                    shortDistanceOfLeftTarget = distanceFromLeftTarget;
                    LeftLockOnTarget = AvalibleTargets[k];

                }
                if (RelativeEnemyPosition.x >= 0.00 && distanceFromRightTarget < shortDistanceOfRightTarget)
                {
                    shortDistanceOfRightTarget = distanceFromRightTarget;
                    RightLockOnTarget = AvalibleTargets[k];
                }
            }
        }
        
    }
    public void HandleChangingLockOnTargets()
    {
        if (playerCombatManager.isLockedOn && inputManager.cameraInput.x > 0)
        {
            inputManager.cameraInput.x = 0;
            HandleLocatingLockOnTargets();

            if (LeftLockOnTarget != null)
            {
                playerCombatManager.SetTarget(LeftLockOnTarget);
            }
        }
        
    }

    public void ClearLockOnTargets()
    {
        //Set camera Back to default
        nearestLockOnTarget = null;
        LeftLockOnTarget = null;
        RightLockOnTarget = null;
        AvalibleTargets.Clear();
    
    }
}
