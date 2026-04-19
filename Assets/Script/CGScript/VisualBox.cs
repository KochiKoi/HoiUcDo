using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class NewGameStarters : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneStep
    {
        public TMP_Text textBlock;
        public float delayBeforeContinue = 4f;
    }

    [Header("Scene Management")]
    public string nextSceneName;

    [Header("Canvas Groups")]
    public CanvasGroup mainMenuGroup;
    public CanvasGroup disclaimerGroup;

    [Header("Cutscene Steps")]
    public CutsceneStep[] cutsceneSteps;

    [Header("Continue Prompt")]
    public TextMeshProUGUI continueText;

    [Header("Fade Timings")]
    public float fadeDuration = 1f;

    [Header("Flicker Settings")]
    public float flickerMinAlpha = 0.5f;
    public float flickerMaxAlpha = 1f;
    public float flickerSpeed = 0.5f;

    private Coroutine flickerCoroutine;
    private bool canContinue = false;
    private int currentStep = 0;

    void Start()
    {
        disclaimerGroup.alpha = 0f;
        disclaimerGroup.gameObject.SetActive(false);

        foreach (var step in cutsceneSteps)
        {
            if (step.textBlock != null)
            {
                step.textBlock.alpha = 0f;
            }
        }

        continueText.alpha = 0f;
        continueText.gameObject.SetActive(false);
    }

    public void OnStartNewGame()
    {
        StartCoroutine(PlayCutsceneSequence());
    }

    IEnumerator PlayCutsceneSequence()
    {
        if (mainMenuGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(mainMenuGroup, 1, 0, fadeDuration));

        disclaimerGroup.gameObject.SetActive(true);

        for (int i = 0; i < cutsceneSteps.Length; i++)
        {
            currentStep = i;

            var step = cutsceneSteps[i];

            // Fade in text block
            yield return StartCoroutine(FadeTextAlpha(step.textBlock, 0, 1, fadeDuration));

            // Wait before allowing continue
            yield return new WaitForSeconds(step.delayBeforeContinue);

            // Enable press-to-continue
            canContinue = true;
            continueText.gameObject.SetActive(true);
            flickerCoroutine = StartCoroutine(SmoothFlickerText());

            // Wait for input
            yield return new WaitUntil(() => canContinue == false);

            // Disable prompt
            if (flickerCoroutine != null)
                StopCoroutine(flickerCoroutine);
            continueText.gameObject.SetActive(false);

            // Fade out current text block
            yield return StartCoroutine(FadeTextAlpha(step.textBlock, 1, 0, fadeDuration));
        }

        // Final fade-out and load scene
        yield return StartCoroutine(FadeCanvasGroup(disclaimerGroup, 1, 0, fadeDuration));
        yield return new WaitForSeconds(0.2f);
        //SceneManager.LoadScene(nextSceneName);
        SceneFader.Instance.FadeToScene(nextSceneName);
    }

    void Update()
    {
        if (canContinue && Input.anyKeyDown)
        {
            canContinue = false;
        }
    }

    IEnumerator SmoothFlickerText()
    {
        float alpha = flickerMinAlpha;
        bool increasing = true;

        while (true)
        {
            if (increasing)
                alpha += Time.deltaTime * flickerSpeed;
            else
                alpha -= Time.deltaTime * flickerSpeed;

            if (alpha >= flickerMaxAlpha)
            {
                alpha = flickerMaxAlpha;
                increasing = false;
            }
            else if (alpha <= flickerMinAlpha)
            {
                alpha = flickerMinAlpha;
                increasing = true;
            }

            continueText.alpha = alpha;
            yield return null;
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }

    IEnumerator FadeTextAlpha(TMP_Text text, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, to);
    }
}
