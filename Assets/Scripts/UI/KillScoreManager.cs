using UnityEngine;
using TMPro;

/// <summary>
/// Jednoduchý kill score systém.
/// Každý zabitý objekt s tagom "Zombie" pridá 1 kill.
/// Pripni na nejaký GameObject v scéne (napr. HUD/GameManager)
/// a prepoj s TextMeshProUGUI.
/// </summary>
public class KillScoreManager : MonoBehaviour
{
    public static KillScoreManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("TextMeshProUGUI text, kde sa bude zobrazovať kill count.")]
    [SerializeField] private TextMeshProUGUI killText;

    [Header("Nastavenia textu")]
    [Tooltip("Prefix pred číslom (napr. 'KILLS: ').")]
    [SerializeField] private string prefix = "KILLS: ";

    /// <summary>
    /// Aktuálny počet killov.
    /// </summary>
    public int KillCount { get; private set; } = 0;

    private void Awake()
    {
        // jednoduchý singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Ak chceš, aby skóre prežilo load scény, odkomentuj:
        // DontDestroyOnLoad(gameObject);

        UpdateUI();
    }

    /// <summary>
    /// Pridá 1 kill do skóre.
    /// Volaj pri smrti zombíka.
    /// </summary>
    public void AddKill()
    {
        KillCount++;
        UpdateUI();
    }

    /// <summary>
    /// Vynuluje skóre. Môžeš volať pri reštarte levelu.
    /// </summary>
    public void ResetScore()
    {
        KillCount = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (killText == null) return;
        killText.text = $"{prefix}{KillCount}";
    }

    /// <summary>
    /// Voliteľné – ak chceš TMP text nastaviť dynamicky.
    /// </summary>
    public void SetKillText(TextMeshProUGUI tmp)
    {
        killText = tmp;
        UpdateUI();
    }
}
