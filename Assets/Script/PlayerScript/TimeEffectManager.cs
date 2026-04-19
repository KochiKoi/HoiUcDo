using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimeEffectManager : MonoBehaviour
{
    public static TimeEffectManager Instance;

    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerQuickSlowdown(float targetScale, float holdDuration, float smoothReturnDuration)
    {
        StartCoroutine(QuickSlowdownRoutine(targetScale, holdDuration, smoothReturnDuration));
    }

    private IEnumerator QuickSlowdownRoutine(float targetScale, float holdTime, float restoreDuration)
    {
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(holdTime);

        float t = 0f;
        float startScale = Time.timeScale;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / restoreDuration;
            Time.timeScale = Mathf.Lerp(startScale, 1f, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    // NEW: Pause game time completely (used when opening Option scene)
    public void PauseTime()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("Game Paused via TimeEffectManager.");
    }

    public void ResumeTime()
    {
        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("Game Resumed via TimeEffectManager.");
    }
}
