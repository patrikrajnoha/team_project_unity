using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;         // Animator s tvojimi animáciami
    public WeaponRaycast weapon;      // skript zbrane (raycast)
    public Ammo ammo;                 // <<< NOVÉ: ammo systém (30/90, UI)

    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.0f;     // pre sprint (RifleRun)
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Combat")]
    public float reloadDuration = 1.5f;   // dĺžka animácie reloadu (prispôsob podľa klipu)

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private float verticalVelocity;

    // stavové premenné pre animátor
    private bool isSprinting;
    private bool isFiring;
    private bool isReloading;

    private float reloadTimer;

    private void Awake()
    {
        controller   = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        // Ak zabudneš nastaviť zbraň v Inspectore, skúsime ju nájsť v deťoch
        if (weapon == null)
        {
            weapon = GetComponentInChildren<WeaponRaycast>();
        }

        // Ak zabudneš nastaviť Ammo v Inspectore, skúsime ho nájsť na tom istom GameObjecte
        if (ammo == null)
        {
            ammo = GetComponent<Ammo>();
        }

        // WeaponRaycast nechá muníciu na Ammo systéme
        if (weapon != null)
        {
            weapon.infiniteAmmo = true;
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Jump.performed   += OnJump;
        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Sprint.canceled  += OnSprintCanceled;

        inputActions.Player.Fire.performed   += OnFirePerformed;
        inputActions.Player.Fire.canceled    += OnFireCanceled;

        inputActions.Player.Reload.performed += OnReloadPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed   -= OnJump;
        inputActions.Player.Sprint.performed -= OnSprintPerformed;
        inputActions.Player.Sprint.canceled  -= OnSprintCanceled;

        inputActions.Player.Fire.performed   -= OnFirePerformed;
        inputActions.Player.Fire.canceled    -= OnFireCanceled;

        inputActions.Player.Reload.performed -= OnReloadPerformed;

        inputActions.Player.Disable();
    }

    private void Update()
    {
        // vstup na pohyb
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        HandleMovement();
        HandleReloadTimer();
        UpdateAnimator();
    }

    // --- POHYB (WASD, gravitácia, skok) ---
    private void HandleMovement()
    {
        // smer pohybu v lokálnom priestore hráča
        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);

        float targetSpeed = isSprinting ? runSpeed : walkSpeed;
        Vector3 horizontalVelocity = moveDirection.normalized * targetSpeed * inputMagnitude;

        // gravitácia
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // malá hodnota, aby bol „pri zemi“
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    // --- časovanie reloadu ---
    private void HandleReloadTimer()
    {
        if (!isReloading) return;

        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
        {
            isReloading = false;

            // po dokončení reloadu doplníme zásobník cez Ammo systém
            if (ammo != null)
            {
                ammo.Reload();
            }
        }
    }

    // --- ANIMÁTOR (všetky parametre) ---
    private void UpdateAnimator()
    {
        if (animator == null) return;

        // rýchlosť po rovine (X,Z) podľa CharacterControlleru
        Vector3 horizontalVel = controller.velocity;
        horizontalVel.y = 0f;
        float speed = horizontalVel.magnitude;

        animator.SetFloat("Speed", speed);                 // Blend Tree Locomotion / Firing / Reload
        animator.SetBool("IsGrounded", controller.isGrounded);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsFiring", isFiring);
        animator.SetBool("IsReloading", isReloading);
    }

    // --- SKOK (Input callback) ---
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!controller.isGrounded) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }

    // --- SPRINT (Shift) ---
    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        isSprinting = false;
    }

    // --- STREĽBA (LMB) ---
    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        if (isReloading) return; // počas reloadu nestrieľame
        if (weapon == null) return;

        // skúsime minúť náboj z Ammo systému
        if (ammo != null)
        {
            // ak TryConsumeBullet vráti false -> zásobník je prázdny
            if (!ammo.TryConsumeBullet())
            {
                Debug.Log("Out of ammo (magazine empty)!");
                return;
            }
        }

        isFiring = true;

        // vystrelíme jednu ranu – WeaponRaycast rieši raycast + damage
        weapon.FireOnce();
    }

    private void OnFireCanceled(InputAction.CallbackContext ctx)
    {
        isFiring = false;
    }

    // --- RELOAD (R) ---
    private void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        if (isReloading) return;

        // ak máme ammo systém, skontrolujeme, či vôbec má zmysel reloadovať
        if (ammo != null && !ammo.CanReload())
        {
            // zásobník plný alebo žiadna rezerva
            return;
        }

        isReloading = true;
        isFiring = false;             // pri reload-e prestaneme strieľať
        reloadTimer = reloadDuration;
    }

    // --- SMRŤ HRÁČA (volaj z PlayerHealth) ---
    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // vypnutie ovládania po smrti
        inputActions.Player.Disable();
        enabled = false;
    }
}
