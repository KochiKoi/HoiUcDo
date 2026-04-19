using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IControllable
{
    [Header("Move Settings")]
    [SerializeField] public float moveSpeed; // Movement speed
    [SerializeField] public float acceleration; // Acceleration rate
    [SerializeField] public float deceleration; // Deceleration rate
    private float currentSpeed = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;


    private float moveInput;
    [SerializeField] private bool isGrounded;

    // Climb System
    private bool canClimb = false; // To check if the player can climb
    private bool isClimbing = false; // To check if the player is currently climbing
    [SerializeField] private float climbSpeed; // Speed at which the player climbs

    //Pickup,Drop,Throw Item System
    [Header("Throw Settings")]
    public float throwForce = 5f;
    public float minForce = 3f;
    public float maxForce = 15f;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 30;
    public float timeStep = 0.05f;
    private bool isDragging;
    private Vector2 startMousePos;
    private Vector2 currentMousePos;
    //hold item
    [Header("Item Handling")]
    [SerializeField] private Transform itemholder;
    private GameObject helditem;
    private bool InRange;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get horizontal input only if grounded
        if (isGrounded)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            moveInput = 0f; // Disable movement if not grounded
        }

        // Start climbing when 'Space' is held and the player is in a climbable area
        if (canClimb && Input.GetKey(KeyCode.Space))
        {
            StartClimbing();
            animator.SetBool("isClimbing", true); // In StartClimbing
        }

        // Stop climbing and start sliding down when 'Space' is released
        if (canClimb && !Input.GetKey(KeyCode.Space))
        {
            StopClimbing();
            animator.SetBool("isClimbing", false); // In StopClimbing
        }

        //Pickup,Drop Item 
        HandlePickupDrop();
        HandleThrowing();
    }

    private void FixedUpdate()
    {
        // If climbing, disable horizontal movement
        if (isClimbing)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement
            return;
        }

        // If the player changes direction, reset current speed immediately
        if (moveInput != 0 && Mathf.Sign(moveInput) != Mathf.Sign(currentSpeed))
        {
            currentSpeed = 0; // Reset speed for instant direction change
        }

        // Accelerate & decelerate normally
        if (moveInput != 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed * moveInput, acceleration * Time.fixedDeltaTime);

        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);

        }
        animator.SetBool("isMoving", moveInput != 0 && isGrounded);

        // Apply velocity
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

        // Flip sprite based on direction
        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;
    }

    // **Detect when entering a Climb Enabled Area**
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ClimbEnabled"))
        {
            canClimb = true;
        }

        //trigger to comparetag Player
        if (collision.CompareTag("Player"))
        {
            //thêm animator
            Debug.Log("Player has move on the Trigger");
        }
    }

    // **Detect when exiting a Climb Enabled Area**
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ClimbEnabled"))
        {
            canClimb = false;
            StopClimbing(); // Stop climbing if exiting the climbable area
        }
    }

    // **Detect when player is on the ground**
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Make sure "Ground" is tagged
        {
            isGrounded = true; // Player is touching the ground
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // Player is no longer touching the ground
        }
    }

    // **Start climbing when holding 'Space'**
    private void StartClimbing()
    {
        if (!isClimbing)
        {
            isClimbing = true;
            rb.velocity = Vector2.zero; // Stop other movement
            rb.isKinematic = true; // Disable physics during climbing
        }

        // Move the player upward while holding 'Space'
        rb.velocity = new Vector2(rb.velocity.x, climbSpeed);
    }

    // **Stop climbing and let gravity pull the player down when releasing 'Space'**
    private void StopClimbing()
    {
        isClimbing = false;
        rb.isKinematic = false; // Re-enable physics
    }

    //funtion pickup,drop,throw Item System

    private void HandlePickupDrop()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (helditem == null)
                PickUpItem();
            else
                DropItem();
        }
    }

    private void HandleThrowing()
    {
        if (helditem == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (isDragging && Input.GetMouseButton(1))
        {
            currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rawForce = (startMousePos - currentMousePos) * throwForce;
            float clampedMagnitude = Mathf.Clamp(rawForce.magnitude, minForce, maxForce);
            Vector2 clampedForce = rawForce.normalized * clampedMagnitude;

            ShowTrajectory(clampedForce);
            animator.SetTrigger("throw");
        }

        if (isDragging && Input.GetMouseButtonUp(1))
        {
            isDragging = false;
            currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rawForce = (startMousePos - currentMousePos) * throwForce;
            float clampedMagnitude = Mathf.Clamp(rawForce.magnitude, minForce, maxForce);
            Vector2 clampedForce = rawForce.normalized * clampedMagnitude;

            ThrowItem(clampedForce);

            if (trajectoryLine != null)
                trajectoryLine.enabled = false;
        }
    }
    private void PickUpItem()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position + Vector3.right * transform.localScale.x * 0.5f, 0.6f, LayerMask.GetMask("Item"));

        if (hit != null)
        {
            GameObject itemObj = hit.gameObject;
            itemObj.transform.SetParent(itemholder);
            itemObj.transform.localPosition = Vector2.zero;
            helditem = itemObj;

            Rigidbody2D itemRb = helditem.GetComponent<Rigidbody2D>();
            SpriteRenderer sr = helditem.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
            if (itemRb != null)
            {
                itemRb.gravityScale = 0f;
                itemRb.isKinematic = true;
                itemRb.velocity = Vector2.zero;
                itemRb.angularVelocity = 0f;
            }
        }
    }

    private void DropItem()
    {
        helditem.transform.SetParent(null);

        Rigidbody2D rb = helditem.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = helditem.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }
        if (rb != null)
        {
            rb.isKinematic = false;
            //thêm animator.enable=false;
        }

        helditem = null;
    }

    private void ThrowItem(Vector2 force)
    {
        helditem.transform.SetParent(null);

        Rigidbody2D itemRb = helditem.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = helditem.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }
        if (itemRb != null)
            if (itemRb != null)
        {
            itemRb.isKinematic = false;
            itemRb.gravityScale = 5f;
            itemRb.velocity = force;
        }
        else
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            helditem = null;
        }
        Item triggerStop = helditem.GetComponent<Item>();
        if (triggerStop != null)
        {
            triggerStop.ResetLanding();
        }

        helditem = null;

    }


    private void ShowTrajectory(Vector2 force)
    {
        if (trajectoryLine == null || helditem == null) return;

        trajectoryLine.enabled = true;

        Vector2 pos = helditem.transform.position;
        Rigidbody2D itemRb = helditem.GetComponent<Rigidbody2D>();
        float mass = itemRb != null ? itemRb.mass : 1f;
        Vector2 velocity = force / mass;

        trajectoryLine.positionCount = trajectoryPoints;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector2 point = pos + velocity * t + 0.5f * Physics2D.gravity * t * t;
            trajectoryLine.SetPosition(i, point);
        }
    }

    public void EnableControl()
    {
        enabled = true;
    }

    public void DisableControl()
    {
        enabled = false;
    }
    public void DieByGun()
    {
        animator.SetTrigger("dieByGun");
        DisableControl();
    }

    public void DieByBomb()
    {
        animator.SetTrigger("dieByBomb");

        // Optional: stop movement
        //rb.velocity = Vector2.zero;
        //rb.simulated = false;

        // Delay actual disabling so animation plays
        StartCoroutine(DelayedDisableControl());
    }

    IEnumerator DelayedDisableControl()
    {
        yield return new WaitForSeconds(1f); // Delay should match your death animation length
        DisableControl();
    }


}
