using UnityEngine;

/// <summary>
/// Zelený „nukleárny“ dym pre portál, nakonfigurovaný iba raz v kóde.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PortalVFX : MonoBehaviour
{
    [Header("Základné nastavenia")]
    [Tooltip("Polomer portálu (šírka dymu).")]
    public float radius = 0.6f;

    [Tooltip("Výška stĺpca dymu nad portálom.")]
    public float height = 2.0f;

    [Tooltip("Intenzita emisie (koľko častíc za sekundu).")]
    public float emissionRate = 35f;

    [Tooltip("Farba dymu (nukleárna zelená).")]
    public Color smokeColor = new Color(0.3f, 1.0f, 0.3f, 1.0f);

    private ParticleSystem ps;

    // ZDIEĽANÝ materiál pre všetky portály
    private static Material sharedParticleMaterial;

    private void Awake()
    {
        EnsureParticleSystem();
        ConfigureParticleSystem();
    }

    private void Start()
    {
        if (ps != null && !ps.isPlaying)
        {
            ps.Play();
        }
    }

    [ContextMenu("Refresh VFX")]
    private void RefreshVFX()
    {
        EnsureParticleSystem();
        ConfigureParticleSystem();
    }

    private void EnsureParticleSystem()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
            ps = gameObject.AddComponent<ParticleSystem>();

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        if (sharedParticleMaterial == null)
        {
            Shader shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
                shader = Shader.Find("Particles/Standard Surface");

            sharedParticleMaterial = new Material(shader);
            sharedParticleMaterial.SetFloat("_Mode", 2); // transparent
        }

        renderer.sharedMaterial = sharedParticleMaterial;
    }

    private void ConfigureParticleSystem()
    {
        if (ps == null) return;

        // MAIN
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f); // malé kúsočky
        main.startRotation = 0f;
        main.gravityModifier = -0.05f; // jemne stúpa hore
        main.maxParticles = 500;
        main.startColor = smokeColor;

        if (sharedParticleMaterial != null)
        {
            sharedParticleMaterial.SetColor("_Color", smokeColor);
            if (sharedParticleMaterial.HasProperty("_EmissionColor"))
                sharedParticleMaterial.SetColor("_EmissionColor", smokeColor * 2f);
        }

        // EMISSION
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = emissionRate;

        // SHAPE – „stĺpec“ nad portálom
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.ConeVolume;
        shape.radius = radius;
        shape.angle = 15f;
        shape.length = height;
        shape.position = Vector3.zero;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        // COLOR OVER LIFETIME
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(smokeColor, 0.0f),
                new GradientColorKey(new Color(0.2f, 0.9f, 0.2f, 1f), 0.4f),
                new GradientColorKey(new Color(0.1f, 0.6f, 0.1f, 1f), 1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(0.8f, 0.2f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);

        // NOISE – turbulencia
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.4f;
        noise.scrollSpeed = 0.5f;
        noise.damping = true;

        // === DÔLEŽITÉ: úplne vypneme velocity moduly ===
        var vel = ps.velocityOverLifetime;
        vel.enabled = false;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.y = new ParticleSystem.MinMaxCurve(0f);
        vel.z = new ParticleSystem.MinMaxCurve(0f);

        var limitVel = ps.limitVelocityOverLifetime;
        limitVel.enabled = false;
        limitVel.dampen = 0f;
        limitVel.limitX = new ParticleSystem.MinMaxCurve(0f);
        limitVel.limitY = new ParticleSystem.MinMaxCurve(0f);
        limitVel.limitZ = new ParticleSystem.MinMaxCurve(0f);

        ps.Clear();
    }
}
