using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanel : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void OnEnable()
    {
        // keď sa PausePanel zapne, pauzni hru a zobraz kurzor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ---------- BUTTON: RETURN ----------
    public void ReturnToGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------- BUTTON: SETTINGS ----------
    public void OpenSettings()
    {
        // Sem si potom doplníš zobrazenie svojho settings panelu
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
