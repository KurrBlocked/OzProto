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
    private bool isBouncing = false;
    private bool isGrounded = false;
    public bool facingRight = true;
    public int bouncesRemaining = 0;
    private int maxBounces = 4;
    public int healthCount = 5;

    private Rigidbody2D rb;
    private PhysicsMaterial2D playerPhysicsMaterial;
    public CircleCollider2D cCollider;
    private SpriteRenderer spriteRenderer;

    public Sprite bounceMode;
    public Sprite normalMode;

    private void Awake()
    {
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
            rb.AddForce(Vector2.up * jumpForce/5, ForceMode2D.Impulse);
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
        
        
        if (!isBouncing)
        { 
            rb.velocity = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);
            if (Mathf.Abs(rb.velocity.y) < 0.0001f && isGrounded)
            {
                bouncesRemaining = maxBounces;
            }
        }
        
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Hazard")
        {
            healthCount--;
            
        }
        if (collision.collider.tag == "Enemy")
        {
            if (isBouncing && bouncesRemaining < 1)
            {
                bouncesRemaining++;
            }
            else
            {
                healthCount--;
            }
        }
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

            if (isBouncing)
            {
                if (normal == new Vector2(0, -1))
                {
                    //collision on up
                    rb.AddForce(Vector2.down * bounceForce, ForceMode2D.Force);
                }
                else if (normal == new Vector2(0, 1))
                {
                    //collision on down
                    rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Force);
                }
                else if (normal == new Vector2(-1, 0))
                {
                    //collision on right
                    rb.AddForce(new Vector2(Vector2.left.x * bounceForce, bounceForce * rb.velocity.y), ForceMode2D.Force);
                }
                else if (normal == new Vector2(1, 0))
                {
                    //collision on left
                    rb.AddForce(new Vector2(Vector2.right.x * bounceForce, bounceForce * rb.velocity.y), ForceMode2D.Force);
                }
                else
                {
                    if (normal.x < 0)
                    {
                        if (normal.y < 0)
                        {
                            //collision upper right 
                            rb.AddForce(new Vector2(-bounceForce, -bounceForce), ForceMode2D.Force);
                        }
                        else
                        {
                            //collision downward right
                            rb.AddForce(new Vector2(-bounceForce, bounceForce), ForceMode2D.Force);
                        }
                    }
                    else
                    {
                        if (normal.y < 0)
                        {
                            //collision upper  left
                            rb.AddForce(new Vector2(bounceForce, -bounceForce), ForceMode2D.Force);
                        }
                        else
                        {
                            //collision downard left
                            rb.AddForce(new Vector2(bounceForce, bounceForce), ForceMode2D.Force);
                        }
                    }

                }
            }
            else
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






    
}