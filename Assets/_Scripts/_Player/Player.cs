using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject fruitDrop;
    [SerializeField] private DifficultyType gameDifficulty;
    private GameManager gameManager;

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;//khai báo biến private mà vẫn muốn chỉnh sửa thì dùng [SerializeField]
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float defaultGravityScale;
    private bool canDoubleJump;

    [Header("Buffer & Coyote Jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpActivated = -1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpActivated = -1;

    [Header("Wall interactions")]
    [SerializeField] private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1;
    [SerializeField] private Vector2 knockbackPower;
    private bool isknocked;
    //private bool canBeKnocked;

    [Header("Collison")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [Space]
    [SerializeField] private Transform enemyCheck;
    [SerializeField] private float enemyCheckRadius;
    [SerializeField] private LayerMask whatIsEnemy;
    private bool isGrounded;
    private bool isAirborne;
    private bool isWallDetected;


    private Joystick joystick;
    private float xInput;
    private float yInput;
    private float joinStickxInput;
    private float joinStickyInput;

    private bool facingRight = true;
    private int facingDir = 1;

    [Header("Player Visuals")]
    [SerializeField] private AnimatorOverrideController[] animators;
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private ParticleSystem dustFx;
    [SerializeField] private int skinId;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();

        FindFirstObjectByType<UI_JumpButton>().UpdatePlayerRef(this);
        joystick = FindFirstObjectByType<Joystick>();
    }
    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        gameManager = GameManager.instance;

        UpdateGameDifficulty();
        RespawnFinished(false);
        UpdateSkin();
    }

    private void Update()
    {
        UpdateAirbornStatus();

        if (canBeControlled == false)
        {
            HandleCollison();
            HandelAnimations();
            return;
        }

        if (isknocked)
            return;

        HandleEnemyDetection();
        //thiết lập phím A,D: A thì xInput = -1, D thì xInput = 1
        HandleInput();
        HandleWallSlide();
        HandelAnimations();
        HandleFlip();
        HandleCollison();
        HandleMovement();

    }

    public void Damage()
    {
        if (gameDifficulty == DifficultyType.Normal)
        {
            
            if(gameManager.FruitsCollected() <= 0)
            {
                Die();
                gameManager.RestartLevel();
            }
            else
            {
                ObjectCreator.instance.CreateObject(fruitDrop, transform, true);
                gameManager.RemoveFruit();
            }

            return;

        }

        if(gameDifficulty == DifficultyType.Hard)
        {
            Die();
            gameManager.RestartLevel();
        }
    }

    public void UpdateGameDifficulty()
    {
        DifficultyManager difficultyManager = DifficultyManager.instance;

        if (difficultyManager != null)
            gameDifficulty = difficultyManager.difficulty;
    }

    public void UpdateSkin()
    {
        SkinManager skinManager = SkinManager.instance;

        if (skinManager == null)
            return;

        anim.runtimeAnimatorController = animators[skinManager.choosenSkinId];
    }

    private void HandleEnemyDetection()
    {
        if (rb.velocity.y >= 0)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyCheck.position, enemyCheckRadius, whatIsEnemy);

        foreach (var enemy in colliders)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();
            if (newEnemy != null)
            {
                AudioManager.instance.PlaySFX(1);
                newEnemy.Die();
                Jump();
            }
        }
    }

    public void RespawnFinished(bool finished)
    {
        
        if (finished)
        {
            rb.gravityScale = defaultGravityScale;
            canBeControlled = true;
            cd.enabled = true;

            AudioManager.instance.PlaySFX(11);
        }
        else
        {
            rb.gravityScale = 0;
            canBeControlled = false;
            cd.enabled = false;
        }
    }
    public void Knockback(float sourceDamageXPosition)
    {
        float knockbackDir = 1;

        if (transform.position.x < sourceDamageXPosition)
            knockbackDir = -1;
        if (isknocked)
            return;

        AudioManager.instance.PlaySFX(9);

        CameraManager.instance.ScreenShake(knockbackDir);

        StartCoroutine(KnockbackRoutine());
        //anim.SetTrigger("knockback");
        rb.velocity = new Vector2(knockbackPower.x * knockbackDir, knockbackPower.y);
    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;
        float yModifer = yInput < 0 ? 1 : .05f;

        if (canWallSlide == false)
            return;
        //if (yInput < 0)
        //{
        //    yModifer = 1;
        //}
        //else
        //{
        //    yModifer = .05f;
        //}
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifer);
    }

    private void UpdateAirbornStatus()
    {
        if (isGrounded && isAirborne)
            HandleLanding();
        if (!isGrounded && !isAirborne)
            BecomeAirborne();
    }

    private void BecomeAirborne()
    {
        isAirborne = true;
        if (rb.velocity.y < 0)
        {
            ActivateCoyoteJump();
        }
    }

    private void HandleLanding()
    {
        dustFx.Play();

        isAirborne = false;
        canDoubleJump = true;
        AttemptBufferJump();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        joinStickxInput = joystick.Horizontal;
        joinStickyInput = joystick.Vertical;

        xInput = Mathf.Abs(joinStickxInput) > Mathf.Abs(xInput) ? joinStickxInput : xInput;
        yInput = Mathf.Abs(joinStickyInput) > Mathf.Abs(yInput) ? joinStickyInput : yInput;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpButton();
        }
    }

    public void JumpButton()
    {
        JumpAttempt();
        RequestBufferJump();
    }

    private void RequestBufferJump()
    {
        if (isAirborne)
            bufferJumpActivated = Time.time;
    }
    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivated + bufferJumpWindow)
        {
            bufferJumpActivated = Time.time - 1;
            Jump();
        }
    }
    private void ActivateCoyoteJump() => coyoteJumpActivated = Time.time;
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;
    private void JumpAttempt()
    {
        bool coyoteJumpAvalible = Time.time < coyoteJumpActivated + coyoteJumpWindow;

        if (isGrounded || coyoteJumpAvalible)
        {
            Jump();
        }
        else if (isWallDetected && !isGrounded)
        {
            WallJump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
        }

        CancelCoyoteJump();
    }


    private void Jump()
    {
        dustFx.Play();
        AudioManager.instance.PlaySFX(3);

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    } 
    private void DoubleJump()
    {
        dustFx.Play();
        AudioManager.instance.PlaySFX(3);

        isWallJumping = false;
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }
    private void WallJump()
    {
        dustFx.Play();
        AudioManager.instance.PlaySFX(12);

        rb.velocity = new Vector2(wallJumpForce.x * -facingDir, wallJumpForce.y);
        canDoubleJump = true;
        //Debug.Log("canDoubleJump set to: " + canDoubleJump);
        //Debug.Log("isWallDetected set to: " + isWallDetected);
        //Debug.Log("iswalljumping set to: " + isWallJumping);
        Flip();
        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }
    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }
    private IEnumerator KnockbackRoutine()
    {
        isknocked = true;
        anim.SetBool("isKnocked", true);
        yield return new WaitForSeconds(knockbackDuration);
        isknocked = false;
        anim.SetBool("isKnocked", false);
    }

    public void Die()
    {
        AudioManager.instance.PlaySFX(0);

        GameObject newDeathVfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void Push(Vector2 direction, float duration = 0)
    {
        StartCoroutine(PushCouroutine(direction, duration));
    }

    private IEnumerator PushCouroutine(Vector2 direction, float duration)
    {
        canBeControlled = false;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        canBeControlled = true;
    }

    private void HandleCollison()
    {
        //Kiểm tra xem có phải mặt đất không
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void HandelAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleMovement()
    {
        if (isWallDetected)
            return;
        if (isWallJumping)
            return;
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
    }
    private void HandleFlip()
    {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
        {
            Flip();
        }
    }
    private void Flip()
    {
        facingDir = facingDir * -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(enemyCheck.position, enemyCheckRadius);
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }
}
