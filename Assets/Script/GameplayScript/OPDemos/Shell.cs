using FirstGearGames.SmoothCameraShaker;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shell : MonoBehaviour
{
    [Header("Shell Settings")]
    public float fallSpeed = 5f;
    public GameObject explosionPrefab;
    public ShakeData shakeData;

    [Header("Warning Circle")]
    public GameObject warningCirclePrefab;  // Prefab with SpriteRenderer or LineRenderer
    public float groundY = -3.5f;           // Fixed Y for all warning markers
    private GameObject currentWarningCircle;

    [Header("Game Over Settings")]
    public GameObject gameOverTriggerPrefab;  // Assign a prefab with BoxCollider2D
    public float timeSlowFactor = 0.3f;
    public float restartDelay = 2f;

    private Rigidbody2D rb;

    void OnEnable()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.velocity = Vector2.down * fallSpeed;

        // Spawn the warning circle at fixed Y
        if (warningCirclePrefab != null)
        {
            Vector3 warningPos = new Vector3(transform.position.x, groundY, 0f);
            currentWarningCircle = Instantiate(warningCirclePrefab, warningPos, Quaternion.identity);

            DangerMarkFlicker flicker = currentWarningCircle.GetComponent<DangerMarkFlicker>();
            if (flicker != null)
            {
                flicker.shell = transform;
                flicker.groundY = groundY;
            }
        }

    }




    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            Explode();
            CameraShakerHandler.Shake(shakeData);
            PlayerController player = other.GetComponent<PlayerController>();
            ChargingController charging = other.GetComponent<ChargingController>();
            if (player != null)
            {
                player.DieByBomb();
                charging.DieByBomb();
            }
        }
    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // Play explosion sound separately
            AudioSource audio = explosion.GetComponent<AudioSource>();
            if (audio != null)
            {
                GameObject soundObj = new GameObject("ExplosionSound");
                soundObj.transform.position = explosion.transform.position;

                AudioSource sound = soundObj.AddComponent<AudioSource>();
                sound.clip = audio.clip;
                sound.volume = audio.volume;
                sound.spatialBlend = audio.spatialBlend;
                sound.reverbZoneMix = audio.reverbZoneMix;
                sound.dopplerLevel = audio.dopplerLevel;
                sound.rolloffMode = audio.rolloffMode;
                sound.pitch = audio.pitch;
                sound.minDistance = audio.minDistance;
                sound.maxDistance = audio.maxDistance;
                sound.Play();

                Destroy(soundObj, sound.clip.length);
            }

            if (currentWarningCircle != null)
                currentWarningCircle.SetActive(false);

            Destroy(explosion, 0.3f);
        }

        // Placeholder: eventually trigger game over here
        //Debug.Log("[Shell] Explosion occurred. Game Over logic goes here.");

        // Optionally slow time (if testing)
        // Time.timeScale = 0.5f;

        gameObject.SetActive(false);
    }

    float FindGroundY()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
        return hit.collider != null ? hit.point.y : transform.position.y - 5f;
    }

    IEnumerator RestartAfterDelay()
    {
        Time.timeScale = timeSlowFactor;

        // ✅ Fade to black using FadeManager
        if (FadeManager.Instance != null && FadeManager.Instance.screenFadeGroup != null)
        {
            yield return FadeManager.Instance.FadeIn(FadeManager.Instance.screenFadeGroup);
        }

        float t = 0f;
        while (t < restartDelay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        // 🔁 Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
