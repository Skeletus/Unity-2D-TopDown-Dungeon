using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float fireRateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private FireWeaponEvent fireWeaponEvent;
    private WeaponFiredEvent weaponFiredEvent;

    private void Awake()
    {
        // Load components.
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
    }

    private void OnEnable()
    {
        // Subscribe to fire weapon event.
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        // Unsubscribe from fire weapon event.
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void Update()
    {
        // Decrease cooldown timer.
        fireRateCoolDownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Handle weapon fire event
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent,
        FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    /// <param name="fireWeaponEventArgs"></param>
    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // Weapon fire.
        if (fireWeaponEventArgs.fire)
        {
            // Test if weapon is ready to fire.
            if (IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponEventArgs.aimAngle,
                        fireWeaponEventArgs.weaponAimAngle,
                        fireWeaponEventArgs.weaponAimDirectionVector);

                ResetCoolDownTimer();
            }
        }
    }

    /// <summary>
    /// Returns true if the weapon is ready to fire, else returns false.
    /// </summary>
    /// <returns></returns>
    private bool IsWeaponReadyToFire()
    {
        // if there is no ammo and weapon doesn't have infinite ammo then return false.
        if (activeWeapon.GetCurrentWeapon().weaponRemainingAmmo <= 0 &&
            !activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteAmmo)
            return false;

        // if the weapon is reloading then return false.
        if (activeWeapon.GetCurrentWeapon().isWeaponReloading)
            return false;

        // If the weapon is cooling down then return false.
        if (fireRateCoolDownTimer > 0f)
            return false;

        // if no ammo in the clip and the weapon doesn't have infinite clip capacity then return false.
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity &&
            activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
        {
            return false;
        }

        // weapon is ready to fire - return true
        return true;
    }

    /// <summary>
    /// Set up ammo using an ammo gameobject and component from the object pool.
    /// </summary>
    /// <param name="aimAngle"></param>
    /// <param name="weaponAimAngle"></param>
    /// <param name="weaponAimDirectionVector"></param>
    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();

        if (currentAmmo != null)
        {
            // get ammo prefab from array
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[Random.Range(0, currentAmmo.ammoPrefabArray.Length)];

            // get random speed value
            float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);

            // get gameobject with IFireable component
            IFireable ammo = (IFireable)PoolManager.Instance.ReUseComponent(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);

            // initialise Ammo
            ammo.InitializeAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            // reduce ammo clip count if not infinite clip capacity
            if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity)
            {
                activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
                activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
            }

            // call weapon fired event
            weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentWeapon());
        }
    }

    /// <summary>
    /// Reset cooldown timer
    /// </summary>
    private void ResetCoolDownTimer()
    {
        // Reset cooldown timer
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponFireRate;
    }
}
