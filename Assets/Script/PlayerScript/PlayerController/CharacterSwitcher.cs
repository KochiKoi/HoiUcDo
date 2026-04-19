using UnityEngine;
using Cinemachine;
using System.Collections;

public class CinemachineSwitchTrigger : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineVirtualCamera currentCam;
    public CinemachineVirtualCamera nextCam;

    [Header("Optional One-Time Use")]
    public bool disableAfterSwitch = true;

    [Header("Cinematic Slowdown")]
    public bool enableSlowdown = true;
    public float slowdownScale = 0.3f;
    public float slowdownDuration = 1f;

    [Header("Character Deactivate Time")]
    public float deactivetime = 1f;

    [Header("Optional Cleanup")]
    public GameObject[] objectsToDisableAfterSwitch;

    public MonoBehaviour coroutineHost; // assign this to a persistent object (like MainCamera)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (currentCam != null) currentCam.Priority = 10;
        if (nextCam != null) nextCam.Priority = 20;

        EnableControl(nextCam?.Follow);
        DisableControl(currentCam?.Follow);

        if (enableSlowdown && TimeEffectManager.Instance != null)
            TimeEffectManager.Instance.TriggerQuickSlowdown(slowdownScale, slowdownDuration, 1f);

        if (coroutineHost != null)
            coroutineHost.StartCoroutine(DisableObjectsAfterDelay(deactivetime));

        if (disableAfterSwitch)
            gameObject.SetActive(false);
    }

    void EnableControl(Transform target)
    {
        if (target == null) return;

        var controller = target.GetComponent<IControllable>();
        if (controller != null)
            controller.EnableControl();
    }

    void DisableControl(Transform target)
    {
        if (target == null) return;

        var controller = target.GetComponent<IControllable>();
        if (controller != null)
            controller.DisableControl();

        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;

        if (coroutineHost != null)
            coroutineHost.StartCoroutine(DelayedDeactivate(target.gameObject, deactivetime));
        else
            Debug.LogWarning("Coroutine host not assigned! GameObject will not deactivate properly.");
    }

    IEnumerator DelayedDeactivate(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        obj.SetActive(false);
    }

    IEnumerator DisableObjectsAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        foreach (GameObject go in objectsToDisableAfterSwitch)
        {
            if (go != null)
            {
                Destroy(go); // Completely removes the object from memory
            }
        }

        // Optional: Clear up unused memory
        Resources.UnloadUnusedAssets();
    }
}
