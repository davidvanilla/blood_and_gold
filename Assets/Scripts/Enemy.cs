using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("Settings")]
    public float attackRange = 2.0f;
    public float attackCooldown = 1.0f;
    public int attackDamage = 10;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip[] walkClips;

    // --- Anti‑lining‑up fields ---
    [Header("Path Randomization")]
    [SerializeField] private float randomOffsetRadius = 2.0f;
    [SerializeField] private float offsetChangeInterval = 2.0f;
    private Vector3 currentOffset;
    private float nextOffsetTime;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 1.5f;
    [SerializeField] private float separationStrength = 2f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    public int maxHealth = 30;
    private int currentHealth;
    private bool isDead = false;
    private bool isHead = false;
    private Collider hitCollider;
    private Animator animator;
    private Player playerScript;
    private int zombieHandLayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerScript = playerObj.GetComponent<Player>();

        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        zombieHandLayer = LayerMask.NameToLayer("ZombieHand");

        // --- Anti‑lining‑up initialisation ---
        currentOffset = Random.insideUnitSphere * randomOffsetRadius;
        currentOffset.y = 0;
        nextOffsetTime = Time.time + Random.Range(0f, offsetChangeInterval);

        // Stagger the start of chasing
        agent.SetDestination(transform.position); // stay still initially
        Invoke(nameof(StartMoving), Random.Range(0f, 1.5f));

        // Vary attack range (stopping distance) per zombie
        //attackRange = Mathf.Max(1.0f, attackRange + Random.Range(-0.5f, 0.5f));
        
    }

    void StartMoving()
    {
        agent.isStopped = false;
    }

    void Update()
    {
        if (isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (playerScript.GetIsDead())
        {
            agent.isStopped = true;
            return;
        }

        if (distance <= attackRange)
        {
            animator.SetBool("Run", false);
            agent.isStopped = true;

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            animator.SetBool("Run", true);
            agent.isStopped = false;

            // Refresh random offset periodically
            if (Time.time >= nextOffsetTime)
            {
                currentOffset = Random.insideUnitSphere * randomOffsetRadius;
                currentOffset.y = 0;
                nextOffsetTime = Time.time + offsetChangeInterval;
            }

            Vector3 target = player.position + currentOffset;
            agent.SetDestination(target);

            // Separation: avoid clumping with other zombies
            AvoidClumping();
        }

        // Face the player (rotation)
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void AvoidClumping()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 push = Vector3.zero;
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (col.TryGetComponent<Enemy>(out _))
            {
                Vector3 dir = transform.position - col.transform.position;
                float dist = dir.magnitude;
                if (dist < separationRadius)
                    push += dir.normalized * (separationRadius - dist);
            }
        }
        if (push != Vector3.zero)
            agent.velocity += push * separationStrength;
    }

    void Attack()
    {
        // Damage the player
        Player playerScript = player.GetComponent<Player>();
        var isPlayerDead = playerScript.GetIsDead();

        if (!isPlayerDead)
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
                audioSource.PlayOneShot(attackClips[Random.Range(0, attackClips.Length)]);
            }
        }

        if (playerScript != null)
        {
            GameObject mainPlayerObj = GameObject.FindGameObjectWithTag("Player");
            var player = mainPlayerObj.GetComponent<Player>();
        }
        else
        {
            Debug.LogWarning("Player script not found on player GameObject.");
        }
    }

    public void TakeDamage(int damage, Collider collider, Vector3 hitPoint, Vector3 hitNormal)
    {
        currentHealth -= damage;
        hitCollider = collider;
        if (currentHealth <= 0)
        {
            Die(hitPoint, hitNormal);
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    public void TakeDamage(int damage, bool ih)
    {
        isHead = ih;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die1();
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Die(Vector3 hitPoint, Vector3 hitNormal)
    {
        agent.isStopped = true;
        isDead = true;
        GetComponent<Animator>().enabled = false;

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }

        if (hitCollider != null)
        {
            Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddForceAtPosition(hitNormal * 5f, hitPoint, ForceMode.Impulse);
            }
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("zombie_hand"))
                col.enabled = false;
        }
    }

    private void Die1()
    {

        Debug.Log($"is head hit? {isHead}");
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.gameObject.layer == zombieHandLayer)
                col.enabled = false;
        }

        if (isHead)
        {
            animator.SetTrigger("HDead");
           
        }
        else
        {
            animator.SetTrigger("BDead");
            
        }

        agent.isStopped = true;
        isDead = true;
    }
}