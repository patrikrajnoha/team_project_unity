using UnityEngine;

/// <summary>
/// Hitbox pre hlavu zombíka.
/// Stačí pridať Sphere Collider na Head bone a tento skript.
/// WeaponRaycast pri zásahu zavolá OnHeadshot().
/// </summary>
public class ZombieHeadHitbox : MonoBehaviour
{
    private ZombieAI zombie;

    private void Awake()
    {
        // nájdeme ZombieAI v rodičovských objektoch
        zombie = GetComponentInParent<ZombieAI>();

        if (zombie == null)
        {
            Debug.LogError("[ZombieHeadHitbox] Nenašiel som ZombieAI v parentoch!", this);
        }
    }

    /// <summary>
    /// Zavolá sa zo zbrane pri headshote.
    /// </summary>
    /// <param name="damage">Damage zbrane (ignoruje sa, je to instant kill)</param>
    /// <param name="hitPoint">Miesto zásahu – kvôli dropnutiu mozgu</param>
    public void OnHeadshot(int damage, Vector3 hitPoint)
    {
        Debug.Log($"[ZombieHeadHitbox] HEADSHOT! hitPoint={hitPoint}", this);

        if (zombie != null)
        {
            zombie.HandleHeadshot(damage, hitPoint);
        }
    }
}
