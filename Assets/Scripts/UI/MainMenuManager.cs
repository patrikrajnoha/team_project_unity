using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;   // nový
    public GameObject settingsPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("Level01"); 
    }

    public void OpenLevelMenu()
    {
        SceneManager.LoadScene("LevelMenu");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);    // skry hlavné menu
        settingsPanel.SetActive(true);     // ukáž settings
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);    // skry settings
        mainMenuPanel.SetActive(true);     // ukáž hlavné menu
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

