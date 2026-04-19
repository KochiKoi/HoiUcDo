using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;

    [Header("Startup Menus")]
    public CanvasGroup[] allMenus;
    public CanvasGroup pressToStartGroup;
    public CanvasGroup mainMenuGroup;

    [Header("Screen Fade (Scene Transitions)")]
    public CanvasGroup screenFadeGroup;

    private bool hasStarted = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (screenFadeGroup != null)
        {
            screenFadeGroup.alpha = 1f;
            StartCoroutine(FadeOut(screenFadeGroup));
        }
        // Hide all menus first
        foreach (var menu in allMenus)
        {
            if (menu != null)
            {
                menu.alpha = 0f;
                menu.interactable = false;
                menu.blocksRaycasts = false;
                menu.gameObject.SetActive(false);
            }
        }

        // Show Press to Start screen
        if (pressToStartGroup != null)
        {
            pressToStartGroup.alpha = 1f;
            pressToStartGroup.interactable = true;
            pressToStartGroup.blocksRaycasts = true;
            pressToStartGroup.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!hasStarted && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            hasStarted = true;
            StartCoroutine(Switch(pressToStartGroup, mainMenuGroup));
        }
    }

    public IEnumerator FadeIn(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.interactable = false;
        group.blocksRaycasts = false;

        while (!Mathf.Approximately(group.alpha, 1f))
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 1f, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public IEnumerator FadeOut(CanvasGroup group)
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        while (!Mathf.Approximately(group.alpha, 0f))
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        group.gameObject.SetActive(false);
    }

    public IEnumerator Switch(CanvasGroup from, CanvasGroup to)
    {
        Debug.Log("Switching from: " + from?.gameObject.name + " to: " + to?.gameObject.name);

        if (from != null)
            yield return StartCoroutine(FadeOut(from));

        if (to != null)
            yield return StartCoroutine(FadeIn(to));
    }
}
