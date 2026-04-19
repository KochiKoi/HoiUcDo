using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneRestartHandler : MonoBehaviour
{
    public static SceneRestartHandler Instance;

    [Header("Fade Reference")]
    public CanvasGroup screenFadeGroup; // Assign in Inspector

    [Header("Restart Settings")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 1.5f;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: Uncomment if you want to persist across scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerSceneRestart()
    {
        StartCoroutine(RestartSequence());
    }

    private IEnumerator RestartSequence()
    {
        // Fade to black
        if (screenFadeGroup != null)
        {
            yield return FadeCanvas(screenFadeGroup, 0f, 1f, fadeDuration);
        }

        // Slow down time
        Time.timeScale = slowTimeScale;
        float t = 0f;
        while (t < slowDuration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Reset time
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        cg.gameObject.SetActive(true);
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    public void FadeInFromBlack()
    {
        if (screenFadeGroup != null)
            StartCoroutine(FadeCanvas(screenFadeGroup, 1f, 0f, fadeDuration));
    }
}
