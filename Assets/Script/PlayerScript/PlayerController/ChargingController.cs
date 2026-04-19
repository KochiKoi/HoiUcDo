using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChargingController : MonoBehaviour, IControllable
{
    [Header("Charging Settings")]
    [SerializeField] private bool enableCharging = false; // Inspector toggle to activate charging
    [SerializeField] private float baseSpeed = 5f;        // Base speed of charging
    [SerializeField] private float minSpeed = 2f;         // Minimum speed limit
    [SerializeField] private float maxSpeed = 10f;        // Maximum speed limit
    [SerializeField] private float acceleration = 0.5f;   // Speed increment value
    [SerializeField] private float deceleration = 0.5f;   // Speed decrement value
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private bool isCharging = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (enableCharging)
        {
            StartCharging();
        }
    }

    void FixedUpdate()
    {
        if (isCharging)
        {
            HandleCharging();
        }
    }

    public void StartCharging()
    {
        isCharging = true;
        //animator.SetBool("isCharging", true);
        animator.SetBool("isMoving", true);
    }

    public void StopCharging()
    {
        isCharging = false;
        //animator.SetBool("isCharging", false);
        animator.SetBool("isMoving", false);
        rb.velocity = Vector2.zero;
    }

    private void HandleCharging()
    {
        float speed = baseSpeed;

        // Increase speed with "D"
        if (Input.GetKey(KeyCode.D))
        {
            speed = Mathf.Min(baseSpeed + acceleration, maxSpeed);
        }

        // Decrease speed with "A"
        if (Input.GetKey(KeyCode.A))
        {
            speed = Mathf.Max(baseSpeed - deceleration, minSpeed);
        }

        // Apply velocity
        rb.velocity = new Vector2(speed, rb.velocity.y);
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
