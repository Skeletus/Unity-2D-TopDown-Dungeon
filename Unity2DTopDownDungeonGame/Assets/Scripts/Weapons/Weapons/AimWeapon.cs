using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AimWeaponEvent))]
[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Transform from the child WeaponRotationPoint gameobject")]
    #endregion
    [SerializeField] private Transform weaponRotationPointTransform;

    private AimWeaponEvent aimWeaponEvent;

    private void Awake()
    {
        // load components
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
    }

    private void OnEnable()
    {
        // subscribe to the aim weapon event
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        // unsubscribe to the aim weapon event
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

    /// <summary>
    /// Aim weapon event handler
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    /// <param name="aimDirection"></param>
    /// <param name="aimAngle"></param>
    private void Aim(AimDirection aimDirection, float aimAngle)
    {
        // set the angle of the weapon transform
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        // flip weapon transform based on player direction
        switch(aimDirection)
        {
            case AimDirection.Left:
            case AimDirection.UpLeft:
                // flip in Y axis
                weaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                break;
            case AimDirection.Up:
            case AimDirection.UpRight:
            case AimDirection.Right:
            case AimDirection.Down:
                // flip in X axis
                weaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;
        }
    }

    #region VALIDATIONS
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }
#endif
    #endregion
}
