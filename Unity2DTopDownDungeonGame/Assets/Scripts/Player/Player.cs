using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
#endregion REQUIRE COMPONENTS
public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerDetailsSO playerDetails;
    [HideInInspector] public Health health;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;

    private void Awake()
    {
        // load components
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Initialize player
    /// </summary>
    /// <param name="playerDetails"></param>
    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;

        // set the player starting health
        SetPlayerHealth();
    }

    /// <summary>
    /// set player health from player details scriptable object
    /// </summary>
    private void SetPlayerHealth()
    {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
    }
}
