using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AI zombíka: chôdza, útoky, damage, smrť + headshot + drop mozgu.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class ZombieAI : MonoBehaviour, IDamageable
{
    // ---------------------- REFERENCES & AI ----------------------
    [Header("References")]
    [Tooltip("Player Transform. Ak je null, v Start sa nájde objekt s tagom 'Player'.")]
    public Transform player;

    [Header("Ranges")]
    public float sightRange  = 15f;
    public float attackRange = 2.5f;
    public float biteRange   = 1.2f;

    [Header("Movement")]
    public float walkSpeed = 1.2f;

    [Header("Attack settings")]
    public float timeBetweenAttacks = 1.5f;
    public float attackHitDelay     = 0.5f;
    public int   lightAttackDamage  = 15;
    public int   hardAttackDamage   = 25;
    public int   biteDamage         = 35;

    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;

    private bool alreadyAttacked;

    // animator parametre
    private static readonly int HashSpeed       = Animator.StringToHash("Speed");
    private static readonly int HashLightAttack = Animator.StringToHash("LightAttack");
    private static readonly int HashHardAttack  = Animator.StringToHash("HardAttack");
    private static readonly int HashBiteAttack  = Animator.StringToHash("BiteAttack");
    private static readonly int HashLightHit    = Animator.StringToHash("LightHit");
    private static readonly int HashHardHit     = Animator.StringToHash("HardHit");
    private static readonly int HashDie         = Animator.StringToHash("Die");

    // pomocná property pre bezpečné používanie NavMeshAgentu
    private bool HasAgentOnNavMesh =>
        agent != null && agent.enabled && agent.isOnNavMesh;

    // ---------------------- HEALTH & DEATH ----------------------
    [Header("Health")]
    public int maxHealth     = 100;
    public int currentHealth = 0;

    [Header("Hit reactions")]
    public int hardHitThreshold = 30;

    [Header("Death")]
    public float destroyDelay = 10f;

    public bool IsDead { get; private set; }

    // ---------------------- BRAIN DROP ----------------------
    [Header("Brain Drop")]
    public GameObject brainPrefab;
    public float      brainForce = 4f;

    private bool brainDropped = false;

    private void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb       = GetComponent<Rigidbody>();

        if (agent != null)
        {
            agent.speed            = walkSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.updateRotation   = true;
            agent.updatePosition   = true;
        }

        // zaživa: žiadna fyzika, iba NavMesh
        if (rb != null)
        {
            rb.isKinematic              = true;
            rb.useGravity               = false;
            rb.interpolation            = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode   = CollisionDetectionMode.Continuous;
        }

        currentHealth = maxHealth;
        IsDead        = false;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    private void Update()
    {
        if (IsDead)
            return;

        if (!HasAgentOnNavMesh)
            return;

        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else if (distance <= sightRange)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
        }

        // rýchlosť pre blend Idle/Walk
        float speed = agent.velocity.magnitude;
        animator.SetFloat(HashSpeed, speed);
    }

    // ---------------------- AI – pohyb a útok ----------------------
    private void StopAgent()
    {
        if (!HasAgentOnNavMesh) return;

        agent.isStopped = true;
        agent.velocity  = Vector3.zero;
    }

    private void ChasePlayer()
    {
        if (!HasAgentOnNavMesh) return;
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void Idle()
    {
        StopAgent();
    }

    private void AttackPlayer()
    {
        if (!HasAgentOnNavMesh) return;
        if (player == null) return;
        if (alreadyAttacked) return;

        // otočenie na hráča
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        float distance = Vector3.Distance(transform.position, player.position);

        int damage;
        int triggerHash;

        if (distance <= biteRange)
        {
            triggerHash = HashBiteAttack;
            damage      = biteDamage;
        }
        else
        {
            if (Random.value < 0.5f)
            {
                triggerHash = HashLightAttack;
                damage      = lightAttackDamage;
            }
            else
            {
                triggerHash = HashHardAttack;
                damage      = hardAttackDamage;
            }
        }

        animator.SetTrigger(triggerHash);
        StartCoroutine(DealDamageAfterDelay(damage, attackHitDelay));

        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private IEnumerator DealDamageAfterDelay(int damage, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (IsDead) yield break;
        if (player == null) yield break;
        if (!HasAgentOnNavMesh) yield break;

        // hráč ušiel mimo range – netrafíme
        if (Vector3.Distance(transform.position, player.position) > attackRange + 0.5f)
            yield break;

        var damageable = player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;

        if (HasAgentOnNavMesh)
            agent.isStopped = false;
    }

    // ---------------------- DAMAGE / HEALTH ----------------------
    public void TakeDamage(float amount)
    {
        TakeDamageInternal(Mathf.RoundToInt(amount), false);
    }

    private void TakeDamageInternal(int damage, bool headshot)
    {
        if (IsDead) return;

        if (headshot)
        {
            Debug.Log($"[ZombAI] HEADSHOT -> {name}", this);
            currentHealth = 0;
            Die(true);
            return;
        }

        if (damage <= 0) return;

        currentHealth -= damage;
        Debug.Log($"[ZombAI] dmg={damage}, hp={currentHealth}/{maxHealth}", this);

        if (currentHealth > 0)
        {
            if (animator != null)
            {
                if (damage >= hardHitThreshold)
                    animator.SetTrigger(HashHardHit);
                else
                    animator.SetTrigger(HashLightHit);
            }
        }
        else
        {
            currentHealth = 0;
            Die(false);
        }
    }

    public void HandleHeadshot(int damage, Vector3 hitPoint)
    {
        Debug.Log($"[ZombAI] HandleHeadshot @ {hitPoint}", this);

        TakeDamageInternal(damage, true);
        DropBrain(hitPoint);
    }

    // ---------------------- BRAIN DROP ----------------------
    private void DropBrain(Vector3 hitPoint)
    {
        if (brainDropped) return;
        brainDropped = true;

        if (brainPrefab == null)
        {
            Debug.LogWarning("[ZombAI] Brain prefab NULL", this);
            return;
        }

        GameObject brain = Instantiate(brainPrefab, hitPoint, Quaternion.identity);

        Rigidbody brainRb = brain.GetComponent<Rigidbody>();
        if (brainRb != null)
        {
            Vector3 randomDir = (Vector3.up + Random.insideUnitSphere * 0.4f).normalized;
            brainRb.AddForce(randomDir * brainForce, ForceMode.Impulse);
        }

        Debug.Log("[ZombAI] Brain dropped", this);
    }

    // ---------------------- DEATH ----------------------
    private void Die(bool headshot)
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"[ZombAI] Die(headshot={headshot}) -> {name}", this);

        // vypnutie NavMeshAgentu
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity  = Vector3.zero;
            }
            agent.enabled = false;
        }

        // fyzika
        if (rb != null)
        {
            rb.isKinematic            = false;
            rb.useGravity             = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.speed          = 1f;

            if (headshot)
            {
                // priamo prehraj stav "Death Headshot" na Base Layeri
                int deathHeadshotHash = Animator.StringToHash("Base Layer.Death Headshot");

                if (animator.HasState(0, deathHeadshotHash))
                {
                    animator.Play(deathHeadshotHash, 0, 0f);
                }
                else
                {
                    Debug.LogWarning("[ZombAI] Nenájdený stav 'Base Layer.Death Headshot' v Animatori!", this);
                }
            }
            else
            {
                animator.SetTrigger(HashDie);
            }
        }

        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
    }
}
