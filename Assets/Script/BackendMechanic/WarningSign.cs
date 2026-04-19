using UnityEngine;

public class WarningSignController : MonoBehaviour
{
    [Header("Blink Settings")]
    public float maxBlinkRate = 0.05f;     // Fastest blink rate
    public float minBlinkRate = 0.5f;      // Slowest blink rate
    public float maxDistance = 15f;        // Distance where blinking is slowest

    private SpriteRenderer spriteRenderer;
    private Transform shellTransform;
    private bool isActive = false;
    private float currentBlinkRate;
    private float blinkTimer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false; // Hide by default
    }

    void Update()
    {
        if (!isActive) return;

        // Calculate the distance
        float distance = Mathf.Abs(shellTransform.position.y - transform.position.y);

        // Adjust blink rate
        currentBlinkRate = Mathf.Lerp(maxBlinkRate, minBlinkRate, distance / maxDistance);

        // Blinking logic
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= currentBlinkRate)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            blinkTimer = 0f;
        }
    }

    // Called by Shell.cs when activated
    public void Activate(Transform shell)
    {
        shellTransform = shell;
        isActive = true;
        spriteRenderer.enabled = true;
    }

    // Called by Shell.cs when exploded
    public void Deactivate()
    {
        isActive = false;
        spriteRenderer.enabled = false;
    }
}
