using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;               // kvôli Volume
using UnityEngine.Rendering.Universal;    // ak používaš URP efekty

public class CamScript : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    public Transform upperBody;

    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    [Header("Look settings")]
    public float mouseSensitivityX = 0.10f;
    public float mouseSensitivityY = 0.08f;

    [Tooltip("Max pitch pre FPS (veľmi malý)")]
    public float pitchLimitFPS = 4f;

    [Tooltip("Max pitch pre TPS (jemný, ale väčší)")]
    public float pitchLimitTPS = 18f;

    [Header("Zoom settings")]
    [Tooltip("FOV pri mierení (Aim). Menšie číslo = väčší zoom.")]
    public float zoomedFOV = 50f;

    [Tooltip("Rýchlosť prechodu medzi normálnou FOV a zoomom.")]
    public float zoomSpeed = 10f;

    [Header("Post-process blur pri pauze")]
    [Tooltip("Global Volume s blur/DepthOfField efektom. Zapne sa len pri pauze.")]
    public Volume blurVolume;

    [Tooltip("Hodnota blur intensity pri pauze (0–1).")]
    [Range(0f, 1f)]
    public float pausedBlurWeight = 1f;

    private float defaultBlurWeight = 0f;

    private PlayerInputActions inputActions;

    private float pitch = 0f;
    private Quaternion upperBodyInitialLocalRot;

    private bool isFirstPerson = true;
    private bool isAiming = false;

    private float defaultFOV_FPS;
    private float defaultFOV_TPS;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.SwitchView.performed += OnSwitchView;
        inputActions.Player.Aim.performed += OnAimPerformed;
        inputActions.Player.Aim.canceled += OnAimCanceled;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (upperBody != null)
            upperBodyInitialLocalRot = upperBody.localRotation;

        // uložíme si pôvodné FOV pre obe kamery
        if (firstPersonCamera != null)
            defaultFOV_FPS = firstPersonCamera.fieldOfView;

        if (thirdPersonCamera != null)
            defaultFOV_TPS = thirdPersonCamera.fieldOfView;

        // uložíme si defaultnú hodnotu blurWeight (na začiatku žiadny alebo jemný blur)
        if (blurVolume != null)
            defaultBlurWeight = blurVolume.weight;

        ApplyCameraMode();
    }

    private void OnDisable()
    {
        inputActions.Player.SwitchView.performed -= OnSwitchView;
        inputActions.Player.Aim.performed -= OnAimPerformed;
        inputActions.Player.Aim.canceled -= OnAimCanceled;
        inputActions.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnSwitchView(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        isFirstPerson = !isFirstPerson;
        ApplyCameraMode();

        // Keď prepneš mód, nech sa kamera necuckne
        pitch = Mathf.Clamp(
            pitch,
            isFirstPerson ? -pitchLimitFPS : -pitchLimitTPS,
            isFirstPerson ?  pitchLimitFPS :  pitchLimitTPS
        );
    }

    private void OnAimPerformed(InputAction.CallbackContext ctx)
    {
        isAiming = true;
    }

    private void OnAimCanceled(InputAction.CallbackContext ctx)
    {
        isAiming = false;
    }

    private void ApplyCameraMode()
    {
        if (firstPersonCamera)
            firstPersonCamera.enabled = isFirstPerson;

        if (thirdPersonCamera)
            thirdPersonCamera.enabled = !isFirstPerson;

        ToggleListener(firstPersonCamera, isFirstPerson);
        ToggleListener(thirdPersonCamera, !isFirstPerson);
    }

    private void ToggleListener(Camera cam, bool enabled)
    {
        if (!cam) return;
        var listener = cam.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = enabled;
    }

    private void Update()
    {
        // --- Blur podľa pauzy (pozadie rozmazané, HUD ostáva ostrý v Screen Space - Overlay) ---
        if (blurVolume != null)
        {
            bool paused = PausePanel.IsGamePaused;   // static flag z PausePanel
            blurVolume.weight = paused ? pausedBlurWeight : defaultBlurWeight;
        }

        // Ak je hra v pauze, hráčom nepohybujeme kamerou
        if (PausePanel.IsGamePaused)
            return;

        if (playerBody == null) return;

        Vector2 look = inputActions.Player.Look.ReadValue<Vector2>();

        // ------------------ YAW ------------------
        playerBody.Rotate(Vector3.up * (look.x * mouseSensitivityX));

        // ------------------ PITCH ------------------
        float mouseY = look.y * mouseSensitivityY;

        if (isFirstPerson)
        {
            // V FPS jemný pitch
            pitch -= mouseY * 0.4f;
            pitch = Mathf.Clamp(pitch, -pitchLimitFPS, pitchLimitFPS);
        }
        else
        {
            // V TPS väčší pitch
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -pitchLimitTPS, pitchLimitTPS);
        }

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Upper body držíme rovno (bez náklonov)
        if (upperBody != null)
            upperBody.localRotation = upperBodyInitialLocalRot;

        // ------------------ ZOOM (Aim) ------------------
        Camera activeCam = isFirstPerson ? firstPersonCamera : thirdPersonCamera;
        if (activeCam != null)
        {
            float defaultFOV = isFirstPerson ? defaultFOV_FPS : defaultFOV_TPS;
            float targetFOV = isAiming ? zoomedFOV : defaultFOV;

            activeCam.fieldOfView = Mathf.Lerp(
                activeCam.fieldOfView,
                targetFOV,
                Time.unscaledDeltaTime * zoomSpeed   // keďže pri pauze je timeScale 0, ale Update sa už nevolá
            );
        }
    }
}
