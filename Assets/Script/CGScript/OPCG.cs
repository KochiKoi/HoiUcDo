using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class OPCG : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneStep
    {
        public TMP_Text textBlock;
        public Image imageBlock;
        public float delayBeforeContinue = 4f;
    }
    [Header("Cutscene Initiate")]
    public bool alreadyPlayed = false; // Safety lock

    [Header("This Cutscene’s Canvas Group")]
    public CanvasGroup cgGroup;

    [Header("Other CanvasGroups to Hide During CG")]
    public CanvasGroup[] otherCanvasGroups;

    [Header("Cutscene Steps")]
    public CutsceneStep[] steps;

    [Header("Press To Continue Prompt")]
    public TextMeshProUGUI continueText;

    [Header("Fade Timings")]
    public float fadeDuration = 1f;

    [Header("Flicker Settings")]
    public float flickerMinAlpha = 0.5f;
    public float flickerMaxAlpha = 1f;
    public float flickerSpeed = 0.5f;

    [Header("Player Control Script")]
    public MonoBehaviour playerController;

    [Header("Optional Scene Transition")]
    public bool switchToNextScene = false;
    public string nextSceneName;
    public bool preloadScene = true;

    [Header("Auto Mode (VN Style)")]
    public bool autoMode = false; // actual behavior flag
    public Button autoModeButton; // assign this via Inspector
    public TextMeshProUGUI autoModeLabel; // optional label update
    public bool autoModeLocked = false;
    [Tooltip("Delay between steps when autoMode is enabled.")]
    public float autoAdvanceDelay = 1f;

    [Header("Cutscene Time Freeze")]
    public bool freezeTimeDuringCutscene = false;
    // Add this variable to store original timescale
    private float originalTimeScale = 1f;

    [Header("Optional Timeline Trigger")]
    public UnityEngine.Playables.PlayableDirector timelineToPlay;


    private AsyncOperation preloadOperation;
    private bool canContinue;
    private Coroutine flickerCoroutine;

    void Awake()
    {
        if (alreadyPlayed)
        {
            cgGroup.gameObject.SetActive(false); // skip and hide CG
            if (playerController != null)
                playerController.enabled = true;

            foreach (var other in otherCanvasGroups)
                if (other != null)
                {
                    other.gameObject.SetActive(true);
                    other.alpha = 1f;
                }

            if (timelineToPlay != null)
                timelineToPlay.Play(); // optionally still play timeline if needed

            return; // 🚫 skip cutscene entirely
        }

        alreadyPlayed = true; // ✅ lock this cutscene for this session

        if (freezeTimeDuringCutscene)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        // Hide other UI
        foreach (var other in otherCanvasGroups)
            if (other != null)
            {
                other.alpha = 0f;
                other.gameObject.SetActive(false);
            }

        // Setup CG canvas
        cgGroup.alpha = 0f;
        cgGroup.gameObject.SetActive(true);

        // Hide all elements initially
        foreach (var step in steps)
        {
            if (step.textBlock != null) step.textBlock.alpha = 0f;
            if (step.imageBlock != null) step.imageBlock.color = new Color(1, 1, 1, 0);
        }

        if (continueText != null)
        {
            continueText.alpha = 0f;
            continueText.gameObject.SetActive(false);
        }

        if (autoModeButton != null)
        {
            if (!autoModeLocked)
            {
                autoModeButton.onClick.AddListener(ToggleAutoMode);
                autoModeButton.interactable = true;
            }
            else
            {
                autoModeButton.interactable = false;
                if (autoModeLabel != null)
                    autoModeLabel.text = "AUTO: ON";
            }
        }

        if (playerController != null)
            playerController.enabled = false;

        // Optional: Preload scene
        if (switchToNextScene && preloadScene && !string.IsNullOrEmpty(nextSceneName))
        {
            preloadOperation = SceneManager.LoadSceneAsync(nextSceneName);
            preloadOperation.allowSceneActivation = false;
        }

        StartCoroutine(PlayOpeningCutscene());
    }

    IEnumerator PlayOpeningCutscene()
    {
        yield return FadeCanvas(cgGroup, 0, 1);

        foreach (var step in steps)
        {
            // Fade in both, but wait for the longest one
            Coroutine fadeText = null;
            Coroutine fadeImage = null;

            if (step.imageBlock != null)
                fadeImage = StartCoroutine(FadeImage(step.imageBlock, 0, 1));
            if (step.textBlock != null)
                fadeText = StartCoroutine(FadeTMP(step.textBlock, 0, 1));

            // Wait for fadeDuration manually if both are being animated
            yield return new WaitForSeconds(fadeDuration);


            // Wait for visual delay or default
            yield return new WaitForSeconds(step.delayBeforeContinue);

            // Show flickering continue prompt
            if (continueText != null)
            {
                continueText.gameObject.SetActive(true);
                yield return FadeTMP(continueText, 0, 1, 0.5f);
            }
            if (continueText != null && !autoMode)
            {
                // Manual mode
                canContinue = true;
                continueText.gameObject.SetActive(true);
                yield return FadeTMP(continueText, 0, 1, 0.5f);
                flickerCoroutine = StartCoroutine(FlickerContinue());
                yield return new WaitUntil(() => !canContinue);
                StopCoroutine(flickerCoroutine);
                yield return FadeTMP(continueText, continueText.alpha, 0, 0.3f);
                continueText.gameObject.SetActive(false);
            }
            else
            {
                // Auto mode (no player input needed)
                yield return WaitForAutoDelay();
            }



            if (step.textBlock != null)
                yield return FadeTMP(step.textBlock, 1, 0);
            if (step.imageBlock != null)
                yield return FadeImage(step.imageBlock, 1, 0);

        }

        yield return FadeCanvas(cgGroup, 1f, 0f);
        cgGroup.gameObject.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        foreach (var other in otherCanvasGroups)
            if (other != null)
            {
                other.gameObject.SetActive(true);
                other.alpha = 1f;
            }

        // Final scene transition (no fade screen)
        if (switchToNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            if (preloadOperation != null)
            {
                preloadOperation.allowSceneActivation = true;
                while (!preloadOperation.isDone)
                    yield return null;
            }
            else
            {
                SceneFader.Instance.FadeToScene(nextSceneName);
            }

            Resources.UnloadUnusedAssets();
        }
        // Restore time after cutscene
        if (freezeTimeDuringCutscene)
            Time.timeScale = originalTimeScale;
        // Trigger Timeline if assigned
        if (timelineToPlay != null)
        {
            timelineToPlay.Play();
        }

    }
    void Update()
    {
        if (continueText != null && canContinue && Input.anyKeyDown)
            canContinue = false;
    }

    void ToggleAutoMode()
    {
        autoMode = !autoMode;

        if (autoModeLabel != null)
            autoModeLabel.text = autoMode ? "AUTO: ON" : "AUTO: OFF";

        Debug.Log("[Cutscene] Auto Mode: " + (autoMode ? "Enabled" : "Disabled"));
    }



    IEnumerator FlickerContinue()
    {
        float alpha = flickerMinAlpha;
        bool up = true;
        while (continueText != null) // 👈 loop stops if it's missing or destroyed
        {
            alpha += (up ? 1 : -1) * Time.deltaTime * flickerSpeed;
            if (alpha >= flickerMaxAlpha) { alpha = flickerMaxAlpha; up = false; }
            if (alpha <= flickerMinAlpha) { alpha = flickerMinAlpha; up = true; }
            continueText.alpha = alpha;
            yield return null;
        }
    }


    IEnumerator FadeCanvas(CanvasGroup g, float from, float to, float dur = -1f)
    {
        if (dur < 0) dur = fadeDuration;
        float t = 0f;
        g.alpha = from;
        while (t < dur)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        g.alpha = to;
    }

    IEnumerator FadeTMP(TMP_Text t, float from, float to, float dur = -1f)
    {
        if (dur < 0) dur = fadeDuration;
        float elapsed = 0f;
        t.color = new Color(t.color.r, t.color.g, t.color.b, from);
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / dur);
            t.color = new Color(t.color.r, t.color.g, t.color.b, a);
            yield return null;
        }
        t.color = new Color(t.color.r, t.color.g, t.color.b, to);
    }

    IEnumerator FadeImage(Image img, float from, float to, float dur = -1f)
    {
        if (dur < 0) dur = fadeDuration;
        float elapsed = 0f;
        Color c = img.color;
        c.a = from;
        img.color = c;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / dur);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }
    IEnumerator WaitForAutoDelay()
    {
        float elapsed = 0f;
        while (elapsed < autoAdvanceDelay)
        {
            elapsed += (freezeTimeDuringCutscene ? Time.unscaledDeltaTime : Time.deltaTime);
            yield return null;
        }
    }

}
