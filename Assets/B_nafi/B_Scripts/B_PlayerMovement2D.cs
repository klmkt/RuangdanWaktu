using UnityEngine;

public class PlayerMovement_nafi : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;

    private float moveInput;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        HandleAnimations();

        Flip();
    }

    private void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    private void Move()
    {
        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );
    }

    private void Jump()
    {
        rb.velocity = new Vector2(
            rb.velocity.x,
            jumpForce
        );
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    private void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        animator.SetBool("Grounded", isGrounded);

        animator.SetFloat("YVelocity", rb.velocity.y);
    }

    private void Flip()
    {
        if (moveInput > 0)
        {
            // flip by flip SpriteRenderer
            // GetComponent<SpriteRenderer>().flipX = false;
            sr.flipX = false;

        }
        else if (moveInput < 0)
        {
            // flip by flip SpriteRenderer
            // GetComponent<SpriteRenderer>().flipX = true;
            sr.flipX = true;

        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundRadius
        );
    }
}