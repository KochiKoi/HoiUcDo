using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

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

    // Fade and Load normal scene
    public void FadeToScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(FadeLoadRoutine(sceneName, additive: false));
        }
        else
        {
            Debug.LogWarning("SceneFader: No scene name assigned!");
        }
    }

    // Fade and Load additive scene
    public void FadeToSceneAdditive(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(FadeLoadRoutine(sceneName, additive: true));
        }
        else
        {
            Debug.LogWarning("SceneFader: No scene name assigned!");
        }
    }

    // Fade and Unload scene
    public void FadeAndUnloadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(FadeUnloadRoutine(sceneName));
        }
        else
        {
            Debug.LogWarning("SceneFader: No scene name assigned!");
        }
    }

    // Fade helper (returns IEnumerator to properly yield)
    public IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        fadeCanvas.blocksRaycasts = true;
        fadeCanvas.alpha = from;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
        fadeCanvas.blocksRaycasts = (to == 1); // Only block if fully black
    }

    // Internal Load Routine
    private IEnumerator FadeLoadRoutine(string sceneName, bool additive)
    {
        // First fade to black fully
        yield return StartCoroutine(Fade(0, 1));

        // THEN start loading scene
        if (additive)
        {
            Debug.Log("Loading Scene Additive: " + sceneName);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
        else
        {
            Debug.Log("Loading Scene: " + sceneName);
            SceneManager.LoadScene(sceneName); // Non-async reload
        }

        // Short optional wait (tiny buffer)
        yield return new WaitForSeconds(0.1f);

        // Fade back from black
        yield return StartCoroutine(Fade(1, 0));
    }

    // Internal Unload Routine
    private IEnumerator FadeUnloadRoutine(string sceneName)
    {
        // First fade to black fully
        yield return StartCoroutine(Fade(0, 1));

        // THEN start unloading
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.Log("Unloading Scene: " + sceneName);
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            while (!unloadOp.isDone)
                yield return null;
        }
        else
        {
            Debug.LogWarning($"SceneFader: Scene '{sceneName}' is not loaded!");
        }

        // Fade back from black
        yield return StartCoroutine(Fade(1, 0));
    }
}
