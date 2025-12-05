using UnityEngine;
using TMPro;

/// <summary>
/// Univerzálny inventár hráča (mozgy, náboje, medkity, kevlar...).
/// Pripni na Playera.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Item Counts")]
    public int brains  = 0;
    public int ammo    = 0;   // zrkadlí rezervnú muníciu z Ammo.cs
    public int medkits = 1;   // 🔹 hráč začína s 1 medkitom
    public int kevlar  = 0;

    [Header("UI References")]
    public TextMeshProUGUI brainsText;
    public TextMeshProUGUI ammoText;    // ak chceš extra ammo HUD, môžeme použiť neskôr
    public TextMeshProUGUI medkitText;
    public TextMeshProUGUI kevlarText;

    [Header("Interact UI")]
    [Tooltip("TMP Text pre interakciu (napr. 'Press E to pick up').")]
    public TextMeshProUGUI interactText;

    [Header("Rich Text ikony (TMP <sprite>)")]
    [Tooltip("Tag pre ikonku mozgu (napr. <sprite index=0> alebo <sprite name=brain>)")]
    public string brainIconTag = "<sprite index=0>";

    [Tooltip("Tag pre ikonku medkitu (napr. <sprite name=aid>)")]
    public string medkitIconTag = "<sprite name=aid>";

    [Tooltip("Tag pre ikonku kevlaru/shieldu (napr. <sprite name=shield>)")]
    public string kevlarIconTag = "<sprite name=shield>";

    [Header("External Systems")]
    [Tooltip("Ammo systém, ktorý rieši magazine/reserve + UI 30/90.")]
    public Ammo ammoSystem;

    private void Awake()
    {
        // ak si zabudol priradiť v Inspectore, skúsime nájsť na tom istom objekte
        if (ammoSystem == null)
        {
            ammoSystem = GetComponent<Ammo>();
        }

        HideInteract();
    }

    private void Start()
    {
        RefreshUI();
    }

    public void AddItem(InventoryItemType type, int amount)
    {
        switch (type)
        {
            case InventoryItemType.Brain:
                brains += amount;
                if (brains < 0) brains = 0;
                break;

            case InventoryItemType.Ammo:
                // náboje rieši Ammo.cs
                if (ammoSystem != null)
                {
                    ammoSystem.AddReserveAmmo(amount);
                    ammo = ammoSystem.currentReserve;  // mirror hodnoty
                }
                else
                {
                    // fallback, keby Ammo nebolo nastavené
                    ammo += amount;
                    if (ammo < 0) ammo = 0;
                }
                break;

            case InventoryItemType.Medkit:
                medkits += amount;
                if (medkits < 0) medkits = 0;
                break;

            case InventoryItemType.Kevlar:
                kevlar += amount;
                if (kevlar < 0) kevlar = 0;
                break;
        }

        RefreshUI();
    }

    /// <summary>
    /// Skúsi použiť 1 medkit. 
    /// Vráti true, ak sa medkit skutočne minul.
    /// </summary>
    public bool TryUseMedkit()
    {
        if (medkits <= 0)
        {
            Debug.Log("[PlayerInventory] Žiadne medkity na použitie.");
            return false;
        }

        medkits--;
        if (medkits < 0) medkits = 0;

        RefreshUI();
        Debug.Log($"[PlayerInventory] Medkit použitý. Zostáva {medkits}x.");

        return true;
    }

    public void RefreshUI()
    {
        if (brainsText != null)
            brainsText.text = $"{brainIconTag} {brains}";

        // ammoText zatiaľ nepoužívame, hlavné ammo UI rieši Ammo.cs

        if (medkitText != null)
            medkitText.text = $"{medkitIconTag} {medkits}";

        if (kevlarText != null)
            kevlarText.text = $"{kevlarIconTag} {kevlar}";
    }

    public void ShowInteract(string message)
    {
        if (interactText == null)
        {
            Debug.LogWarning("[PlayerInventory] interactText nie je priradený!");
            return;
        }

        interactText.text = message;
        interactText.gameObject.SetActive(true);
    }

    public void HideInteract()
    {
        if (interactText == null)
            return;

        interactText.gameObject.SetActive(false);
    }
}
