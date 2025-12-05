using System.Collections;
using UnityEngine;

/// <summary>
/// AI zombíka: chôdza za hráčom, útoky, damage, smrť + headshot + drop mozgu.
/// Verzia bez NavMeshAgent – pohyb cez Rigidbody, aby kolidoval s prekážkami.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class ZombieAI : MonoBehaviour, IDamageable
{
    // ---------------------- REFERENCES & AI ----------------------
    [Header("References")]
    [Tooltip("Player Transform. Ak je null, v Start sa nájde objekt s tagom 'Player'.")]
    public Transform player;

    [Header("Ranges")]
    public float sightRange  = 15f;   // vzdialenosť, kde si všimne hráča
    public float attackRange = 2.5f;  // vzdialenosť, kde začne útok
    public float biteRange   = 1.2f;  // blízky útok (uhryznutie)

    [Header("Movement")]
    public float walkSpeed = 1.2f;

    [Header("Attack settings")]
    public float timeBetweenAttacks = 1.5f;
    public float attackHitDelay     = 0.5f;
    public int   lightAttackDamage  = 15;
    public int   hardAttackDamage   = 25;
    public int   biteDamage         = 35;

    private Animator animator;
    private Rigidbody rb;

    private bool alreadyAttacked;
    private Vector3 moveDirection = Vector3.zero;

    // animator parametre
    private static readonly int HashSpeed       = Animator.StringToHash("Speed");
    private static readonly int HashLightAttack = Animator.StringToHash("LightAttack");
    private static readonly int HashHardAttack  = Animator.StringToHash("HardAttack");
    private static readonly int HashBiteAttack  = Animator.StringToHash("BiteAttack");
    private static readonly int HashLightHit    = Animator.StringToHash("LightHit");
    private static readonly int HashHardHit     = Animator.StringToHash("HardHit");
    private static readonly int HashDie         = Animator.StringToHash("Die");

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

    // -----------------------------------------------------------
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb       = GetComponent<Rigidbody>();

        // Zombík je klasický dynamický Rigidbody – koliduje s prekážkami
        rb.isKinematic            = false;
        rb.useGravity             = true;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

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
        if (IsDead) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // pri útoku sa prestane hýbať
            moveDirection = Vector3.zero;
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

        // rýchlosť pre animáciu – podľa smeru, nie fyziky
        float targetSpeed = moveDirection.magnitude * walkSpeed;
        float currentSpeed = animator.GetFloat(HashSpeed);
        float speed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 8f); // trochu vyhladené
        animator.SetFloat(HashSpeed, speed);

    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Vector3 newPos = rb.position + moveDirection * walkSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    // ---------------------- AI – pohyb a útok ----------------------
    private void ChasePlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir.Normalize();
            moveDirection = dir;

            // otočenie k hráčovi
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void Idle()
    {
        moveDirection = Vector3.zero;
    }

    private void AttackPlayer()
    {
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

        // zastav pohyb
        moveDirection      = Vector3.zero;
        rb.linearVelocity        = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.speed          = 1f;

            if (headshot)
            {
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
