using UnityEngine;

/// <summary>
/// Dáta o pickupe (mozog, ammo, atď.).
/// Logiku vstupu (E + vzdialenosť) rieši BrainInteract.
/// </summary>
public class BrainPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Aký typ itemu sa pridá do inventára.")]
    public InventoryItemType itemType = InventoryItemType.Brain;

    [Tooltip("Koľko kusov sa pridá.")]
    public int amount = 1;

    /// <summary>
    /// Zavolá BrainInteract, keď hráč stlačí E v dosahu.
    /// </summary>
    public void Pickup(PlayerInventory inventory)
    {
        if (inventory == null) return;

        inventory.AddItem(itemType, amount);
    }
}
