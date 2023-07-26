using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public InputActionReference movementAction;
    public InputActionReference jumpAction;
    public InputActionReference bounceAction;
    private Vector2 movementInput;

    public float moveSpeed = 8f;
    public float jumpForce = 18f;
    public float bounceForce = 250f;
    public float verticalBounceForceOffset = 5f;
    public float knockbackForce = 20f;
    public bool isBouncing = false;
    private bool isGrounded = false;
    public bool facingRight = true;
    public int bouncesRemaining = 0;
    public int maxBounces = 4;
    public int totalHealth = 5;
    public int healthCount;
    public float maxVelocity = 25f;
    private bool isHit;
    private int hitStunTimer = 0;
    public int hitStunTime = 10;
    public float bounceDelay = 0.03f;

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
        if (bouncesRemaining > 0 && isBouncing && rb.velocity.y != 0f)
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
            if (hitStunTimer == 0)
            {
                healthCount--;
            }
            hitStunTimer++;
            if (hitStunTimer == hitStunTime)
            {
                isHit = false;
                hitStunTimer = 0;
            }
        }
        else
        {
            if (!isBouncing)
            {
                rb.velocity = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);
                if (Mathf.Abs(rb.velocity.y) < 0.0001f && isGrounded)
                {
                    bouncesRemaining = maxBounces;
                }
            }
        }

        //Curbs large magnitudes
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet" && !isBouncing)
        {
            isHit = true;
        }
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

            if (collision.collider.tag == "Hazard" && !isHit)// Detected Hazard
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
                rb.velocity = calculateKnockbackForce(normal); // adds knockback from getting hit
            }
            else if (collision.collider.tag == "Enemy")
            {
                if (isBouncing && bouncesRemaining > 0)
                {
                    bouncesRemaining++;
                    Bounce(normal, rb.velocity);
                }
                else
                {
                    isHit = true;
                    rb.velocity = calculateKnockbackForce(normal);
                }
            }
            else if (isBouncing)// Detected WAF and is bouncing
            {
                Bounce(normal, rb.velocity);

                //rb.velocity = temp;
                //rb.AddForce(calculateBounceForce(normal), ForceMode2D.Force);
                //rb.isKinematic = false;
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
    private void Bounce(Vector2 normal, Vector2 currentVelocity)
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        StartCoroutine(BounceCoroutine(normal, currentVelocity));
    }
    private Vector2 calculateBounceForce(Vector2 normal)
    {
        //Checks collision from cardinal directions
        if (normal == new Vector2(0, -1))
        {
            //collision on up
            if (rb.velocity.x > 0 || (rb.velocity.x == 0 && facingRight))
            {
                return new Vector2(Vector2.right.x * bounceForce, Vector2.down.y * bounceForce / verticalBounceForceOffset);
            }
            else
            {
                return new Vector2(Vector2.left.x * bounceForce, Vector2.down.y * bounceForce / verticalBounceForceOffset);
            }
        }
        else if (normal == new Vector2(0, 1))
        {

            if (rb.velocity.x > 0 || (rb.velocity.x == 0 && facingRight))
            {
                return new Vector2(Vector2.right.x * bounceForce, Vector2.up.y * bounceForce / verticalBounceForceOffset);
            }
            else
            {
                return new Vector2(Vector2.left.x * bounceForce, Vector2.up.y * bounceForce / verticalBounceForceOffset);
            }
        }
        else if (normal == new Vector2(-1, 0))
        {
            //collision on right
            return new Vector2(Vector2.left.x * bounceForce,rb.velocity.y * bounceForce / verticalBounceForceOffset);
        }
        else if (normal == new Vector2(1, 0))
        {
            //collision on left
            return new Vector2(Vector2.right.x * bounceForce,rb.velocity.y * bounceForce / verticalBounceForceOffset);
        }
        else//Checks collision from diagonal directions
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
        if (normal == new Vector2(0, -1))
        {
            //collision on up
            return Vector2.down * knockbackForce;
        }
        else if (normal == new Vector2(0, 1))
        {
            //collision on down
            return Vector2.up * knockbackForce;
        }
        else if (normal == new Vector2(-1, 0))
        {
            //collision on right
            return Vector2.left * knockbackForce;
        }
        else if (normal == new Vector2(1, 0))
        {
            //collision on left
            return Vector2.right * knockbackForce;
        }
        else//Checks collision from diagonal directions
        {
            if (normal.x < 0)
            {
                if (normal.y < 0)
                {
                    //collision upper right 
                    if (normal.x >= -0.6f)
                    {
                        return Vector2.left * knockbackForce;
                    }
                    else
                    {
                        return Vector2.down * knockbackForce;
                    }
                }
                else
                {
                    //collision downward right
                    if (normal.x >= -0.6f)
                    {
                        return Vector2.left * knockbackForce;
                    }
                    else
                    {
                        return Vector2.up * knockbackForce;
                    }
                }
            }
            else
            {
                if (normal.y < 0)
                {
                    //collision upper  left
                    if (normal.x <= 0.6f)
                    {
                        return Vector2.right * knockbackForce;
                    }
                    else
                    {
                        return Vector2.down * knockbackForce;
                    }
                }
                else
                {
                    //collision downard left
                    if (normal.x <= 0.6f)
                    {
                        return Vector2.right * knockbackForce;
                    }
                    else
                    {
                        return Vector2.up * knockbackForce;
                    }
                }
            }
        }
    }

    private IEnumerator BounceCoroutine(Vector2 normal, Vector2 savedVelocity)
    {
        yield return new WaitForSeconds(bounceDelay);
        Debug.Log("coroutine");
        rb.isKinematic = false;
        rb.velocity = savedVelocity;
        rb.AddForce(calculateBounceForce(normal), ForceMode2D.Force);
        

        // Wait for a specified duration before proceeding to the next bounce.
        
    }
}