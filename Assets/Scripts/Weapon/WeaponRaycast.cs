using UnityEngine;
using UnityEngine.InputSystem;   // kvôli typom, ak by si chcel rozšíriť

public class WeaponRaycast : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Fallback kamera hráča, ak by Camera.main nebola dostupná.")]
    public Camera playerCamera;

    [Tooltip("Pre crosshair kick efekt pri výstrele.")]
    public CrosshairAnimator crosshairAnimator;

    [Header("Effects")]
    [Tooltip("Empty objekt na konci hlavne (Muzzle).")]
    public Transform muzzlePoint;

    [Tooltip("Muzzle flash VFX skript (na objekte s ParticleSystem-om pri hlavni).")]
    public MuzzleFlashVFX muzzleFlashVFX;

    [Tooltip("Voliteľné: prefab efektu pri zásahu (iskry, krv, atď.).")]
    public GameObject hitEffectPrefab;

    [Header("Weapon Stats")]
    [Tooltip("Poškodenie jedného zásahu.")]
    public float damage = 25f;

    [Tooltip("Maximálny dosah strely v metroch.")]
    public float range = 100f;

    [Tooltip("Počet rán za sekundu (fire rate).")]
    public float fireRate = 10f;

    [Header("Ammo")]
    [Tooltip("Kapacita zásobníka.")]
    public int magazineSize = 30;

    [Tooltip("Aktuálny počet nábojov v zásobníku.")]
    public int ammoInMagazine = 30;

    [Tooltip("Ak je zapnuté, munícia sa nebude míňať.")]
    public bool infiniteAmmo = false;

    // ============================================================
    //                         LASER
    // ============================================================
    [Header("Laser")]
    [Tooltip("LineRenderer pre laser. Môže byť na zbrani alebo samostatný objekt.")]
    public LineRenderer laserLine;

    [Tooltip("Či má byť laser zapnutý na začiatku hry.")]
    public bool laserInitiallyOn = false;

    private bool _laserOn = false;

    private float _nextTimeToFire = 0f;

    // Aktívna kamera = preferuj Camera.main, inak fallback na playerCamera
    private Camera ActiveCamera
    {
        get
        {
            if (Camera.main != null)
                return Camera.main;

            return playerCamera;
        }
    }

    private void Start()
    {
        _laserOn = laserInitiallyOn;
        if (laserLine != null)
        {
            laserLine.enabled = _laserOn;
        }
    }

    private void Update()
    {
        UpdateLaser();
    }

    // ============================================================
    //                         FIRE FUNCTION
    // ============================================================
    public void FireOnce()
    {
        if (Time.time < _nextTimeToFire)
            return;

        if (!infiniteAmmo && ammoInMagazine <= 0)
            return;

        _nextTimeToFire = Time.time + 1f / fireRate;

        if (!infiniteAmmo)
        {
            ammoInMagazine = Mathf.Max(0, ammoInMagazine - 1);
        }

        // muzzle flash VFX
        if (muzzleFlashVFX != null)
        {
            if (muzzlePoint != null)
            {
                muzzleFlashVFX.transform.position = muzzlePoint.position;
                muzzleFlashVFX.transform.rotation = muzzlePoint.rotation;
            }
            muzzleFlashVFX.PlayFlash();
        }

        // crosshair kick
        if (crosshairAnimator != null)
        {
            crosshairAnimator.OnShootKick();
        }

        Camera cam = ActiveCamera;
        if (cam == null)
        {
            Debug.LogWarning("[WeaponRaycast] Žiadna aktívna kamera! Streľba zrušená.");
            return;
        }

        // ============================================================
        //      RAYCAST Z CROSSHAIRU — stred obrazovky (FPS aj TPS)
        // ============================================================
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(center);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log($"[WeaponRaycast] Zásah: {hit.collider.name} na pozícii {hit.point}");

            int intDamage = Mathf.RoundToInt(damage);

            // HEADSHOT
            ZombieHeadHitbox headHitbox = hit.collider.GetComponent<ZombieHeadHitbox>();

            if (headHitbox != null)
            {
                Debug.Log($"[WeaponRaycast] HEADSHOT na {headHitbox.gameObject.name}, dmg: {intDamage}");
                headHitbox.OnHeadshot(intDamage, hit.point);
            }
            else
            {
                // bežný zásah – cez IDamageable (zombie telo, portál, iné objekty)
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    Debug.Log($"[WeaponRaycast] IDamageable nájdený na {damageable}. Dávam dmg: {damage}");
                    damageable.TakeDamage(damage);
                }
                else
                {
                    Debug.Log("[WeaponRaycast] Žiadny IDamageable na zásiahnutom objekte.");
                }
            }

            // krv z zombíka
            ZombieBloodVFX blood = hit.collider.GetComponentInParent<ZombieBloodVFX>();
            if (blood != null)
            {
                blood.SpawnBlood(hit.point, -ray.direction);
            }

            // efekt zásahu (napr. iskry / dopad na stene)
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // pre debug, či vôbec niečo trafil ray
            // Debug.Log("[WeaponRaycast] Raycast nič netrafil.");
        }
    }

    // ============================================================
    //                     REFILL MAGAZINE (RELOAD)
    // ============================================================
    public void RefillMagazine()
    {
        ammoInMagazine = magazineSize;
    }

    // ============================================================
    //                     LASER API
    // ============================================================

    public void ToggleLaser()
    {
        SetLaser(!_laserOn);
    }

    public void SetLaser(bool state)
    {
        _laserOn = state;
        if (laserLine != null)
        {
            laserLine.enabled = _laserOn;
        }
    }

    private void UpdateLaser()
    {
        if (!_laserOn || laserLine == null)
            return;

        Camera cam = ActiveCamera;
        if (cam == null)
            return;

        // Ray z centra obrazovky (rovnako ako streľba)
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(center);
        RaycastHit hit;

        Vector3 startPos = muzzlePoint != null ? muzzlePoint.position : ray.origin;
        Vector3 endPos;

        if (Physics.Raycast(ray, out hit, range))
        {
            endPos = hit.point;
        }
        else
        {
            endPos = ray.origin + ray.direction * range;
        }

        // nastavíme body LineRendereru
        laserLine.positionCount = 2;
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPos);
    }
}
