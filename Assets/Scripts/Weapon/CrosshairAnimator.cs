using UnityEngine;

/// <summary>
/// Riadi veľkosť crosshairu podľa streľby, pohybu hráča a mierenia (RMB).
/// </summary>
public class CrosshairAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("RectTransform crosshairu (ak je null, vezme sa z tohto objektu).")]
    public RectTransform crosshairRect;

    [Tooltip("CharacterController hráča – kvôli rýchlosti pohybu.")]
    public CharacterController playerController;

    [Header("Sizes")]
    [Tooltip("Základná veľkosť (idle).")]
    public float idleSize = 40f;

    [Tooltip("Veľkosť po výstrele (okamžitý 'kick').")]
    public float shootKickSize = 60f;

    [Tooltip("Max veľkosť pri pohybe hráča.")]
    public float moveMaxSize = 80f;

    [Tooltip("Veľkosť pri Aim (RMB držaní).")]
    public float aimSize = 32f;

    [Header("Speeds")]
    [Tooltip("Ako rýchlo sa zväčšuje pri pohybe.")]
    public float moveLerpSpeed = 5f;

    [Tooltip("Ako rýchlo sa zmenšuje späť k cieľu.")]
    public float shrinkLerpSpeed = 10f;

    [Tooltip("Ako rýchlo sa mení veľkosť pri Aim.")]
    public float aimLerpSpeed = 12f;

    [Tooltip("Minimálna rýchlosť, od ktorej berieme hráča ako 'v pohybe'.")]
    public float moveSpeedThreshold = 0.05f;

    private float currentSize;
    private bool isAiming = false;

    private void Awake()
    {
        if (crosshairRect == null)
            crosshairRect = GetComponent<RectTransform>();

        currentSize = idleSize;
        ApplySize();
    }

    private void Update()
    {
        if (crosshairRect == null)
            return;

        float targetSize = idleSize;

        // ---------------------------------------------------------
        // AIM (Right Mouse Button držaný)
        // ---------------------------------------------------------
        if (isAiming)
        {
            targetSize = aimSize;
        }
        else
        {
            // -----------------------------------------------------
            // MOVEMENT (rozťahovanie pri pohybe)
            // -----------------------------------------------------
            if (playerController != null)
            {
                Vector3 v = playerController.velocity;
                v.y = 0f;
                if (v.magnitude > moveSpeedThreshold)
                {
                    targetSize = moveMaxSize;
                }
            }
        }

        // vyber rýchlosť lerpu
        float speed;

        if (isAiming)
            speed = aimLerpSpeed;
        else
            speed = (currentSize > targetSize) ? shrinkLerpSpeed : moveLerpSpeed;

        currentSize = Mathf.Lerp(currentSize, targetSize, speed * Time.deltaTime);
        ApplySize();
    }

    private void ApplySize()
    {
        crosshairRect.sizeDelta = new Vector2(currentSize, currentSize);
    }

    /// <summary>
    /// Okamžitý kick pri streľbe.
    /// </summary>
    public void OnShootKick()
    {
        currentSize = shootKickSize;
        ApplySize();
    }

    /// <summary>
    /// Zapnúť Aim (Right Mouse Button + držanie).
    /// </summary>
    public void SetAim(bool aiming)
    {
        isAiming = aiming;
    }
}
