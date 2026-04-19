using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionSceneToggle : MonoBehaviour
{
    public static OptionSceneToggle Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private string optionSceneName = "Option";
    private bool optionSceneLoaded = false;
    private bool isTransitioning = false;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            if (optionSceneLoaded)
                UnloadOptionScene();
            else
                LoadOptionScene();
        }
    }

    public void LoadOptionScene()
    {
        if (optionSceneLoaded || isTransitioning) return;

        isTransitioning = true;
        SceneFader.Instance.FadeToSceneAdditive(optionSceneName);
        StartCoroutine(WaitThenSetLoaded(true));
    }

    public void UnloadOptionScene()
    {
        if (!optionSceneLoaded || isTransitioning) return;

        isTransitioning = true;
        SceneFader.Instance.FadeAndUnloadScene(optionSceneName);
        StartCoroutine(WaitThenSetLoaded(false));

    }

    private System.Collections.IEnumerator WaitThenSetLoaded(bool state)
    {
        yield return new WaitForSecondsRealtime(0.1f); // Slightly longer than fadeDuration
        optionSceneLoaded = state;
        isTransitioning = false;
    }
}
