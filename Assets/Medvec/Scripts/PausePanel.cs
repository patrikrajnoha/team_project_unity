using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Sem potiahni ONLY objekt, ktorý je samotné pause menu (napr. Panel PauseMenu).")]
    [SerializeField] private GameObject pauseWindow;

    [Tooltip("Sem môžeš (nemusíš) potiahnuť HUD, ak by si ho chcel pri pauze skrývať/zobrazovať.")]
    [SerializeField] private GameObject hud;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Start()
    {
        // na začiatku je hra bežiaca, pause okno skryté, HUD zapnutý
        if (pauseWindow != null)
            pauseWindow.SetActive(false);

        if (hud != null)
            hud.SetActive(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------- PAUSE / RESUME ----------

    public void PauseGame()
    {
        isPaused = true;

        if (pauseWindow != null)
            pauseWindow.SetActive(true);

        // AK nechceš, aby HUD mizol, na hud NESIAHAJ:
        // if (hud != null) hud.SetActive(false);  // použij len ak chceš HUD schovať

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pauseWindow != null)
            pauseWindow.SetActive(false);

        if (hud != null)
            hud.SetActive(true);  // HUD nech je určite zapnutý

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
        // Tu si neskôr otvor svoj settings panel
        Debug.Log("OpenSettings clicked – tu otvor svoj settings panel.");
    }

    // ---------- BUTTON: BACK TO MAIN MENU ----------

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
