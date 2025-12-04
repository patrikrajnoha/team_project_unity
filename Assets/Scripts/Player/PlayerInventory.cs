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
    public int medkits = 0;
    public int kevlar  = 0;

    [Header("UI References")]
    public TextMeshProUGUI brainsText;
    public TextMeshProUGUI ammoText;    // ak chceš extra ammo HUD, môžeme použiť neskôr
    public TextMeshProUGUI medkitText;
    public TextMeshProUGUI kevlarText;

    [Header("Interact UI")]
    [Tooltip("TMP Text pre interakciu (napr. 'Press E to pick up').")]
    public TextMeshProUGUI interactText;

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

    public void RefreshUI()
    {
        if (brainsText != null)
            brainsText.text = $"<sprite index=0> {brains}";

        // ammoText zatiaľ nepoužívame, hlavné ammo UI rieši Ammo.cs
        if (medkitText != null)
            medkitText.text = medkits.ToString();

        if (kevlarText != null)
            kevlarText.text = kevlar.ToString();
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
