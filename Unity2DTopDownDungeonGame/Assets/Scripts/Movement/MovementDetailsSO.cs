using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/Movement Details")]
public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion Header

    #region Tooltip
    [Tooltip("The minimun move speed. The GetMoveSpeed method calculates a random value between the minimun and maximum")]
    #endregion
    public float minMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("The maximun move speed. The GetMoveSpeed method calculates a random value between the minimun and maximum")]
    #endregion
    public float maxMoveSpeed = 8f;

    /// <summary>
    /// Get a random movement speed between the minimum and maximum values
    /// </summary>
    /// <returns></returns>
    public float GetMoveSpeed()
    {
        if (minMoveSpeed == maxMoveSpeed)
        {
            return minMoveSpeed;
        }
        else
        {
            return Random.Range(minMoveSpeed, maxMoveSpeed);
        }
    }

    #region VALIDATION
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minMoveSpeed), minMoveSpeed,
            nameof(maxMoveSpeed), maxMoveSpeed, false);
    }
#endif
    #endregion
}
