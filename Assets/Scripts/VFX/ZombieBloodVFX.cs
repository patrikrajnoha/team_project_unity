using UnityEngine;

/// <summary>
/// VFX krvavý sprej zo zombíka.
/// Pripni na root zombíka (tam, kde je ZombieAI).
/// </summary>
public class ZombieBloodVFX : MonoBehaviour
{
    [Header("Blood Settings")]
    public int   particleCount = 25;
    public float lifeTime      = 0.35f;
    public float startSpeed    = 6f;
    public float startSize     = 0.07f;

    // tmavo-červená krv, nie ružová :)
    public Color bloodColor = new Color(0.35f, 0f, 0f, 1f);

    private Material bloodMaterial;

    private void EnsureMaterial()
    {
        if (bloodMaterial != null) return;

        // URP particle shader
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh != null)
        {
            bloodMaterial = new Material(sh);
            bloodMaterial.SetColor("_BaseColor", bloodColor);
        }
        else
        {
            Debug.LogWarning("[ZombieBloodVFX] Nenašiel som URP particle shader, použijem default.");
        }
    }

    public void SpawnBlood(Vector3 position, Vector3 direction)
    {
        GameObject go = new GameObject("BloodVFX");
        go.transform.position = position;
        go.transform.rotation = Quaternion.LookRotation(direction);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        // istota, že systém ešte nehrá, aby sme mohli meniť duration atď.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration        = lifeTime;
        main.startLifetime   = lifeTime;
        main.startSpeed      = startSpeed;
        main.startSize       = startSize;
        main.startColor      = bloodColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop            = false;
        main.playOnAwake     = false;
        main.maxParticles    = particleCount;

        var emission = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });

        var shape = ps.shape;
        shape.enabled  = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 20f;
        shape.radius    = 0.02f;

        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.y       = -5f;   // nech krv trošku padá dole

        // renderer + materiál (kvôli ružovej)
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;

        EnsureMaterial();
        if (bloodMaterial != null)
        {
            rend.material = bloodMaterial;
        }

        ps.Play();

        // cleanup po skončení efektu
        Destroy(go, lifeTime + 0.5f);
    }
}
