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
    public Vector2 WallJumpForce = new Vector2(12f, 14f);
    public LayerMask WallLayer;

    [Header("--- WALL JUMP TUNING ---")]
    public float WallJumpLockTime = 0.15f; // how long horizontal input is ignored after a wall jump
    private float wallJumpLockTimer = 0f;

    [Header("--- CELESTE STAMINA SYSTEM ---")]
    public float MaxClimbStamina = 4f;
    private float currentStamina;
    private bool isExhausted;

    [Header("--- CELESTE DASH MECHANICS ---")]
    public float DashSpeed = 25f;
    public float DashTime = 0.15f;

    [Header("--- COYOTE TIME & JUMP BUFFER ---")]
    public float CoyoteTime = 0.1f;
    public float JumpBufferTime = 0.1f;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;

    [Header("--- ABILITY UNLOCKS ---")]
    public bool HasDoubleJumpUnlocked = false;

    [Header("--- DEATH ---")]
    public LayerMask ObstacleLayer;
    private Sprite deathPoseSprite;  // assigned per-spawn by DeathManager, matching this variant
    private Sprite skeletonSprite;   // assigned per-spawn by DeathManager, matching this variant
    private bool isDead = false;

    [Header("--- DEBUG ---")]
    public bool DebugKillKey = true; // toggle off before a real build

    [Header("--- ANIMATION ---")]
    private Animator PlayerAnim;

    [Header("--- RANDOM POSE VARIANTS ---")]
    public Sprite[] JumpSprites;       // e.g. jamp1, jamp2 — one picked at random each time you leave the ground
    public Sprite[] WallClimbSprites;  // e.g. wallwalk1, wallwalk2 — one picked at random each time you grab a wall
    private bool wasAirborneForJumpPose = false;
    private bool wasClimbingForPose = false;

    [Header("--- RESOURCE TINT INDICATORS ---")]
    public Color DashAvailableTint = Color.white;
    public Color DashUsedTint = new Color(0.3f, 0.55f, 1f, 1f);        // blue while dash is spent
    public Color JumpsFullTint = Color.white;                          // grounded / haven't jumped yet
    public Color SingleJumpUsedTint = new Color(0.85f, 0.7f, 1f, 1f);  // light purple after the first jump
    public Color DoubleJumpAvailableFlashColor = Color.white;          // peak color the flash oscillates toward
    public float DoubleJumpFlashSpeed = 6f;                            // higher = faster flicker

    // --- PRIVATE INTERNAL STATES ---
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private float moveInput;
    private Vector2 fullMoveInput;

    private bool IsFacingRight = true;
    private bool isGrounded = true;
    private bool wasGroundedStable = false;
    private bool isDoubleJump = false;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isClimbing;
    private bool holdClimbInput;
    private bool isDashing;
    private bool canDash = true;

    // --- PUBLIC ACCESSORS (for UI / external systems) ---
    public float StaminaPercent => currentStamina / MaxClimbStamina;
    public bool IsClimbing => isClimbing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        PlayerAnim = GetComponentInChildren<Animator>();
        currentStamina = MaxClimbStamina;

        if (DeathManager.Instance != null)
            DeathManager.Instance.RegisterPlayer(gameObject); // safety net — DeathManager already registers this synchronously on spawn, but this covers cases where a player is placed manually in the scene
    }

    void Update()
    {
        CheckGrounded();
        CheckWallStatus();
        DetermineMovementStates();
        HandleStaminaDecay();
        HandleJumpBuffer();
        HandleAnimationAndPoseState(); // drives Idle/Walk via Animator, and Jump/WallClimb via direct sprite override
        ApplyResourceTint(); // colors the sprite based on dash/jump resource usage

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

        if (DebugKillKey && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            Die();
        }
    }

    // Handles three visual states:
    // - Wall climbing: picks a random pose once per grab, Animator disabled so it can't fight the override
    // - Airborne (jumping/falling, not climbing): picks a random pose once per takeoff, Animator disabled
    // - Grounded, not climbing: normal Animator-driven Idle/Walk, re-enabled if it was off
    private void HandleAnimationAndPoseState()
    {
        if (isClimbing)
        {
            if (!wasClimbingForPose && WallClimbSprites != null && WallClimbSprites.Length > 0)
            {
                spriteRenderer.sprite = WallClimbSprites[Random.Range(0, WallClimbSprites.Length)];
            }

            if (PlayerAnim != null) PlayerAnim.enabled = false;
            wasClimbingForPose = true;
            wasAirborneForJumpPose = false; // reset so a fresh jump after letting go picks a new pose
        }
        else if (!isGrounded)
        {
            if (!wasAirborneForJumpPose && JumpSprites != null && JumpSprites.Length > 0)
            {
                spriteRenderer.sprite = JumpSprites[Random.Range(0, JumpSprites.Length)];
            }

            if (PlayerAnim != null) PlayerAnim.enabled = false;
            wasAirborneForJumpPose = true;
            wasClimbingForPose = false;
        }
        else
        {
            // Grounded and not climbing — hand control back to the Animator for Idle/Walk
            if (PlayerAnim != null)
            {
                PlayerAnim.enabled = true;
                bool isMoving = fullMoveInput.magnitude > 0;
                PlayerAnim.SetBool("isWalking", isMoving);
            }

            wasAirborneForJumpPose = false;
            wasClimbingForPose = false;
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

        // Dashing never counts as "settled" on ground, even if the dash path grazes it,
        // so mid-air diagonal dashes near the floor can't falsely refresh dash/double jump.
        bool isGroundedStable = isGrounded && !isDashing;

        if (isGroundedStable && !wasGroundedStable)
        {
            isDoubleJump = false;
            canDash = true;
            currentStamina = MaxClimbStamina;
            isExhausted = false;
        }

        if (isGrounded)
            coyoteTimer = CoyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        wasGroundedStable = isGroundedStable;
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

        // WALL HOLD: Only true if touching a wall, holding grab, and NOT exhausted
        if (isTouchingWall && !isGrounded && holdClimbInput && !isExhausted)
            isClimbing = true;
        else
            isClimbing = false;
    }

    private void HandleStaminaDecay()
    {
        if (isClimbing)
        {
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; // Forcibly drops the hold state
            }
        }
    }

    // Colors the sprite to reflect resource state: dash availability and jump count used.
    // Dash and jump tints are computed independently, then multiplied together — so both
    // states can be reflected at once (e.g. dash used while airborne) without needing a
    // separate color defined for every possible combination.
    private void ApplyResourceTint()
    {
        Color dashTint = canDash ? DashAvailableTint : DashUsedTint;

        Color jumpTint;
        if (isGrounded)
        {
            jumpTint = JumpsFullTint;
        }
        else if (!isDoubleJump && HasDoubleJumpUnlocked)
        {
            // Airborne, haven't used the double jump yet, and it's available — flash to signal
            // the player still has a second jump in reserve, oscillating between purple and the flash color.
            float t = (Mathf.Sin(Time.time * DoubleJumpFlashSpeed) + 1f) * 0.5f; // 0..1 oscillation
            jumpTint = Color.Lerp(SingleJumpUsedTint, DoubleJumpAvailableFlashColor, t);
        }
        else
        {
            // Either double jump already used, or the ability isn't unlocked at all — solid, no flash.
            jumpTint = SingleJumpUsedTint;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = dashTint * jumpTint; // Color * Color multiplies component-wise
    }

    private void HandleHorizontalAndClimbMovement()
    {
        if (isClimbing)
        {
            // Static hold only — no climbing up/down. Player can only jump off or let go.
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            return;
        }

        rb.gravityScale = DefaultGravityScale;

        if (wallJumpLockTimer > 0f)
        {
            wallJumpLockTimer -= Time.fixedDeltaTime;
            return; // skip overwriting X velocity so the wall jump push isn't erased
        }

        rb.linearVelocity = new Vector2(moveInput * Speed, rb.linearVelocity.y);
    }

    private void HandleWallSliding()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -WallSlideSpeed);
        }
    }

    private void HandleJumpBuffer()
    {
        if (jumpBufferTimer <= 0f) return;

        jumpBufferTimer -= Time.deltaTime;

        bool canGroundJump = isGrounded || coyoteTimer > 0f;

        if (canGroundJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
            isDoubleJump = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }
        else if (!isGrounded && !isDoubleJump && !isTouchingWall && HasDoubleJumpUnlocked)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
            isDoubleJump = true;
            jumpBufferTimer = 0f;
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
            jumpBufferTimer = JumpBufferTime; // remember the press briefly for HandleJumpBuffer()

            // Wall jump fires immediately — it depends on real-time wall contact,
            // not something worth delaying through the buffer.
            if (!isGrounded && isTouchingWall)
            {
                int pushDirection = IsFacingRight ? -1 : 1;
                rb.linearVelocity = new Vector2(pushDirection * WallJumpForce.x, WallJumpForce.y);
                wallJumpLockTimer = WallJumpLockTime;
                holdClimbInput = false; // force-release hold so isClimbing doesn't re-trigger and cancel the jump

                IsFacingRight = !IsFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;

                jumpBufferTimer = 0f; // consumed, don't also trigger a buffered ground/double jump
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
        // canDash re-enables only via the landing edge check in CheckGrounded() — matches double jump behavior
    }

    // --- ABILITY UNLOCKS ---

    // Called by the double-jump pickup trigger. Routes through DeathManager so the unlock
    // is remembered permanently and auto-applied to every future spawned player, not just this one.
    public void UnlockDoubleJump()
    {
        if (DeathManager.Instance != null)
            DeathManager.Instance.UnlockDoubleJumpPermanently();
        else
            HasDoubleJumpUnlocked = true; // fallback if no manager present (e.g. isolated test scene)
    }

    // --- DEATH SYSTEM ---

    // Called once by DeathManager right after this player is spawned, so the correct
    // death pose / skeleton sprites for THIS variant are ready before they're ever needed.
    public void SetVariantDeathAssets(Sprite deathPose, Sprite skeleton)
    {
        deathPoseSprite = deathPose;
        skeletonSprite = skeleton;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (((1 << collision.gameObject.layer) & ObstacleLayer) != 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        this.enabled = false; // stop all PlayerController logic (Update/FixedUpdate no longer run)
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; // stays solid — this becomes the new ACTIVE corpse

        // Release this object's input device pairing so the next spawned player can claim it.
        // Without this, PlayerInput stays active on the corpse and blocks the new player's auto-pairing.
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = false;

        if (spriteRenderer != null && deathPoseSprite != null)
            spriteRenderer.sprite = deathPoseSprite;

        // Put the corpse on its own layer so it's treated as standable/climbable —
        // works automatically as long as "Corpse" is checked in GroundLayer/WallLayer in the Inspector.
        int corpseLayer = LayerMask.NameToLayer("Corpse");
        if (corpseLayer != -1)
            gameObject.layer = corpseLayer;
        else
            Debug.LogWarning("PlayerController: No layer named 'Corpse' found. Add one in Project Settings > Tags and Layers.");

        if (DeathManager.Instance != null)
            DeathManager.Instance.PlayerDied(gameObject);
    }

    // Called by DeathManager when a newer corpse takes over as the active one.
    public void RetireCorpse()
    {
        rb.simulated = false;              // fully remove from physics
        if (col != null) col.enabled = false; // no more collision — purely visual now

        if (spriteRenderer != null && skeletonSprite != null)
            spriteRenderer.sprite = skeletonSprite;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (GroundCheck != null) Gizmos.DrawWireCube(GroundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        if (WallCheck != null) Gizmos.DrawWireCube(WallCheck.position, wallCheckSize);
    }
}