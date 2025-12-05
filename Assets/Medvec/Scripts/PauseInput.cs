using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    [SerializeField] private PausePanel pausePanel; // referencuj priamo skript

    private InputAction pauseAction;

    private void Awake()
    {
        // Ak si zabudneš nastaviť v Inspectorovi, nájde prvý PausePanel v scéne (aj skrytý)
        if (pausePanel == null)
            pausePanel = FindObjectOfType<PausePanel>(true);

        // vytvorenie akcie na kláves P
        pauseAction = new InputAction(
            type: InputActionType.Button,
            binding: "<Keyboard>/p"
        );
    }

    private void Start()
    {
        // na začiatku hra beží a panel je skrytý
        Time.timeScale = 1f;

        if (pausePanel != null && pausePanel.gameObject.activeSelf)
            pausePanel.ResumeGame();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPause;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPause;
        pauseAction.Disable();
    }

    private void OnDestroy()
    {
        pauseAction.Dispose();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (pausePanel == null)
            return;

        if (pausePanel.IsPaused)
            pausePanel.ResumeGame();
        else
            pausePanel.PauseGame();
    }
}
