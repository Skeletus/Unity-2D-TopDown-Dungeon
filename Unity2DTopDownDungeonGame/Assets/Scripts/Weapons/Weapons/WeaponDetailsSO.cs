using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    #region Header WEAPON BASE DETAILS
    [Space(10)]
    [Header("WEPON BASE DETAILS")]
    #endregion Header WEAPON BASE DETAILS
    #region Tooltip
    [Tooltip("Weapon name")]
    #endregion Tooltip
    public string weaponName;

    #region Tooltip
    [Tooltip("The sprite for the weapon - the sprite should have the " +
        " generate physics shape option selected")]
    #endregion
    public Sprite weaponSprite;

    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("WEAPON CONFIGURATION")]
    #endregion Header WEAPON CONFIGURATION
    #region Tooltip
    [Tooltip("Weapon shoot position - the offset position for the end of the weapon" +
        " from the sprite pivot point")]
    #endregion Tooltip
    public Vector3 weaponShootPosition;
    
    #region Tooltip
    [Tooltip("Weapon current ammo")]
    #endregion Tooltip
    public AmmoDetailsSO weaponCurrentAmmo;

    #region Header WEAPON OPERATING VALUES
    [Space(10)]
    [Header("WEAPON OPERATING VALUES")]
    #endregion Header WEAPON OPERATING VALUES
    #region Tooltip
    [Tooltip("Select if the weapon has infinite ammo")]
    #endregion
    public bool hasInfiniteAmmo = false;

    #region Tooltip
    [Tooltip("Select if the weapon has infinite clip capacity")]
    #endregion
    public bool hasInfiniteClipCapacity = false;

    #region Tooltip
    [Tooltip("The weapon capacity - shots before a reload")]
    #endregion
    public int weaponClipAmmoCapacity = 6;

    #region Tooltip
    [Tooltip("Weapon ammo capacity - the maximum number of rounds at" +
        " that can be held for this weapon")]
    #endregion
    public int weaponAmmoCapacity = 100;

    #region Tooltip
    [Tooltip("Weapon fire rate - 0.2 means 5 shots a second")]
    #endregion Tooltip
    public float weaponFireRate = 0.2f;

    #region Tooltip
    [Tooltip("Weapon precharge time - time in seconds to hold fire " +
        "button down before firing")]
    #endregion Tooltip
    public float weaponPreChargeTime = 0f;

    #region Tooltip
    [Tooltip("This is the weapon reload time in seconds")]
    #endregion Tooltip
    public float weaponReloadTime = 0f;

    #region VALIDATION
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyStrings(this, nameof(weaponName), weaponName);
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponCurrentAmmo), weaponCurrentAmmo);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponFireRate), weaponFireRate, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPreChargeTime), weaponPreChargeTime, true);

        if (!hasInfiniteAmmo)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponAmmoCapacity), weaponAmmoCapacity, false);
        }
        if (!hasInfiniteClipCapacity)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponClipAmmoCapacity), weaponClipAmmoCapacity, false);
        }
    }
#endif
    #endregion
}
