using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputActionReference movementAction;
    public InputActionReference jumpAction;
    public InputActionReference bounceAction;
    private Vector2 movementInput;

    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float bounceForce = 50f;
    public float knockbackForce = 6100f;
    private bool isBouncing = false;
    private bool isGrounded = false;
    public bool facingRight = true;
    public int bouncesRemaining = 0;
    public int maxBounces = 4;
    public int totalHealth = 5;
    public int healthCount;
    public int maxVelocity = 30;
    private bool isHit;


    private Rigidbody2D rb;
    private PhysicsMaterial2D playerPhysicsMaterial;
    public CircleCollider2D cCollider;
    private SpriteRenderer spriteRenderer;

    public Sprite bounceMode;
    public Sprite normalMode;

    private void Awake()
    {
        isHit = false;
        healthCount = totalHealth;
        cCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerPhysicsMaterial = GetComponent<Collider2D>().sharedMaterial as PhysicsMaterial2D;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normalMode;

        //Ensure starting with 0 bounciness
        playerPhysicsMaterial.bounciness = 0;
        //Recompute collider shape to update bounciness
        cCollider.enabled = false;
        cCollider.enabled = true;
    }
    private void OnEnable()
    {
        movementAction.action.Enable();
        movementAction.action.performed += OnMovementPerformed;
        movementAction.action.canceled += OnMovementCanceled;

        jumpAction.action.Enable();
        jumpAction.action.started += OnJumpStarted;

        bounceAction.action.Enable();
        bounceAction.action.performed += OnBouncePerformed;
    }
    private void OnDisable()
    {
        movementAction.action.Disable();
        movementAction.action.performed -= OnMovementPerformed;
        movementAction.action.canceled -= OnMovementCanceled;

        jumpAction.action.Disable();
        jumpAction.action.started -= OnJumpStarted;

        bounceAction.action.Disable();
        bounceAction.action.performed -= OnBouncePerformed;
    }
    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        if (!isBouncing)
        {
            if (movementInput.x < 0)
            {
                facingRight = false;
            }
            else
            {
                facingRight = true;
            }
        }
    }
    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero;
    }
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

    }
    private void OnBouncePerformed(InputAction.CallbackContext context)
    {
        isBouncing = !isBouncing;
        if (bouncesRemaining > 0 && isBouncing)
        {

            rb.AddForce(Vector2.up * jumpForce / 5f, ForceMode2D.Impulse);
            playerPhysicsMaterial.bounciness = 1.0f;
            spriteRenderer.sprite = bounceMode;
        }
        else
        {
            isBouncing = false;
            playerPhysicsMaterial.bounciness = 0;
            spriteRenderer.sprite = normalMode;
        }
        //Recompute collider shape to update bounciness
        cCollider.enabled = false;
        cCollider.enabled = true;
    }
    private void FixedUpdate()
    {
        float moveHorizontal = movementInput.x;

        if (isHit)
        {
            healthCount--;
            isHit = false;
        }
        if (!isBouncing)
        {
            rb.velocity = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);
            if (Mathf.Abs(rb.velocity.y) < 0.0001f && isGrounded)
            {
                bouncesRemaining = maxBounces;
            }
        }
        //Curbs large magnitudes
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBouncing)
        {
            bouncesRemaining--;
        }
        if (bouncesRemaining < 1)
        {
            isBouncing = false;
            playerPhysicsMaterial.bounciness = 0;
            cCollider.enabled = false;
            cCollider.enabled = true;
            spriteRenderer.sprite = normalMode;
        }
        if (collision.contacts.Length != 0)
        {
            Vector2 normal = collision.contacts[0].normal;

            if (collision.collider.tag == "Hazard")// Detected Hazard
            {
                isHit = true;
                //rb.velocity = new Vector2 (rb.velocity.x, 0f);
                if (isBouncing)
                {
                    isBouncing = false;
                    playerPhysicsMaterial.bounciness = 0;
                    cCollider.enabled = false;
                    cCollider.enabled = true;
                    spriteRenderer.sprite = normalMode;
                }
                rb.velocity = new Vector2(rb.velocity.x, calculateKnockbackForce(normal).y * 3.5f); // adds knockback from getting hit
            }
            else if (collision.collider.tag == "Enemy")
            {
                if (isBouncing && bouncesRemaining < 0)
                {
                    bouncesRemaining++;
                }
                else
                {
                    healthCount--;
                }
            }
            else if (isBouncing)// Detected WAF and is bouncing
            {
                rb.AddForce(calculateBounceForce(normal), ForceMode2D.Force);
                rb.velocity = Vector2.ClampMagnitude(rb.velocity, 100f);
            }
            else// Detected WAF collision from the bottom and isn't bouncing to determined grounded
            {
                if (normal.y > 0)
                {
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }
            }
        }
    }
    private Vector2 calculateBounceForce(Vector2 normal)
    {
        if (normal == new Vector2(0, -1))
        {
            //collision on up
            //return Vector2.down * bounceForce;

            if (rb.velocity.x > 0)
            {
                Debug.Log("right");
                return new Vector2(Vector2.right.x * bounceForce * 5, Vector2.down.y * bounceForce);
            }
            else if (rb.velocity.x < 0)
            {
                Debug.Log(rb.velocity.x);
                Debug.Log("left");
                return new Vector2(Vector2.left.x * bounceForce * 5, Vector2.down.y * bounceForce);
            }
            else if (facingRight)
            {
                return new Vector2(Vector2.right.x * bounceForce * 5, Vector2.down.y * bounceForce);
            }
            else
            {
                return new Vector2(Vector2.left.x * bounceForce * 5, Vector2.down.y * bounceForce);
            }
        }
        else if (normal == new Vector2(0, 1))
        {
            //collision on down
            //return Vector2.up * bounceForce;
            if (rb.velocity.x > 0)
            {
                Debug.Log("right");
                return new Vector2(Vector2.right.x * bounceForce * 5, Vector2.up.y * bounceForce);
            }
            else if (rb.velocity.x < 0)
            {
                Debug.Log(rb.velocity.x);
                Debug.Log("left");
                return new Vector2(Vector2.left.x * bounceForce * 5, Vector2.up.y * bounceForce);
            }
            else if (facingRight)
            {
                return new Vector2(Vector2.right.x * bounceForce * 5, Vector2.up.y * bounceForce);
            }
            else
            {
                return new Vector2(Vector2.left.x * bounceForce * 5, Vector2.up.y * bounceForce);
            }
        }
        else if (normal == new Vector2(-1, 0))
        {
            //collision on right
            return new Vector2(Vector2.left.x * bounceForce * 5, bounceForce * rb.velocity.y);
        }
        else if (normal == new Vector2(1, 0))
        {
            //collision on left
            return new Vector2(Vector2.right.x * bounceForce * 5, bounceForce * rb.velocity.y);
        }
        else
        {
            if (normal.x < 0)
            {
                if (normal.y < 0)
                {
                    //collision upper right 
                    return new Vector2(-bounceForce, -bounceForce);
                }
                else
                {
                    //collision downward right
                    return new Vector2(-bounceForce, bounceForce);
                }
            }
            else
            {
                if (normal.y < 0)
                {
                    //collision upper  left
                    return new Vector2(bounceForce, -bounceForce);
                }
                else
                {
                    //collision downard left
                    return new Vector2(bounceForce, bounceForce);
                }
            }
        }
    }
    private Vector2 calculateKnockbackForce(Vector2 normal)
    {
        return Vector2.up * knockbackForce;
    }
}