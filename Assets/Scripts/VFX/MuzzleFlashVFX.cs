using UnityEngine;

/// <summary>
/// VFX pre muzzle flash.
/// Pripni na objekt, ktorý je ako dieťa pri muzzlePoint.
/// Nastaví ParticleSystem cez kód a poskytne metódu PlayFlash().
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class MuzzleFlashVFX : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("Počet častíc v jednom záblesku.")]
    public short particleCount = 20;

    [Tooltip("Ako dlho trvá záblesk.")]
    public float lifeTime = 0.06f;

    [Tooltip("Rýchlosť častíc.")]
    public float startSpeed = 4f;

    [Tooltip("Veľkosť častíc.")]
    public float startSize = 0.12f;

    [Tooltip("Farba záblesku (teplá oranžovo-žltá).")]
    public Color flashColor = new Color(1f, 0.8f, 0.3f, 1f);

    private ParticleSystem ps;
    private Material flashMaterial;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        // istota, že systém je stopnutý, môžeme meniť nastavenia
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // MAIN
        var main = ps.main;
        main.duration        = lifeTime;
        main.startLifetime   = lifeTime;
        main.startSpeed      = startSpeed;
        main.startSize       = startSize;
        main.startColor      = flashColor;
        main.loop            = false;
        main.playOnAwake     = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles    = particleCount;

        // EMISSION
        var emission = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, particleCount)
        });

        // SHAPE – malý kužeľ dopredu
        var shape = ps.shape;
        shape.enabled  = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 18f;
        shape.radius    = 0.01f;

        // Trochu náhodná rotácia a veľkosť (aby nebol každý záblesk identický)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        ));

        var rotOverLifetime = ps.rotationOverLifetime;
        rotOverLifetime.enabled = true;
        rotOverLifetime.z = new ParticleSystem.MinMaxCurve(-30f * Mathf.Deg2Rad, 30f * Mathf.Deg2Rad);

        // Renderer + materiál (aby nebol ružový v URP)
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;

        // Nájsť URP particle shader
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh != null)
        {
            flashMaterial = new Material(sh);
            flashMaterial.SetColor("_BaseColor", flashColor);
            rend.material = flashMaterial;
        }
        else
        {
            Debug.LogWarning("[MuzzleFlashVFX] URP particle shader nenájdený, použijem default material.");
        }
    }

    /// <summary>
    /// Zavolaj pri výstrele.
    /// </summary>
    public void PlayFlash()
    {
        if (ps == null) return;

        // pre istotu reset
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }
}
