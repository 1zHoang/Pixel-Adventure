﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr => GetComponent<SpriteRenderer>();
    protected Transform player;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D[] colliders;

    //[SerializeField] protected Transform player;
    [Header("Genaral info")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float idleDuration = 1.5f;
    protected float idleTimer;
    protected bool canMove = true;
    //[SerializeField] protected float idleTimer;

    [Header("Death details")]
    [SerializeField] protected float deathImpactSpeed = 5;
    [SerializeField] protected float deathRotationSpeed = 150;
    protected int deathRotationDirection = 1;
    protected bool isDead;

    [Header("Basic collison")]
    [SerializeField] protected float groundCheckDistance = 1.1f;
    [SerializeField] protected float wallCheckDistance = .7f;
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected float playerDetectionDistance = 15;
    [SerializeField] protected LayerMask whatIsPlayer;
    [SerializeField] protected Transform groundCheck;
    protected bool isPlayerDetected;
    protected bool isGrounded;
    protected bool isWallDetected;
    protected bool isGroundInfrontDetected;

    protected int facingDir = -1;
    protected bool facingRight = false;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    protected virtual void Start()
    {
        if(sr.flipX == true && !facingRight)
        {
            sr.flipX = false;
            Flip();
        }

        PlayerManager.OnPlayerRespawn += UpdatePlayerReference;
    }
    
    private void UpdatePlayerReference()
    {
        if(player == null)
            player = PlayerManager.instance.player.transform;
    }

    protected virtual void Update()
    {
        HandleCollison();
        HandleAnimator();

        idleTimer -= Time.deltaTime;

        if(isDead)
            HandleDeathRotation();
    }
    
    public virtual void Die()
    {
        if (rb.isKinematic)
            rb.isKinematic = false;
        EnableColliders(false);

        anim.SetTrigger("hit");
        rb.velocity = new Vector2(rb.velocity.x, deathImpactSpeed);
        isDead = true;

        if (Random.Range(0, 100) < 50)
            deathRotationDirection = deathRotationDirection * -1;

        PlayerManager.OnPlayerRespawn -= UpdatePlayerReference;
        Destroy(gameObject, 10);
    }

    protected void EnableColliders(bool enable)
    {
        foreach (var collider in colliders)
        {
            collider.enabled = enable;
        }
    }

    private void HandleDeathRotation()
    {
        transform.Rotate(0, 0, (deathRotationSpeed * deathRotationDirection) * Time.deltaTime);
    }

    protected virtual void HandleFlip(float xValue)
    {
        if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight)
            Flip();
    }
    protected virtual void Flip()
    {
        facingDir = facingDir * -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    [ContextMenu("Change Facing Direction")]
    public void FlipDefaultFacingDirection()
    {
        sr.flipX = !sr.flipX;
    }

    protected virtual void HandleAnimator()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
    }

    protected virtual void HandleCollison()
    {
        //Kiểm tra xem có phải mặt đất không
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isGroundInfrontDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        isPlayerDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, playerDetectionDistance, whatIsPlayer);
    }
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (playerDetectionDistance * facingDir), transform.position.y));
    }
 }
