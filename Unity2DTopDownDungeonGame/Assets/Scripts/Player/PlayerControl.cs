using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;

    #region Tooltip
    [Tooltip("The player WeaponShootPosition gameobject in the hierachy")]
    #endregion
    [SerializeField] private Transform weaponShootPosition;

    private Player player;
    private float moveSpeed;
    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool isPlayerRolling = false;
    private float playerRollCooldownTimer = 0f;

    private void Awake()
    {
        // load components
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        // create waitforfixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();
    }

    private void Update()
    {
        // if player is rolling then return
        if (isPlayerRolling)
        {
            return;
        }

        // process player movement input
        MovementInput();

        // process player weapon input
        WeaponInput();

        // player roll cooldown timer
        PlayerRollCooldownTimer();
    }

    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees;
        float playerAngleDegrees;
        AimDirection playerAimDirection;

        // aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees,
            out playerAngleDegrees, out playerAimDirection);
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        // get mouse world position
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // calculate direction vector of mouse cursor from weapon shoot position
        weaponDirection = (mouseWorldPosition - weaponShootPosition.position);

        // calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // set player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        // trigger weapon aim event
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees,
            weaponAngleDegrees, weaponDirection);
    }

    /// <summary>
    /// Player movement input
    /// </summary>
    private void MovementInput()
    {
        // get movement input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        // check if right click was pressed
        bool rightMouseBottonDown = Input.GetMouseButtonDown(1);

        // create a direction vector based on the input
        Vector2 direction = new Vector2(horizontalInput, verticalInput);

        // adjust distance for diagonal movement 
        if (horizontalInput != 0f && verticalInput != 0f)
        {
            direction *= 0.7f;
        }

        // if there is movement either move or roll
        if (direction != Vector2.zero)
        {
            if (!rightMouseBottonDown)
            {
                // trigger movement speed
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
            }
            else if (playerRollCooldownTimer <= 0f)
            {
                PlayerRoll((Vector3)direction);
            }
        }
        else
        {
            player.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Player roll
    /// </summary>
    /// <param name="direction"></param>
    private void PlayerRoll(Vector3 direction)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    /// <summary>
    /// Player roll coroutine
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        // min distance used to decide when to exite coroutine loop
        float minDistance = 0.2f;

        isPlayerRolling = true;

        Vector3 targetPosition = player.transform.position + (Vector3)direction * 
            movementDetails.rollDistance;

        while(Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition,
                player.transform.position, movementDetails.rollSpeed,
                direction, isPlayerRolling);

            // yield and wait for fixed update
            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        // set cooldownTimer 
        playerRollCooldownTimer = movementDetails.rollCooldownTime;

        player.transform.position = targetPosition;
    }

    private void PlayerRollCooldownTimer()
    {
        if (playerRollCooldownTimer >= 0f)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if collided with something stop player roll coroutine
        StopPlayerRollRoutine();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // if in collision with something stop player roll coroutine
        StopPlayerRollRoutine();
    }

    private void StopPlayerRollRoutine()
    {
        if (playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);

            isPlayerRolling = false;
        }
    }

    #region VALIDATION
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
