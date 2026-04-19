using UnityEngine;
using UnityEngine.Localization.Components;
using System.Collections;

public class RangefinderController : MonoBehaviour
{
    [Header("Range & Rotation")]
    public GameObject rangefinder;
    public float Angle = 0f;
    public float keyboardSpeed = 100f;
    public float scrollSpeed = 100f;
    public float minAngle = 0f;
    public float maxAngle = 1000f;
    public float maxRotationDegrees = 90f;

    [Header("Enemy Distance")]
    public float distance = 0f;
    public float minDistance = 0f;
    public float maxDistance = 1000f;
    public float acceptableError = 50f;
    public float step = 100f;

    [Header("Wind Settings")]
    public float minWind = -30f;
    public float maxWind = 30f;
    private float windBias;

    [Header("Enemy Settings")]
    public int[] enemyArmors;
    private int currentEnemyIndex = 0;
    private int currentArmor = 0;
    private float lastShotDistance = 1000f;

    [Header("Voice Line Settings")]
    public LocalizeStringEvent npcVoiceLocalizedEvent;
    public CanvasGroup npcVoiceGroup;
    public float dialogueDisplayDuration = 3f;
    public float distanceFeedbackDelay = 1f;

    [Header("Cannon Cooldown")]
    public float reloadTime = 2f;
    private bool isReloading = false;

    [Header("Reload UI")]
    public UnityEngine.UI.Image reloadIndicator;

    [Header("Cutscene")]
    public EDCG edcg;

    private void Start()
    {
        SetInitialEnemyDistance();
        FadeOutVoiceImmediate();
    }

    private void Update()
    {
        HandleInput();
        UpdateRangefinderRotation();

        if (Input.GetMouseButtonDown(0) && !isReloading)
        {
            StartCoroutine(HandleShot());
        }
    }

    private IEnumerator HandleShot()
    {
        isReloading = true;

        StartCoroutine(AnimateReloadBar());

        float actualShot = Angle + windBias;
        float diff = Mathf.Abs(actualShot - distance);

        Debug.Log($"[SHOT] Angle={Angle:F1} + Wind={windBias:F1} → Total={actualShot:F1} | Target={distance:F1} | Diff={diff:F1}");

        // Step 1: Show error voice immediately
        DisplayLocalizedLine(GetErrorVoiceKey(diff));

        // Step 2: Wait, then show distance feedback
        yield return new WaitForSeconds(distanceFeedbackDelay);
        DisplayLocalizedLine(GetDistanceVoiceKey(distance));

        // Step 3: Keep dialogue visible for UX
        yield return new WaitForSeconds(dialogueDisplayDuration);
        FadeOutVoice();

        // Step 4: Evaluate result
        ProcessShotOutcome(diff);

        // Final wait: remainder of reload time if needed
        float remainingReloadTime = Mathf.Max(0f, reloadTime - (distanceFeedbackDelay + dialogueDisplayDuration));
        yield return new WaitForSeconds(remainingReloadTime);
        isReloading = false;
    }

    private IEnumerator AnimateReloadBar()
    {
        if (reloadIndicator == null)
            yield break;

        reloadIndicator.fillAmount = 0f;
        float t = 0f;

        while (t < reloadTime)
        {
            t += Time.deltaTime;
            reloadIndicator.fillAmount = t / reloadTime;
            yield return null;
        }

        reloadIndicator.fillAmount = 1f;
    }

    private void ProcessShotOutcome(float diff)
    {
        if (diff <= acceptableError)
        {
            currentArmor--;

            if (currentArmor <= 0)
            {
                Debug.Log("🎯 Enemy Destroyed!");
                DisplayLocalizedLine("npchit");
                lastShotDistance = distance;
                currentEnemyIndex++;
                SetInitialEnemyDistance();
            }
            else
            {
                Debug.Log($"🛡️ Hit armor! Remaining: {currentArmor}");
                DisplayLocalizedLine("npcarmorhit");
                distance -= step;
            }
        }
        else
        {
            Debug.Log("💥 Missed target!");
            distance -= step;
        }

        if (distance <= 0f)
        {
            Debug.Log("💀 Game Over!");
            // Optional: Game over cutscene
        }
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.W)) Angle += keyboardSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) Angle -= keyboardSpeed * Time.deltaTime;
        Angle += Input.mouseScrollDelta.y * scrollSpeed * Time.deltaTime;
        Angle = Mathf.Clamp(Angle, minAngle, maxAngle);
    }

    private void UpdateRangefinderRotation()
    {
        float rotationZ = Mathf.Lerp(0f, maxRotationDegrees, Angle / maxAngle);
        if (rangefinder != null)
            rangefinder.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void SetInitialEnemyDistance()
    {
        if (currentEnemyIndex >= enemyArmors.Length)
        {
            Debug.Log("✅ All enemies down.");
            edcg?.TriggerCutscene();
            return;
        }

        distance = (currentEnemyIndex == 0)
            ? GetRandomStepValue(600f, maxDistance, step)
            : Mathf.Clamp(lastShotDistance - 100f, minDistance, maxDistance);

        windBias = Random.Range(minWind, maxWind);
        currentArmor = enemyArmors[currentEnemyIndex];

        Debug.Log($"🚨 Enemy #{currentEnemyIndex + 1} spawned | Distance: {distance:F0} | Armor: {currentArmor} | Wind: {windBias:F0}");
    }

    private float GetRandomStepValue(float min, float max, float stepSize)
    {
        int steps = Mathf.FloorToInt((max - min) / stepSize);
        return min + stepSize * Random.Range(0, steps + 1);
    }

    private string GetDistanceVoiceKey(float d)
    {
        if (d > 900f) return "npcdis900to1000";
        if (d > 600f) return "npcdis600to900";
        if (d > 400f) return "npcdis400to600";
        return "npcdis400";
    }

    private string GetErrorVoiceKey(float diff)
    {
        if (diff >= 300f) return "npcmissfar";
        if (diff >= 100f) return "npcmissclose";
        if (diff > 50f) return "npcalmosthit";
        return "";
    }

    private void DisplayLocalizedLine(string key)
    {
        if (npcVoiceLocalizedEvent != null && !string.IsNullOrEmpty(key))
        {
            npcVoiceLocalizedEvent.StringReference.TableReference = "Language_UIUX";
            npcVoiceLocalizedEvent.StringReference.TableEntryReference = key;
            npcVoiceLocalizedEvent.RefreshString();

            if (npcVoiceGroup != null)
                StartCoroutine(FadeInVoice());
        }
    }

    private IEnumerator FadeInVoice()
    {
        npcVoiceGroup.alpha = 0f;
        npcVoiceGroup.gameObject.SetActive(true);
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            npcVoiceGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            yield return null;
        }
    }

    private void FadeOutVoice()
    {
        if (npcVoiceGroup != null)
            StartCoroutine(FadeOutRoutine());
    }

    private void FadeOutVoiceImmediate()
    {
        if (npcVoiceGroup != null)
        {
            npcVoiceGroup.alpha = 0f;
            npcVoiceGroup.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            npcVoiceGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }
        npcVoiceGroup.gameObject.SetActive(false);
    }
}
