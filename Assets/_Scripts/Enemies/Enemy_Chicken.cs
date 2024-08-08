using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Chicken : Enemy
{
    //[Header("Chicken details")]
    //[SerializeField] private float aggroDuration;
    //[SerializeField] private float detectionRange;

    //private float aggroTimer;
    //private bool playerDetected;
    //private bool canFlip = true;

    //protected override void Update()
    //{
    //    base.Update();

    //    aggroTimer -= Time.deltaTime;

    //    if (isDead)
    //        return;

    //    if (playerDetected)
    //    {
    //        canMove = true;
    //        aggroTimer = aggroDuration;
    //    }

    //    if(aggroTimer < 0)
    //        canMove = false;

    //    HandleCollison();
    //    HandleMovement();

    //    if (isGrounded)
    //        HandleTurnAround();
    //}

    //private void HandleTurnAround()
    //{
    //    if (!isGroundInfrontDetected || isWallDetected)
    //    {
    //        Flip();
    //        canMove = false;
    //        rb.velocity = Vector2.zero;
    //    }
    //}

    //private void HandleMovement()
    //{
    //    if (canMove == false)
    //        return;

    //    //float xValue = player.position.x;

    //    HandleFlip(player.position.x);

    //    rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
    //}

    //protected override void HandleFlip(float xValue)
    //{
    //    if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight)
    //    {
    //        if (canFlip)
    //        {
    //            canFlip = false;
    //            Invoke(nameof(Flip), .3f);
    //        }
    //    }
    //}

    //protected override void Flip()
    //{
    //    base.Flip();
    //    canFlip = true;
    //}

    //protected override void HandleCollison()
    //{
    //    base.HandleCollison();

    //    playerDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, detectionRange, whatIsPlayer);
    //}
    [Header("Chicken details")]
    [SerializeField] private float aggrDuration;
    [SerializeField] private float detectionRange;
    
    private float aggroTimer;
    private bool playerDetected;
    private bool canFlip = true;

    protected override void Update()
    {
        base.Update();

        anim.SetFloat("xVelocity", rb.velocity.x);
        aggroTimer -= Time.deltaTime;


        if (isDead)
            return;
        
        if (playerDetected)
        {
            canMove = true;
            aggroTimer = aggrDuration;
        }

        if (aggroTimer < 0)
            canMove = false;

        HandleCollison();
        HandleMovement();

        if (isGrounded)
            HandleTurnAround();
    }

    private void HandleTurnAround()
    {
        if (!isGroundInfrontDetected || isWallDetected)
        {
            Flip();
            canMove = false;
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        if (canMove == false)
            return;

        HandleFlip(player.transform.position.x);

        rb.velocity = new Vector2(moveSpeed * facingDir, rb.velocity.y);
    }

    protected override void HandleFlip(float xValue)
    {
        if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight)
        {
            if (canFlip)
            {
                canFlip = false;
                Invoke(nameof(Flip), 3f);
            }
        }
    }

    protected override void Flip()
    {
        base.Flip();
        canFlip = true;
    }

    protected override void HandleCollison()
    {
        base.HandleCollison();

        playerDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, detectionRange, whatIsPlayer);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (detectionRange * facingDir), transform.position.y));
    }
}
