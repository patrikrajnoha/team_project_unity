using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZombiePortal : MonoBehaviour, IDamageable
{
    // --------- STATIC – spoločné pre všetky portály v scéne ---------
    private static bool initialized = false;

    private static int destroyedPortals = 0;

    private static TMP_Text sharedGoalText;
    private static string   sharedGoalTextFormat = "Defeat portals {0}/{1}";

    // Jeden z portálov nastaví tento final portal (z Inspectoru)
    private static GameObject sharedFinalPortal;

    // ------------------ INSTANCE FIELDS (Inspector) ------------------

    [Header("Health")]
    [Tooltip("Maximálne zdravie portálu.")]
    public float maxHealth = 400f;

    [Tooltip("Aktuálne zdravie portálu (len na debug).")]
    public float currentHealth;

    [Header("Spawn nastavenia")]
    [Tooltip("Prefab tvojho zombie nepriateľa.")]
    public GameObject zombiePrefab;

    [Tooltip("Miesto, kde sa má zombie objaviť. Ak je null, použije sa pozícia portálu.")]
    public Transform spawnPoint;

    [Tooltip("Základný čas medzi spawnmi, keď je portál úplne zdravý.")]
    public float baseSpawnInterval = 6f;

    [Tooltip("Najkratší čas medzi spawnmi, keď je portál skoro zničený.")]
    public float minSpawnInterval = 1.5f;

    [Tooltip("Maximálny počet zombíkov naraz z tohto portálu.")]
    public int maxAliveZombies = 20;

    [Header("HUD")]
    [Tooltip("TMP text pre cieľ hry (GameGoal_1). Stačí nastaviť na jednom portáli.")]
    public TMP_Text goalText;

    [Tooltip("Formát textu – {0} = zničené portály, {1} = všetky portály.")]
    public string goalTextFormat = "Defeat portals {0}/{1}";

    [Header("Final Portal")]
    [Tooltip("Final portal objekt v scéne (deaktivovaný). Stačí priradiť na JEDNOM portáli.")]
    public GameObject finalPortal;

    [Header("Efekty")]
    [Tooltip("VFX prefab pri zničení portálu (voliteľné).")]
    public GameObject destroyVfx;

    [Tooltip("AudioSource na prehrávanie zvukov (voliteľné).")]
    public AudioSource audioSource;

    [Tooltip("Zvuk pri zásahu (voliteľné).")]
    public AudioClip hitClip;

    [Tooltip("Zvuk pri zničení (voliteľné).")]
    public AudioClip destroyClip;

    // ------------------ INTERNAL ------------------
    private float spawnTimer;
    private bool  isDestroyed = false;

    private readonly List<GameObject> aliveZombies = new List<GameObject>();

    // ----------------------------------------------------------    
    private void Awake()
    {
        currentHealth = maxHealth;

        // Reset statických premenných pri prvom portáli v scéne
        if (!initialized)
        {
            initialized          = true;
            destroyedPortals     = 0;
            sharedGoalText       = null;
            sharedGoalTextFormat = "Defeat portals {0}/{1}";
            sharedFinalPortal    = null;
        }
    }

    private void Start()
    {
        // Nastav shared HUD text – stačí, keď goalText je priradený aspoň na jednom portáli
        if (sharedGoalText == null && goalText != null)
        {
            sharedGoalText = goalText;
            if (!string.IsNullOrEmpty(goalTextFormat))
                sharedGoalTextFormat = goalTextFormat;
        }

        // Nastav shared final portal – stačí priradiť na jednom portáli
        if (sharedFinalPortal == null && finalPortal != null)
        {
            sharedFinalPortal = finalPortal;
            sharedFinalPortal.SetActive(false); // istota, že je skrytý na začiatku
        }

        UpdateSharedHUD();

        // hneď na začiatku spawnni jedného zombíka
        SpawnZombie();
        spawnTimer = baseSpawnInterval;
    }

    private void Update()
    {
        if (isDestroyed)
            return;

        // Vyčisti null zombíkov
        for (int i = aliveZombies.Count - 1; i >= 0; i--)
        {
            if (aliveZombies[i] == null)
                aliveZombies.RemoveAt(i);
        }

        if (aliveZombies.Count >= maxAliveZombies)
            return;

        // automatický spawn
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnZombie();
            spawnTimer = GetCurrentSpawnInterval();
        }
    }

    // ------------------ SPAWN LOGIKA ------------------

    private void SpawnZombie()
    {
        if (zombiePrefab == null)
        {
            Debug.LogWarning($"[ZombiePortal] Chýba zombiePrefab na objekte {name}!");
            return;
        }

        Transform point = spawnPoint != null ? spawnPoint : transform;

        GameObject z = Instantiate(zombiePrefab, point.position, point.rotation);
        aliveZombies.Add(z);

        Debug.Log($"[ZombiePortal] Spawn zombie z portálu {name}. Aktuálnych: {aliveZombies.Count}");
    }

    private float GetCurrentSpawnInterval()
    {
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        return Mathf.Lerp(minSpawnInterval, baseSpawnInterval, healthPercent);
    }

    // ------------------ DAMAGE / DEATH ------------------

    public void TakeDamage(float amount)
    {
        if (isDestroyed)
            return;

        currentHealth -= amount;
        if (currentHealth < 0f)
            currentHealth = 0f;

        Debug.Log($"[ZombiePortal] {name} dostal damage {amount}. HP: {currentHealth}/{maxHealth}");

        if (hitClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitClip);
        }

        if (currentHealth > 0f)
        {
            // portál je poškodený – zrýchlime spawn
            float newInterval = GetCurrentSpawnInterval();
            float accelerated = newInterval * 0.5f;
            spawnTimer = Mathf.Min(spawnTimer, accelerated);
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        Debug.Log($"[ZombiePortal] {name} zničený!");

        // prestane sa počítať ako „Portal“
        gameObject.tag = "Untagged";

        // navýš globálny counter a updatni HUD
        destroyedPortals++;
        UpdateSharedHUD();

        // skús aktivovať final portal, ak už žiadny Portal neexistuje
        TryActivateFinalPortal();

        if (destroyClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroyClip);
        }

        if (destroyVfx != null)
        {
            Instantiate(destroyVfx, transform.position, Quaternion.identity);
        }

        // vypni kolider a rendery (portál zmizne)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        Destroy(gameObject, 2f);
    }

    // ------------------ STATIC POMOCNÉ METÓDY ------------------

    /// <summary>
    /// Celkový počet portálov = aktívne portály (tag "Portal") + zničené portály.
    /// Funguje aj pri dynamickom spawnovaní.
    /// </summary>
    private static int GetTotalPortalsDynamic()
    {
        int active = GameObject.FindGameObjectsWithTag("Portal").Length;
        return active + destroyedPortals;
    }

    private static void UpdateSharedHUD()
    {
        if (sharedGoalText == null) return;

        int destroyed = destroyedPortals;
        int total     = GetTotalPortalsDynamic();

        if (total < destroyed)
            total = destroyed;

        sharedGoalText.text = string.Format(sharedGoalTextFormat, destroyed, total);

        // Preškrtnutie, keď sú všetky portály preč
        if (destroyed >= total && total > 0)
            sharedGoalText.fontStyle |= FontStyles.Strikethrough;
        else
            sharedGoalText.fontStyle &= ~FontStyles.Strikethrough;
    }

    private static void TryActivateFinalPortal()
    {
        if (sharedFinalPortal == null) return;
        if (sharedFinalPortal.activeSelf) return;

        int activePortals = GameObject.FindGameObjectsWithTag("Portal").Length;

        if (activePortals == 0)
        {
            sharedFinalPortal.SetActive(true);
            Debug.Log("[ZombiePortal] All portals destroyed — final portal ACTIVATED.");
        }
    }
}
