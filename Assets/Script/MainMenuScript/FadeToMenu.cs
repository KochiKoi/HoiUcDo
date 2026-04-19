using UnityEngine;

public class FadeToMenu : MonoBehaviour
{
    public CanvasGroup fromMenu;
    public CanvasGroup toMenu;

    public void OnFadeRequest()
    {
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.StartCoroutine(FadeManager.Instance.Switch(fromMenu, toMenu));
        }
    }
}
