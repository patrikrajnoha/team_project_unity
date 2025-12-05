using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private PausePanel pausePanelScript;
    private InputAction pauseAction;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanelScript = pausePanel.GetComponent<PausePanel>();

        // vytvorenie akcie len raz
        pauseAction = new InputAction(type: InputActionType.Button,
                                      binding: "<Keyboard>/p");
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
        // korektné uvoľnenie akcie
        pauseAction.Dispose();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (pausePanel == null)
            return;

        if (pausePanel.activeSelf)
        {
            if (pausePanelScript != null)
                pausePanelScript.ReturnToGame();
            else
                pausePanel.SetActive(false);
        }
        else
        {
            pausePanel.SetActive(true);
        }
    }
}
