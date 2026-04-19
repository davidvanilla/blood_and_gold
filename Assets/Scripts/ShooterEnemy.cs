using UnityEngine;
using UnityEngine.AI;

public class ShooterEnemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    public Transform player;
    public Transform gunMuzzle;           // Where bullets come out
    public GameObject hitEffectPrefab;    // Optional impact VFX
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private Light _muzzleLight;
    private NavMeshAgent agent;
    private Animator animator;
    private Player playerHealth;           // Player's health script

    [Header("Combat")]
    public float attackRange = 15f;
    public float attackCooldown = 1.5f;    // Seconds between shots
    public float damagePerShot = 15;
    public float accuracySpread = 5f;      // Degrees of random spread
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("Movement")]
    public float stoppingDistance = 10f;    // Stop chasing when this close
    public float updatePathInterval = 0.5f;

    // Ammo (optional, set to -1 for infinite)
    public int magazineSize = 30;
    public int carriedAmmo = 90;
    private int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;
    private float nextAttackTime = 0f;
    private float nextPathUpdate = 0f;


    [Header("Path Randomization")]
    [SerializeField] private float randomOffsetRadius = 2.5f;   
    [SerializeField] private float offsetChangeInterval = 2f;  
    private Vector3 currentOffset;
    private float nextOffsetTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<Player>();

        currentHealth = maxHealth;
        currentAmmo = magazineSize;

        agent.stoppingDistance = stoppingDistance + Random.Range(-1f, 1f);
        agent.updateRotation = false;      // We rotate manually for aiming
        animator.SetBool("HasRiffle", true);

        nextPathUpdate = Time.time + Random.Range(0f, 1.5f);
        currentOffset = Random.insideUnitSphere * randomOffsetRadius;
        currentOffset.y = 0;
        nextOffsetTime = Time.time + Random.Range(0f, offsetChangeInterval);

    }

    void Update()
    {
        if (isDead || playerHealth == null || playerHealth.GetIsDead())
        {
            agent.isStopped = true;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        //Debug.Log($"Distance: {distance}, AttackRange: {attackRange}, CanSee: {HasLineOfSight()}, Ammo: {currentAmmo}, Reloading: {isReloading}");
        // --- Movement ---
        if (distance > stoppingDistance && !isReloading)
        {
            agent.isStopped = false;
            if (Time.time >= nextPathUpdate)
            {
                // Refresh random offset periodically
                if (Time.time >= nextOffsetTime)
                {
                    currentOffset = Random.insideUnitSphere * randomOffsetRadius;
                    currentOffset.y = 0;
                    nextOffsetTime = Time.time + offsetChangeInterval;
                }

                Vector3 target = player.position + currentOffset;
                agent.SetDestination(target);
                nextPathUpdate = Time.time + updatePathInterval;
            }

            AvoidClumping();
            animator?.SetBool("Aim", false);
            animator.SetBool("Sprint", true);
            animator.SetFloat("Walk", 0.5f);
            StopShoot();
            
        }
        else
        {
            
            agent.isStopped = true;
            animator.SetBool("Sprint", false);
            animator.SetFloat("Walk", 0f);
            animator?.SetBool("Aim", true);
        }

        // --- Face the player (aiming) ---
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
        
        // --- Attack logic ---
        if (!isReloading && distance <= attackRange && Time.time >= nextAttackTime)
        {
            if (HasLineOfSight())
            {
                if (currentAmmo > 0)
                {
                    Shoot();
                    nextAttackTime = Time.time + attackCooldown;
                }
                else
                {
                    // Out of ammo – reload
                    StartCoroutine(ReloadCoroutine());
                    StopShoot();
                }
            }
        }
    }

    void StopShoot()
    {
        _muzzleLight.enabled = false;
        _muzzleFlash.Stop();
    }

    bool HasLineOfSight()
    {
        Vector3 start = gunMuzzle.position;
        Vector3 end = player.position + Vector3.up * 0.5f; // Aim at chest/head
        int playerGunLayer = LayerMask.NameToLayer("PlayerGun");
        int layerMask = ~((1 << playerGunLayer) | (1 << gameObject.layer));
        if (Physics.Linecast(start, end, out RaycastHit hit, layerMask))
        {
            Debug.DrawLine(start, hit.point, Color.red, 0.5f);
            return hit.collider.CompareTag("Player");
        }
          Debug.DrawLine(start, end, Color.green, 0.5f);
        return false;
    }

    void Shoot()
    {
        // Play shoot animation and sound
        animator?.SetBool("Aim", true);
        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound);
        
        // Muzzle flash
        if (_muzzleFlash)
            _muzzleFlash.Play();
        if (_muzzleLight)
            _muzzleLight.enabled = true;

        // Calculate spread
        Vector3 aimDir = (player.position + Vector3.up * 0.3f - gunMuzzle.position).normalized;
        float spreadX = Random.Range(-accuracySpread, accuracySpread);
        float spreadY = Random.Range(-accuracySpread, accuracySpread);
        Quaternion spread = Quaternion.Euler(spreadX, spreadY, 0);
        Vector3 finalDir = spread * aimDir;

        Debug.DrawRay(gunMuzzle.position, finalDir * 200f, Color.red, 2f); // visible for 2 seconds
        int playerGunLayer = LayerMask.NameToLayer("PlayerGun");
        int layerMask = ~((1 << playerGunLayer) | (1 << gameObject.layer));
        // Raycast
        if (Physics.Raycast(gunMuzzle.position, finalDir, out RaycastHit hit, 200f, layerMask))
        {
            // Impact effect
            if (hitEffectPrefab)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

            // Damage player
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player hit by shooter enemy!");
                // Use existing TakeDamage method in Player (adjust as needed)
                playerHealth.TakeGunDamage(); // This deals 5 damage. Better to add a generic TakeDamage(int amount)
                // Or add a new method: playerHealth.TakeDamage(damagePerShot);
            }
            else
            {
                Debug.Log("RAY MISSED: No hit detected.");
                // Optionally draw a green ray for the perfect aim for comparison
                Debug.DrawRay(gunMuzzle.position, aimDir * 200f, Color.green, 2f);
            }
        }

        currentAmmo--;
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        agent.isStopped = true;
        animator?.SetBool("Reload", true);
        yield return new WaitForSeconds(reloadTime);
        animator?.SetBool("Reload", false);

        int needed = magazineSize - currentAmmo;
        int transfer = Mathf.Min(needed, carriedAmmo);
        currentAmmo += transfer;
        carriedAmmo -= transfer;
        isReloading = false;
    }

    public void TakeDamage(int damage, bool iH)
    {

        if (isDead) return;
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage, current health: {currentHealth}");
        if (audioSource && hurtSound) audioSource.PlayOneShot(hurtSound);
       // animator.SetTrigger("HitBullet");


        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        animator?.SetTrigger("Die1");
        isDead = true;
        agent.isStopped = true;
       _muzzleLight.enabled = false;
        _muzzleFlash.Stop();
        if (audioSource && deathSound) audioSource.PlayOneShot(deathSound);

        GameEvents.TriggerZombieDied();
        // Ragdoll (optional)
        //Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        //foreach (Rigidbody rb in rbs) rb.isKinematic = false;
        //GetComponent<Collider>().enabled = false;

        //Destroy(gameObject, 5f);
    }

    // Visual helpers
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

    void AvoidClumping()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, 2.5f);
        Vector3 push = Vector3.zero;
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (col.TryGetComponent<ShooterEnemy>(out _))
            {
                Vector3 dir = transform.position - col.transform.position;
                float dist = dir.magnitude;
                if (dist < 2f)
                    push += dir.normalized * (2f - dist);
            }
        }
        if (push != Vector3.zero)
            agent.velocity += push * 2f;
    }
}