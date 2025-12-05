using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PausePanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel hlavného pause menu.")]
    [SerializeField] private GameObject pauseWindow;

    [Tooltip("HUD ktorý počas pause môže (ale nemusí) ostať zapnutý.")]
    [SerializeField] private GameObject hud;

    [Header("Settings Panel")]
    [Tooltip("Tvoj settings panel, ktorý sa aktivuje po kliknutí na Settings.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Post-process blur (optional)")]
    [SerializeField] private Volume blurVolume;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    public static bool IsGamePaused { get; private set; } = false;

    private void Start()
    {
        if (pauseWindow != null)
            pauseWindow.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (hud != null)
            hud.SetActive(true);

        if (blurVolume != null)
            blurVolume.weight = 0f;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        IsGamePaused = false;
    }

    // ---------- PAUSE / RESUME ----------

    public void PauseGame()
    {
        isPaused = true;
        IsGamePaused = true;

        if (pauseWindow != null)
            pauseWindow.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 0f;

        if (blurVolume != null)
            blurVolume.weight = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        IsGamePaused = false;

        Time.timeScale = 1f;

        if (pauseWindow != null)
            pauseWindow.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (hud != null)
            hud.SetActive(true);

        if (blurVolume != null)
            blurVolume.weight = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------- BUTTON: RETURN ----------

    public void ReturnToGame()
    {
        ResumeGame();
    }

    // ---------- BUTTON: SETTINGS ----------

    public void OpenSettings()
    {
        if (pauseWindow != null)
            pauseWindow.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Debug.Log("Settings panel opened.");
    }

    // Ak v nastaveniach máš tlačidlo „Back“
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseWindow != null)
            pauseWindow.SetActive(true);

        Debug.Log("Returned to pause menu.");
    }

    // ---------- BUTTON: BACK TO MAIN MENU ----------

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        IsGamePaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ---------- BUTTON: QUIT GAME ----------

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
