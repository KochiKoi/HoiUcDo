using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class OptionsMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Toggle vsyncToggle;

    [Header("Dropdown - Language")]
    public TMP_Dropdown languageDropdown;

    [Header("Dropdown - Resolution")]
    public TMP_Dropdown resolutionDropdown;

    [Header("Dropdown - Fullscreen Mode")]
    public TMP_Dropdown fullscreenDropdown;

    private Resolution[] availableResolutions;
    private readonly string[] fullscreenModes = { "Fullscreen", "Borderless Window", "Windowed" };

    void Start()
    {
        InitVSync();
        StartCoroutine(InitLanguage());
        InitResolution();
        InitFullscreenMode();
    }

    #region V-Sync
    private void InitVSync()
    {
        int current = QualitySettings.vSyncCount;
        vsyncToggle.isOn = current > 0;

        vsyncToggle.onValueChanged.AddListener((isOn) =>
        {
            QualitySettings.vSyncCount = isOn ? 1 : 0;
            Debug.Log("V-Sync: " + (isOn ? "Enabled" : "Disabled"));
        });
    }
    #endregion

    #region Language (Dropdown)
    private IEnumerator InitLanguage()
    {
        yield return LocalizationSettings.InitializationOperation;

        languageDropdown.options.Clear();
        var locales = LocalizationSettings.AvailableLocales.Locales;
        int currentLocaleIndex = 0;

        for (int i = 0; i < locales.Count; i++)
        {
            var locale = locales[i];
            string label = locale.Identifier.CultureInfo.NativeName.Split('(')[0].Trim();
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(label));

            if (LocalizationSettings.SelectedLocale == locale)
                currentLocaleIndex = i;
        }

        languageDropdown.value = currentLocaleIndex;
        languageDropdown.RefreshShownValue();

        languageDropdown.onValueChanged.AddListener((index) =>
        {
            var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            LocalizationSettings.SelectedLocale = selectedLocale;
            Debug.Log("Language changed to: " + selectedLocale.Identifier.Code);
        });
    }
    #endregion

    #region Resolution (Dropdown)
    private void InitResolution()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.options.Clear();

        int currentResolutionIndex = 0;
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string label = availableResolutions[i].width + " x " + availableResolutions[i].height;
            if (!seen.Contains(label))
            {
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(label));
                seen.Add(label);

                if (Screen.currentResolution.width == availableResolutions[i].width &&
                    Screen.currentResolution.height == availableResolutions[i].height)
                {
                    currentResolutionIndex = resolutionDropdown.options.Count - 1;
                }
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void OnResolutionChanged(int index)
    {
        string[] dims = resolutionDropdown.options[index].text.Split('x');
        int width = int.Parse(dims[0].Trim());
        int height = int.Parse(dims[1].Trim());
        Screen.SetResolution(width, height, Screen.fullScreenMode);
        Debug.Log("Resolution set to: " + width + "x" + height);
    }
    #endregion

    #region Fullscreen Mode (Dropdown)
    private void InitFullscreenMode()
    {
        fullscreenDropdown.options.Clear();

        int currentIndex = 0;
        for (int i = 0; i < fullscreenModes.Length; i++)
        {
            fullscreenDropdown.options.Add(new TMP_Dropdown.OptionData(fullscreenModes[i]));

            if ((int)Screen.fullScreenMode == i)
            {
                currentIndex = i;
            }
        }

        fullscreenDropdown.value = currentIndex;
        fullscreenDropdown.RefreshShownValue();
        fullscreenDropdown.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void OnFullscreenChanged(int index)
    {
        FullScreenMode mode = (FullScreenMode)index;
        Screen.fullScreenMode = mode;

        // Always fullscreen unless windowed
        Screen.fullScreen = mode != FullScreenMode.Windowed;

        if (mode == FullScreenMode.FullScreenWindow)
        {
            // Borderless = use display's current resolution
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, mode);
        }
        else
        {
            // Use the resolution from dropdown
            OnResolutionChanged(resolutionDropdown.value);
        }

        Debug.Log("Fullscreen Mode set to: " + mode);
    }

    #endregion
}