using UnityEngine;
using TMPro;

public class Ammo : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Skript zbrane, ktorá strieľa raycastom.")]
    public WeaponRaycast weapon;

    [Tooltip("TextMeshPro UI pre zobrazenie munície (napr. 30/90).")]
    public TextMeshProUGUI ammoText;

    [Header("Nastavenia munície")]
    [Tooltip("Kapacita zásobníka (max počet nábojov v zásobníku).")]
    public int magazineCapacity = 30;

    [Tooltip("Maximálny počet nábojov v rezerve.")]
    public int maxReserveAmmo = 90;

    [Header("Aktuálny stav")]
    [Tooltip("Aktuálny počet nábojov v zásobníku.")]
    public int currentMagazine;

    [Tooltip("Aktuálny počet nábojov v rezerve.")]
    public int currentReserve;

    // Odkaz na PlayerInventory – aby vedel, koľko máme nábojov
    private PlayerInventory inventory;

    private void Awake()
    {
        // nájdi inventár na tom istom GameObjecte (Player)
        inventory = GetComponent<PlayerInventory>();

        // Počiatočný stav: 30/90
        currentMagazine = magazineCapacity; // 30
        currentReserve  = maxReserveAmmo;   // 90

        // Prepojíme hodnoty aj do WeaponRaycast
        SyncWeapon();
        UpdateUI();
    }

    /// <summary>
    /// Pokus o spotrebovanie jedného náboja pri streľbe.
    /// Vracia true = náboj bol použitý, false = prázdny zásobník.
    /// </summary>
    public bool TryConsumeBullet()
    {
        if (currentMagazine <= 0)
        {
            // zásobník je prázdny
            return false;
        }

        currentMagazine--;
        SyncWeapon();
        UpdateUI();
        return true;
    }

    /// <summary>
    /// Či sa oplatí reloadovať (zásobník nie je plný a máme rezervnú muníciu).
    /// </summary>
    public bool CanReload()
    {
        return currentMagazine < magazineCapacity && currentReserve > 0;
    }

    /// <summary>
    /// Prebehne reload – doplní zásobník z rezervy.
    /// Volaj to na konci reload animácie (napr. z PlayerControlleru).
    /// </summary>
    public void Reload()
    {
        if (!CanReload())
            return;

        int needed = magazineCapacity - currentMagazine;      // koľko chýba do plna
        int toLoad = Mathf.Min(needed, currentReserve);       // koľko vieme reálne doplniť

        currentReserve  -= toLoad;
        currentMagazine += toLoad;

        SyncWeapon();
        UpdateUI();
    }

    /// <summary>
    /// Pridanie munície do rezervy (napr. pickup na mape).
    /// </summary>
    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0) return;

        currentReserve = Mathf.Clamp(currentReserve + amount, 0, maxReserveAmmo);
        UpdateUI();
    }

    /// <summary>
    /// Nastaví obe hodnoty priamo (napr. pre cheaty / debug).
    /// </summary>
    public void SetAmmo(int magazine, int reserve)
    {
        currentMagazine = Mathf.Clamp(magazine, 0, magazineCapacity);
        currentReserve  = Mathf.Clamp(reserve,  0, maxReserveAmmo);

        SyncWeapon();
        UpdateUI();
    }

    /// <summary>
    /// Zosynchronizuje stav s WeaponRaycast skriptom.
    /// </summary>
    private void SyncWeapon()
    {
        if (weapon == null) return;

        weapon.magazineSize   = magazineCapacity;
        weapon.ammoInMagazine = currentMagazine;
    }

    /// <summary>
    /// Aktualizuje TMP text, formát: 30/90 alebo 00/00
    /// + odovzdá info do PlayerInventory.
    /// </summary>
    private void UpdateUI()
    {
        if (ammoText != null)
        {
            // 2-miestne čísla: 03/07, 30/90, 00/00
            string magStr     = currentMagazine.ToString("00");
            string reserveStr = currentReserve.ToString("00");

            ammoText.text = $"{magStr}/{reserveStr}";
        }

        // posuň info o nábojoch do inventára
        if (inventory != null)
        {
            inventory.ammo = currentReserve;   // v inventári berieme „rezervu“
            inventory.RefreshUI();            // nech sa prepočíta HUD (mozgy, medkity, atď.)
        }
    }
}
