using UnityEngine;

public class DangerMarkFlicker : MonoBehaviour
{
    [HideInInspector] public Transform shell;
    [HideInInspector] public float groundY;

    public float baseFlickerSpeed = 1f;
    public float maxFlickerSpeed = 6f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.6f;

    private float flickerTimer;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (shell == null || spriteRenderer == null) return;

        float shellDistance = Mathf.Max(0.01f, shell.position.y - groundY);
        float flickerSpeed = Mathf.Lerp(maxFlickerSpeed, baseFlickerSpeed, shellDistance / 10f);

        flickerTimer += Time.deltaTime * flickerSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(flickerTimer * Mathf.PI * 2f) + 1f) / 2f);

        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
