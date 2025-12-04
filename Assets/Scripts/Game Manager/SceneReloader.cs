using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneReloader : MonoBehaviour
{
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.ResetScene.performed += OnResetScene;
    }

    private void OnDisable()
    {
        inputActions.Player.ResetScene.performed -= OnResetScene;
        inputActions.Disable();
    }

    private void OnResetScene(InputAction.CallbackContext ctx)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
