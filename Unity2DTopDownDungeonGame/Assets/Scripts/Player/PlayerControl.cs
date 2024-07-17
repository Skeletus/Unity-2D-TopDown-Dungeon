using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("The player WeaponShootPosition gameobject in the hierachy")]
    #endregion
    [SerializeField] private Transform weaponShootPosition;

    private Player player;

    private void Awake()
    {
        // load components
        player = GetComponent<Player>();
    }

    private void Update()
    {
        // process player movement input
        MovementInput();

        // process player weapon input
        WeaponInput();
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
        player.idleEvent.CallIdleEvent();
    }
}
