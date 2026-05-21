using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    private const string DeathColliderName = "ColliderMati";

    private const float GroundNormalThreshold = 0.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private string footstepSoundName = "footstep";
    [SerializeField] private string landingSoundName = "fall";
    [SerializeField] private float footstepInterval = 0.45f;
    [SerializeField] private float footstepSpeedThreshold = 0.1f;
    [SerializeField] private float landingMinVelocity = 1f;

    private float horizontalInput;
    private bool isFacingRight;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float footstepTimer;
    private float lastVerticalVelocity;
    private bool isGrounded;
    private readonly HashSet<Collider2D> groundedColliders = new HashSet<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (rb == null)
        {
            Debug.LogError("PlayerMovement2D requires a Rigidbody2D component.", this);
            enabled = false;
        }

        if (audioManager == null)
        {
            audioManager = AudioManager.instance;
            if (audioManager == null)
            {
                audioManager = Object.FindFirstObjectByType<AudioManager>();
            }
        }
    }

    private void Start()
    {
        if (audioManager == null)
        {
            audioManager = AudioManager.instance;
            if (audioManager == null)
            {
                audioManager = Object.FindFirstObjectByType<AudioManager>();
            }
        }
    }

    private void Update()
    {
        if (rb == null)
        {
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        FlipSprite();
        HandleAnimations();

        if (IsJumpPressed() && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        lastVerticalVelocity = rb.velocity.y;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        CheckGround();
        HandleFootsteps();
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            if (Mathf.Abs(horizontalInput) > 0.001f)
            {
                spriteRenderer.flipX = horizontalInput < 0f;
            }

            return;
        }

        if ((isFacingRight && horizontalInput < 0f) || (!isFacingRight && horizontalInput > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.gameObject.name == DeathColliderName)
        {
            RestartLevel();
            return;
        }

        if (groundCheck != null)
        {
            return;
        }

        if (TryAddGroundContact(collision))
        {
            UpdateGroundedState(groundedColliders.Count > 0);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider == null)
        {
            return;
        }

        if (groundCheck != null)
        {
            return;
        }

        if (TryAddGroundContact(collision))
        {
            UpdateGroundedState(groundedColliders.Count > 0);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == null)
        {
            return;
        }

        if (groundCheck != null)
        {
            return;
        }

        if (groundedColliders.Remove(collision.collider))
        {
            UpdateGroundedState(groundedColliders.Count > 0);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool TryAddGroundContact(Collision2D collision)
    {
        if (!IsGroundCollision(collision))
        {
            return false;
        }

        groundedColliders.Add(collision.collider);
        return true;
    }

    private bool IsGroundCollision(Collision2D collision)
    {
        if (collision == null)
        {
            return false;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y >= GroundNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateGroundedState(bool groundedNow)
    {
        bool wasGrounded = isGrounded;
        isGrounded = groundedNow;

        if (!wasGrounded && isGrounded)
        {
            TryPlayLanding();
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            return;
        }

        bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        UpdateGroundedState(groundedNow);
    }

    private void HandleAnimations()
    {
        if (animator == null || rb == null)
        {
            return;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("YVelocity", rb.velocity.y);
    }

    private bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }

    private void HandleFootsteps()
    {
        if (!isGrounded || Mathf.Abs(horizontalInput) < 0.01f || Mathf.Abs(rb.velocity.x) < footstepSpeedThreshold)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer += Time.fixedDeltaTime;
        if (footstepTimer >= footstepInterval)
        {
            TryPlayFootstep();
            footstepTimer = 0f;
        }
    }

    private void TryPlayFootstep()
    {
        if (audioManager == null || string.IsNullOrWhiteSpace(footstepSoundName))
        {
            return;
        }

        audioManager.PlaySFX(footstepSoundName);
    }

    private void TryPlayLanding()
    {
        if (audioManager == null || string.IsNullOrWhiteSpace(landingSoundName))
        {
            return;
        }

        if (lastVerticalVelocity <= -landingMinVelocity)
        {
            audioManager.PlaySFX(landingSoundName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
