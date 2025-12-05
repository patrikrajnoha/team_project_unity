using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM
using TMPro;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Tooltip("Maximálne zdravie hráča.")]
    public int maxHealth = 100;

    [Tooltip("Aktuálne zdravie hráča.")]
    public int currentHealth = 100;

    [Header("Shield")]
    [Tooltip("Shield pre vestu (+50).")]
    public int vestShieldAmount = 50;

    [Tooltip("Shield pre vestu + helmu (+100).")]
    public int vestHelmetShieldAmount = 100;

    [Tooltip("Aktuálny shield hráča.")]
    public int currentShield = 0;

    // true = hráč už normálne kúpil nejaký armor (vesta alebo vesta+helma)
    private bool armorPurchased = false;

    [Header("UI (TextMeshPro)")]
    [Tooltip("Text pre zobrazenie zdravia s ikonou.")]
    public TextMeshProUGUI healthText;

    [Tooltip("Text pre zobrazenie shieldu s ikonou.")]
    public TextMeshProUGUI shieldText;

    [Header("Rich Text ikony (TMP <sprite>)")]
    [Tooltip("Tag pre ikonku zdravia (napr. <sprite name=heart>)")]
    public string healthIconTag = "<sprite name=heart>";

    [Tooltip("Tag pre ikonku shieldu (napr. <sprite name=shield>)")]
    public string shieldIconTag = "<sprite name=shield>";

    [Header("Ovládanie hráča")]
    private FPSPlayerController playerController;

    [Header("Game Over")]
    [Tooltip("Referencie na GameOver skript (napr. na Canvase).")]
    public GameOver gameOver;

    [Header("Vizuálny damage efekt")]
    [Tooltip("Efekt desaturácie obrazovky pri zásahu (DamageScreenDesaturate na Global Volume).")]
    public DamageScreenDesaturate damageScreenEffect;

    [Header("Inventár")]
    [Tooltip("Inventár hráča s medkitmi, mozgami, atď.")]
    public PlayerInventory playerInventory;

    // New Input System actions
    private PlayerInputActions inputActions;

    private bool isDead = false;

    private void Awake()
    {
        playerController = GetComponent<FPSPlayerController>();
        inputActions = new PlayerInputActions();

        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        // 🔹 ŠTART HODNOTY – 100 HP, 100 shield (vest + helmet)
        currentHealth = maxHealth;
        currentShield = vestHelmetShieldAmount;
        armorPurchased = true;    // berieme, že hráč už má armor
        isDead = false;

        Debug.Log($"PlayerHealth.Awake: HP={currentHealth}, shield={currentShield}, gameOver={(gameOver ? gameOver.name : "NULL")}");

        UpdateHUD();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.CheatShield.performed += OnCheatShield;
        inputActions.Player.Heal.performed += OnUseMedkit;
  // 🔹 Q (alebo čo nastavíš)
    }

    private void OnDisable()
    {
        inputActions.Player.CheatShield.performed -= OnCheatShield;
        inputActions.Player.Heal.performed -= OnUseMedkit;

        inputActions.Player.Disable();
    }

    private void OnCheatShield(InputAction.CallbackContext ctx)
    {
        Debug.Log("PlayerHealth.OnCheatShield: cheat aktivovaný.");
        GiveVestHelmetShieldCheat();
    }

    /// <summary>
    /// Q – použitie medkitu, doplní HP na maxHealth (100).
    /// </summary>
    private void OnUseMedkit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (isDead)
        {
            Debug.Log("PlayerHealth.OnUseMedkit: hráč je mŕtvy, ignorujem.");
            return;
        }

        // ak je pauza, ignoruj (nech si to hráč nevykliká v menu)
        if (PausePanel.IsGamePaused)
        {
            Debug.Log("PlayerHealth.OnUseMedkit: hra je pauznutá, ignorujem.");
            return;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("PlayerHealth.OnUseMedkit: HP je už na maxime, netreba heal.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerHealth.OnUseMedkit: playerInventory je NULL, nemám odkiaľ brať medkity.");
            return;
        }

        // pokusíme sa minúť 1 medkit
        bool used = playerInventory.TryUseMedkit();
        if (!used)
        {
            Debug.Log("PlayerHealth.OnUseMedkit: žiadne medkity.");
            return;
        }

        // doplníme zdravie na max
        currentHealth = maxHealth;
        Debug.Log($"PlayerHealth.OnUseMedkit: medkit použitý, HP doplnené na {currentHealth}.");

        UpdateHUD();
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            Debug.Log("PlayerHealth.TakeDamage: hráč je už mŕtvy, ignorujem damage.");
            return;
        }

        int damage = Mathf.RoundToInt(amount);
        if (damage <= 0) return;

        Debug.Log($"PlayerHealth.TakeDamage: prichádzajúci damage={damage}, HP={currentHealth}, shield={currentShield}");

        // vizuálny efekt – akýkoľvek damage
        if (damageScreenEffect != null)
        {
            damageScreenEffect.PlayDamageEffect();
        }

        // 1) najprv berieme zo shieldu
        if (currentShield > 0)
        {
            int shieldDamage = Mathf.Min(currentShield, damage);
            currentShield -= shieldDamage;
            damage -= shieldDamage;
            Debug.Log($"PlayerHealth.TakeDamage: po shielde -> damage={damage}, shield={currentShield}");
        }

        // 2) zvyšok ide do zdravia
        if (damage > 0)
        {
            currentHealth -= damage;
            Debug.Log($"PlayerHealth.TakeDamage: po HP -> HP={currentHealth}");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        UpdateHUD();
    }

    public void BuyVest()
    {
        if (armorPurchased) return;

        armorPurchased = true;
        currentShield = vestShieldAmount;
        Debug.Log($"PlayerHealth.BuyVest: shield={currentShield}");
        UpdateHUD();
    }

    public void BuyVestAndHelmet()
    {
        if (armorPurchased) return;

        armorPurchased = true;
        currentShield = vestHelmetShieldAmount;
        Debug.Log($"PlayerHealth.BuyVestAndHelmet: shield={currentShield}");
        UpdateHUD();
    }

    private void GiveVestHelmetShieldCheat()
    {
        currentShield = vestHelmetShieldAmount;
        armorPurchased = true;
        Debug.Log($"PlayerHealth.GiveVestHelmetShieldCheat: shield={currentShield}");
        UpdateHUD();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("PlayerHealth.Die: hráč zomrel.");

        UpdateHUD();

        if (playerController != null)
        {
            Debug.Log("PlayerHealth.Die: púšťam death animáciu.");
            playerController.PlayDeathAnimation();
        }
        else
        {
            Debug.LogWarning("PlayerHealth.Die: FPSPlayerController nie je nastavený!");
        }

        if (gameOver != null)
        {
            Debug.Log("PlayerHealth.Die: volám gameOver.ShowGameOver().");
            gameOver.ShowGameOver();
        }
        else
        {
            Debug.LogError("PlayerHealth.Die: gameOver referencia je NULL!");
        }
    }

    private void UpdateHUD()
    {
        // HEALTH text s ikonou
        if (healthText != null)
        {
            healthText.text = $"{healthIconTag} {currentHealth:000}";
        }

        // SHIELD text s ikonou
        if (shieldText != null)
        {
            if (currentShield > 0)
            {
                shieldText.gameObject.SetActive(true);
                shieldText.text = $"{shieldIconTag} {currentShield:000}";
            }
            else
            {
                shieldText.gameObject.SetActive(false);
            }
        }
    }
}
