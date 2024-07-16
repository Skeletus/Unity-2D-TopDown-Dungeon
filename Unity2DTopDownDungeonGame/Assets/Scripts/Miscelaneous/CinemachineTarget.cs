using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{

    private CinemachineTargetGroup cinemachineTargetGroup;

    private void Awake()
    {
        // load component
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void Start()
    {
        SetCinemachineTargetGroup();
    }

    /// <summary>
    /// Set the cinemachine camera target group
    /// </summary>
    private void SetCinemachineTargetGroup()
    {
        // create target group for cinemahcine camera to follow
        CinemachineTargetGroup.Target cinemachineTargetGroup_Player = new CinemachineTargetGroup.Target{
            weight = 1f,
            radius = 1f,
            target = GameManager.Instance.GetPlayer().transform
        };

        CinemachineTargetGroup.Target[] cinemachineTargetGroup_Array = new CinemachineTargetGroup.Target[] { cinemachineTargetGroup_Player};

        cinemachineTargetGroup.m_Targets = cinemachineTargetGroup_Array;
    }
}
