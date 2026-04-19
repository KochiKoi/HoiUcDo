using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartTrigger : MonoBehaviour
{
    [Header("Trigger State")]
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        hasTriggered = true;

        // Optional: Only restart if player or explosion
        if (other.CompareTag("Player"))
        {
            Debug.Log("[RestartTrigger] Game Over triggered by: " + other.name);
            if (SceneRestartHandler.Instance != null)
            {
                SceneRestartHandler.Instance.TriggerSceneRestart();
            }
            else
            {
                Debug.LogWarning("[RestartTrigger] SceneRestartHandler not found in scene.");
            }
        }
    }
}
