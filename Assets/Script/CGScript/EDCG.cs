using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EDCG : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneStep
    {
        public TMP_Text textBlock;
        public Image imageBlock;
        public float delayBeforeContinue = 4f;
    }

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

    [Header("Trigger Settings")]
    public string playerTag = "Player";
    private bool hasTriggered = false;
    [Tooltip("If true, this cutscene will only play once per game session.")]
    public bool playOnce = false;

    private static HashSet<string> playedCutscenes = new HashSet<string>(); // global session tracker
    [Tooltip("Unique ID for this cutscene if playOnce is enabled.")]
    public string cutsceneID;


    [Header("Auto Mode (VN Style)")]
    public bool autoMode = false; // actual behavior flag
    public Button autoModeButton; // assign this via Inspector
    public TextMeshProUGUI autoModeLabel; // optional label update
    public bool autoModeLocked = false;

    [Header("Auto Mode Delay")]
    public float autoAdvanceDelay = 1f;

    [Header("Scene Transition Settings")]
    public bool switchToNextScene = false;
    public string nextSceneName;
    public bool preloadScene = true;

    private AsyncOperation preloadOperation;
    private bool canContinue;
    private Coroutine flickerCoroutine;

    void Start()
    {
        // Hide other canvases
        foreach (var other in otherCanvasGroups)
            if (other != null)
            {
                other.alpha = 0f;
                other.gameObject.SetActive(false);
            }

        // Prepare CG elements
        cgGroup.alpha = 0f;
        cgGroup.gameObject.SetActive(false);

        foreach (var s in steps)
        {
            if (s.textBlock != null) s.textBlock.alpha = 0f;
            if (s.imageBlock != null) s.imageBlock.color = new Color(1, 1, 1, 0);
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


        // Optionally preload scene
        if (switchToNextScene && preloadScene && !string.IsNullOrEmpty(nextSceneName))
        {
            preloadOperation = SceneManager.LoadSceneAsync(nextSceneName);
            preloadOperation.allowSceneActivation = false;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            // Check if it already played (optional safety)
            if (playOnce && !string.IsNullOrEmpty(cutsceneID))
            {
                if (playedCutscenes.Contains(cutsceneID))
                    return; // 🚫 Already played, skip it
            }

            hasTriggered = true;

            if (playOnce && !string.IsNullOrEmpty(cutsceneID))
                playedCutscenes.Add(cutsceneID);

            StartCoroutine(PlayOutroCutscene());
        }
    }

    IEnumerator PlayOutroCutscene()
    {
        cgGroup.gameObject.SetActive(true);
        yield return FadeCanvas(cgGroup, 0, 1);

        foreach (var step in steps)
        {
            if (step.imageBlock != null)
                StartCoroutine(FadeImage(step.imageBlock, 0, 1));
            if (step.textBlock != null)
                StartCoroutine(FadeTMP(step.textBlock, 0, 1));
            yield return new WaitForSeconds(fadeDuration);

            yield return new WaitForSeconds(step.delayBeforeContinue);

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
                // Auto mode (either no text OR auto mode is active)
                yield return new WaitForSeconds(autoAdvanceDelay);
            }



            if (step.textBlock != null)
                yield return FadeTMP(step.textBlock, 1, 0);
            if (step.imageBlock != null)
                yield return FadeImage(step.imageBlock, 1, 0);
        }

        yield return FadeCanvas(cgGroup, 1f, 0f);
        cgGroup.gameObject.SetActive(false);

        foreach (var other in otherCanvasGroups)
            if (other != null)
            {
                other.gameObject.SetActive(true);
                other.alpha = 1f;
            }
        if (!playOnce)
            hasTriggered = false; // Reset for next time

        // ✅ Safe scene transition logic (same as OPCG)
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
    public void TriggerCutscene()
    {
        if (hasTriggered) return;

        if (playOnce && !string.IsNullOrEmpty(cutsceneID))
        {
            if (playedCutscenes.Contains(cutsceneID))
                return;
            playedCutscenes.Add(cutsceneID);
        }

        hasTriggered = true;
        StartCoroutine(PlayOutroCutscene());
    }

    IEnumerator FlickerContinue()
    {
        float alpha = flickerMinAlpha;
        bool up = true;
        while (continueText != null)
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
        Color c = img.color; c.a = from; img.color = c;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / dur);
            img.color = c;
            yield return null;
        }
        c.a = to; img.color = c;
    }
}
