using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    public void LoadSceneAdditive()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Button Pressed");
            //SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            if (TimeEffectManager.Instance != null)
            {
                TimeEffectManager.Instance.PauseTime();
            }
            else
            {
                Debug.Log("Keep moving on then");
            }
            SceneFader.Instance.FadeToSceneAdditive(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("SceneLoaderButton: No scene name assigned!");
        }
    }
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Button Pressed");
            //SceneManager.LoadSceneAsync(sceneToLoad);
            SceneFader.Instance.FadeToScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("SceneLoaderButton: No scene name assigned!");
        }
    }
    public void CloseScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Button Pressed");
            //SceneManager.UnloadSceneAsync(sceneToLoad);
            SceneFader.Instance.FadeAndUnloadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("SceneLoaderButton: No scene name assigned!");
        }
    }

}
