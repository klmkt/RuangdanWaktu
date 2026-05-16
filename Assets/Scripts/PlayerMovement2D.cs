using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float fallingForce = 2.5f;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Rigidbody2D rb2d;
    private float moveInput;
    private bool jumpQueued;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private ContactFilter2D groundFilter;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        groundFilter = new ContactFilter2D
        {
            useLayerMask = false,
            useTriggers = false
        };
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb2d.velocity;
        velocity.x = moveInput * speed;

        if (jumpQueued && IsGrounded())
        {
            velocity.y = jumpPower;
        }

        if (velocity.y < 0f)
        {
            velocity += Vector2.up * Physics2D.gravity.y * (fallingForce - 1f) * Time.fixedDeltaTime;
        }

        rb2d.velocity = velocity;
        jumpQueued = false;
    }

    private bool IsGrounded()
    {
        return rb2d.Cast(Vector2.down, groundFilter, groundHits, groundCheckDistance) > 0;
    }
}
