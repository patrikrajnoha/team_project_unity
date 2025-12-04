using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZombiePortal : MonoBehaviour, IDamageable
{
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
    [Tooltip("TMP text pre cieľ hry (GameGoal_1).")]
    public TMP_Text goalText;

    [Tooltip("Formát textu – {0} = aktuálny počet zničených portálov.")]
    public string goalTextFormat = "Defeat portals {0}/1";

    [Header("Efekty")]
    [Tooltip("VFX prefab pri zničení portálu (voliteľné).")]
    public GameObject destroyVfx;

    [Tooltip("AudioSource na prehrávanie zvukov (voliteľné).")]
    public AudioSource audioSource;

    [Tooltip("Zvuk pri zásahu (voliteľné).")]
    public AudioClip hitClip;

    [Tooltip("Zvuk pri zničení (voliteľné).")]
    public AudioClip destroyClip;

    private float spawnTimer;
    private bool isDestroyed = false;

    private readonly List<GameObject> aliveZombies = new List<GameObject>();

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // HUD – na začiatku 0/1
        if (goalText != null)
        {
            goalText.fontStyle &= ~FontStyles.Strikethrough;   // pre istotu zruš strikethrough
            goalText.text = string.Format(goalTextFormat, 0);  // "Defeat portals 0/1"
        }

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

        // HUD – 1/1 a preškrtnúť
        if (goalText != null)
        {
            goalText.text = string.Format(goalTextFormat, 1); // "Defeat portals 1/1"
            goalText.fontStyle |= FontStyles.Strikethrough;
        }

        if (destroyClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroyClip);
        }

        if (destroyVfx != null)
        {
            Instantiate(destroyVfx, transform.position, Quaternion.identity);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        Destroy(gameObject, 2f);
    }
}
