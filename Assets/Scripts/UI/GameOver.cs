using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOver : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel, ktorý sa zobrazí po smrti hráča.")]
    public GameObject gameOverPanel;

    [Tooltip("Voliteľný text GAME OVER (môže zostať prázdny).")]
    public TextMeshProUGUI gameOverText;

    [Header("Menu")]
    [Tooltip("Názov scény s hlavným menu (napr. MainMenu).")]
    public string menuSceneName = "MainMenu";

    private bool isGameOver = false;

    private void Awake()
    {
        if (gameOverPanel == null)
            Debug.LogError("GameOver.Awake: gameOverPanel NIE JE nastavený!");
        else
            Debug.Log($"GameOver.Awake: gameOverPanel = {gameOverPanel.name}, active = {gameOverPanel.activeSelf}");
    }

    private void Start()
    {
        // Panel nechceme vidieť pri štarte – len istota
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("GameOver.Start: vypínam GameOverPanel na začiatku hry.");
        }

        // Hra beží normálne
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("GameOver.Start: nastavujem timeScale=1, kurzor zamknutý.");
    }

    /// <summary>
    /// Zavolaj, keď hráč zomrie.
    /// </summary>
    public void ShowGameOver()
    {
        Debug.Log("GameOver.ShowGameOver: volané.");

        if (isGameOver)
        {
            Debug.Log("GameOver.ShowGameOver: už je game over, ignorujem ďalšie volanie.");
            return;
        }

        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOver.ShowGameOver: zapínam GameOverPanel.");
        }
        else
        {
            Debug.LogWarning("GameOver.ShowGameOver: gameOverPanel je NULL!");
        }

        // pauza hry
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("GameOver.ShowGameOver: timeScale=0, kurzor odomknutý.");
    }

    /// <summary>
    /// OnClick pre tlačidlo RESTART.
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("GameOver.RestartLevel: reštart levelu.");

        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    /// <summary>
    /// OnClick pre tlačidlo GO TO MENU.
    /// </summary>
    public void GoToMenu()
    {
        Debug.Log("GameOver.GoToMenu: prechod do menu.");

        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("GameOver.GoToMenu: menuSceneName nie je nastavený!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
