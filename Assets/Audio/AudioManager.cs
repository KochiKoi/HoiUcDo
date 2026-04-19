using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer & Sliders")]
    public AudioMixer mainMixer;
    public Slider masterSlider, musicSlider, sfxSlider, ambienceSlider;
    public Toggle muteToggle;

    [Header("Default Volumes")]
    private float defaultVolume = 1f;
    private float minVolume = -80f;

    void Start()
    {
        // Set initial volume from PlayerPrefs or default to 100%
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", defaultVolume);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
        ambienceSlider.value = PlayerPrefs.GetFloat("AmbienceVolume", defaultVolume);

        // Add listeners to sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        ambienceSlider.onValueChanged.AddListener(SetAmbienceVolume);

        // Mute Toggle Listener
        muteToggle.onValueChanged.AddListener(ToggleMute);

        // Set initial volumes
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetAmbienceVolume(ambienceSlider.value);
    }

    public void SetMasterVolume(float volume)
    {
        if (volume <= 0.0001f || muteToggle.isOn)
        {
            mainMixer.SetFloat("MasterVolume", minVolume); // Full Mute
        }
        else
        {
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("MusicVolume", minVolume); // Full Mute
        }
        else
        {
            mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("SFXVolume", minVolume); // Full Mute
        }
        else
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetAmbienceVolume(float volume)
    {
        if (volume <= 0.0001f)
        {
            mainMixer.SetFloat("AmbienceVolume", minVolume); // Full Mute
        }
        else
        {
            mainMixer.SetFloat("AmbienceVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("AmbienceVolume", volume);
    }

    private void ToggleMute(bool isMuted)
    {
        if (isMuted)
        {
            mainMixer.SetFloat("MasterVolume", minVolume); // Mute all
        }
        else
        {
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterSlider.value) * 20); // Restore volume
        }
    }
}
