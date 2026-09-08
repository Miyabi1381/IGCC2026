using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("--- CORE PHYSICS & BALANCING ---")]
    public float Speed;
    public float JumpForce;
    public float DefaultGravityScale = 4f;

    [Header("--- GROUND CHECK CONFIGURATION ---")]
    public Transform GroundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask GroundLayer;

    [Header("--- CELESTE WALL MECHANICS ---")]
    public Transform WallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);
    [Space(5)]
    public float WallSlideSpeed = 2f;
    public float WallClimbSpeed = 5f;
    public Vector2 WallJumpForce = new Vector2(12f, 14f);
    public LayerMask WallLayer;

    [Header("--- CELESTE STAMINA SYSTEM ---")]
    public float MaxClimbStamina = 4f;
    private float currentStamina;
    private bool isExhausted;

    [Header("--- CELESTE DASH MECHANICS ---")]
    public float DashSpeed = 25f;
    public float DashTime = 0.15f;
    public float DashCooldown = 0.2f;

    // --- PRIVATE INTERNAL STATES ---
    private Rigidbody2D rb;
    private float moveInput;
    private Vector2 fullMoveInput;

    private bool IsFacingRight = true;
    private bool isGrounded = true;
    private bool isDoubleJump = false;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isClimbing;
    private bool holdClimbInput;
    private bool isDashing;
    private bool canDash = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentStamina = MaxClimbStamina;
    }

    void Update()
    {
        CheckGrounded();
        CheckWallStatus();
        DetermineMovementStates();
        HandleStaminaDecay();

        if (!isDashing && !isClimbing)
        {
            if ((moveInput > 0 && !IsFacingRight) || (moveInput < 0 && IsFacingRight))
            {
                IsFacingRight = !IsFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        HandleHorizontalAndClimbMovement();
        HandleWallSliding();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(GroundCheck.position, groundCheckSize, 0f, GroundLayer);

        if (isGrounded)
        {
            isDoubleJump = false;
            canDash = true;
            currentStamina = MaxClimbStamina; // Full stamina refresh on ground touch
            isExhausted = false;
        }
    }

    private void CheckWallStatus()
    {
        if (WallCheck != null)
        {
            isTouchingWall = Physics2D.OverlapBox(WallCheck.position, wallCheckSize, 0f, WallLayer);
        }
    }

    private void DetermineMovementStates()
    {
        // WALL SLIDE: True if pushing against a wall, airborne, falling, and NOT holding grab (or exhausted)
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && (!holdClimbInput || isExhausted))
            isWallSliding = true;
        else
            isWallSliding = false;

        // WALL CLIMB/HOLD: Only true if touching a wall, holding grab, and NOT exhausted
        if (isTouchingWall && !isGrounded && holdClimbInput && !isExhausted)
            isClimbing = true;
        else
            isClimbing = false;
    }

    private void HandleStaminaDecay()
    {
        if (isClimbing)
        {
            // Drain stamina over time while climbing or clinging
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; // Forcibly drops the grab state
            }
        }
    }

    private void HandleHorizontalAndClimbMovement()
    {
        if (isClimbing)
        {
            // Vertical movement up/down walls using raw Y input
            rb.linearVelocity = new Vector2(0, fullMoveInput.y * WallClimbSpeed);
            rb.gravityScale = 0f; // Freeze gravity completely while holding
            return;
        }

        // Correctly restore regular gravity when running, falling, or standard sliding
        rb.gravityScale = DefaultGravityScale;
        rb.linearVelocity = new Vector2(moveInput * Speed, rb.linearVelocity.y);
    }

    private void HandleWallSliding()
    {
        if (isWallSliding)
        {
            // Caps the falling speed to simulate wall friction drag
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -WallSlideSpeed);
        }
    }

    // --- INPUT SYSTEM RECEIVERS ---

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        fullMoveInput = inputVector;
        moveInput = inputVector.x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            // Priority 1: Grounded Jump
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                isDoubleJump = false;
            }
            // Priority 2: Wall Jump (Executes if touching a wall, even if sliding or grabbing)
            // CRITICAL: This explicitly skips checking or consuming "isDoubleJump"!
            else if (isTouchingWall && !isGrounded)
            {
                // Push away from the wall direction based on sprite facing target
                int pushDirection = IsFacingRight ? -1 : 1;
                rb.linearVelocity = new Vector2(pushDirection * WallJumpForce.x, WallJumpForce.y);

                // Flip player tracking instantly to face away from the wall they kicked off of
                IsFacingRight = !IsFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
            // Priority 3: Air Double Jump (Only works if completely out in the open air)
            else if (!isGrounded && !isDoubleJump && !isTouchingWall)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                isDoubleJump = true;
            }
        }
    }

    public void OnClimb(InputValue value)
    {
        holdClimbInput = value.isPressed;
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDashing && !isClimbing)
        {
            StartCoroutine(PerformCelesteDash());
        }
    }

    private IEnumerator PerformCelesteDash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDirection = Vector2.zero;

        if (fullMoveInput.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(fullMoveInput.y, fullMoveInput.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            dashDirection = new Vector2(Mathf.Cos(snappedAngle * Mathf.Deg2Rad), Mathf.Sin(snappedAngle * Mathf.Deg2Rad));
        }
        else
        {
            dashDirection = new Vector2(IsFacingRight ? 1f : -1f, 0f);
        }

        rb.linearVelocity = dashDirection * DashSpeed;

        yield return new WaitForSeconds(DashTime);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y * 0.5f);
        isDashing = false;

        yield return new WaitForSeconds(DashCooldown);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (GroundCheck != null) Gizmos.DrawWireCube(GroundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        if (WallCheck != null) Gizmos.DrawWireCube(WallCheck.position, wallCheckSize);
    }
}
