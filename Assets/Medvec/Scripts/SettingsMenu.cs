using UnityEngine;
using UnityEngine.UI;
using TMPro;   // ➜ PRIDANÉ

public class SettingsMenu : MonoBehaviour
{
    [Header("UI")]
    public Slider masterVolumeSlider;
    public Slider mouseSensitivitySlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;   // ➜ ZMENENÉ

    public GameObject panelRoot;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MOUSE_SENS_KEY = "MouseSensitivity";
    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string QUALITY_KEY = "QualityLevel";

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;
    }

    private void Start()
    {
        // 1️⃣ Načítanie hlasitosti
        float vol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        masterVolumeSlider.value = vol;
        ApplyMasterVolume(vol);

        // 2️⃣ Načítanie citlivosti myši
        float sens = PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 1f);
        mouseSensitivitySlider.value = sens;

        // 3️⃣ Načítanie fullscreen
        bool fs = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        fullscreenToggle.isOn = fs;
        ApplyFullscreen(fs);

        // 4️⃣ Načítanie kvality grafiky
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        qualityDropdown.value = quality;
        ApplyQuality(quality);
    }

    // ---------- HANDLERY Z UI ----------
    public void OnMasterVolumeChanged(float value)
    {
        ApplyMasterVolume(value);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
    }

    public void OnMouseSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(MOUSE_SENS_KEY, value);
    }

    public void OnFullscreenToggled(bool isOn)
    {
        ApplyFullscreen(isOn);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isOn ? 1 : 0);
    }

    public void OnQualityChanged(int index)
    {
        ApplyQuality(index);
        PlayerPrefs.SetInt(QUALITY_KEY, index);
    }

    public void OnBackButton()
    {
        PlayerPrefs.Save();
        panelRoot.SetActive(false);
    }

    // ---------- FUNKCIE APLIKUJÚCE NASTAVENIE ----------
    private void ApplyMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ApplyFullscreen(bool isOn)
    {
        Screen.fullScreen = isOn;
    }

    private void ApplyQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
