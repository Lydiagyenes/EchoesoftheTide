using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Állapot")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Mozgás")]
    public float detectionRadius = 15f;
    public float patrolRadius = 20f;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Harc Beállítások")]
    public float attackRadius = 2.5f;   
    public float damageAmount = 20f;    
    public float attackCooldown = 3.0f; 
    public float impactTime = 0.5f;     

    [Header("Sérülés Reakció")]
    public float stunDuration = 1.0f;

    private float lastAttackTime;
    private bool isDead = false;
    private bool isAttacking = false; 
    private bool isStunned = false;
    private Vector3 startPosition;

    private Coroutine attackCoroutine; 

    [Header("Referenciák")]
    public Animator animator;
    private NavMeshAgent agent;
    private Transform playerTarget;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }

        SetRandomPatrolPoint();
    }

    void Update()
    {
        // 1. SZIGORÚ KILÉPÉS: Ha halott, egy lépést se tovább!
        if (isDead) return;

        // 2. Ha bénult, vagy épp támad, nem döntünk újra
        if (isStunned || isAttacking) return;

        // 3. Ha nincs NavMeshAgent (hiba elkerülése) vagy Player
        if (!agent.isOnNavMesh || playerTarget == null || PlayerStats.Instance == null || PlayerStats.Instance.currentHealth <= 0)
        {
            if(agent.isOnNavMesh) PatrolBehavior();
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackRadius)
        {
            AttackBehavior();
        }
        else if (distance <= detectionRadius)
        {
            ChaseBehavior();
        }
        else
        {
            PatrolBehavior();
        }
    }

    // --- MOZGÁS ---

    void PatrolBehavior()
    {
        if (!agent.isOnNavMesh) return; // Biztonsági csekk

        agent.speed = walkSpeed;
        agent.isStopped = false;
        if (animator != null) animator.SetBool("isRunning", false);

        if (!agent.pathPending && agent.remainingDistance < 0.5f) SetRandomPatrolPoint();
    }

    void ChaseBehavior()
    {
        if (!agent.isOnNavMesh) return;

        agent.speed = runSpeed;
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);
        if (animator != null) animator.SetBool("isRunning", true);
    }
    
    void SetRandomPatrolPoint()
    {
        for (int i = 0; i < 10; i++) 
        {
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;
            randomPoint.y = transform.position.y; 
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 20.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return; 
            }
        }
        if(agent.isOnNavMesh) agent.SetDestination(startPosition);
    }

    // --- HARC ---

    void AttackBehavior()
    {
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
        RotateTowards(playerTarget.position);
        if (animator != null) animator.SetBool("isRunning", false);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            attackCoroutine = StartCoroutine(PerformAttackRoutine());
        }
    }

    IEnumerator PerformAttackRoutine()
    {
        isAttacking = true; 
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger("Attack");

        // Várakozás a harapásig (impactTime)
        // Ciklusban várunk, hogy közben ellenőrizhessük, él-e még
        float timer = 0f;
        while(timer < impactTime)
        {
            timer += Time.deltaTime;
            if(isDead || isStunned) 
            {
                // Ha várakozás közben meghalt vagy megütötték -> MEGSZAKÍTÁS
                isAttacking = false;
                yield break; 
            }
            yield return null;
        }

        // --- SEBZÉS PILLANATA ---
        
        // Még egyszer ellenőrizzük: biztos él? biztos nem bénult?
        if (!isDead && !isStunned && playerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            // Kicsit elnézőbbek vagyunk a távolsággal (1.2 méter ráhagyás)
            if (distance <= attackRadius + 1.2f) 
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.TakeDamage(damageAmount);
                    Debug.Log("[Enemy] Harapás sikeres!");
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false; 
        attackCoroutine = null;
    }

    // --- SÉRÜLÉS ÉS HALÁL ---

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        Debug.Log($"[Enemy] FÁJDALOM! Maradék HP: {currentHealth}");

        // STUN LOGIKA
        // Ha épp támadott, azt azonnal leállítjuk
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        
        // Minden támadási flaget törlünk
        isAttacking = false;
        
        // Elindítjuk a bénulást
        StartCoroutine(StunRoutine());

        // Reseteljük a támadást, hogy a stun után ne harapjon azonnal
        lastAttackTime = Time.time + stunDuration; 

        if (currentHealth <= 0) Die();
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        
        if (agent.isOnNavMesh) agent.isStopped = true;
        
        if (animator != null)
        {
            animator.SetTrigger("GetHit");
            animator.SetBool("isRunning", false);
        }

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        // Az Update majd újra elindítja a mozgást
    }

   void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines(); 

        if(agent.isOnNavMesh) agent.isStopped = true;
        agent.enabled = false;
        
        if (animator != null) animator.SetTrigger("Die");
        
        Debug.Log("[Enemy] A farkas elpusztult. Zsákmányolható!");

        // --- JAVÍTOTT RÉSZ ---
        foreach (var col in GetComponentsInChildren<Collider>()) 
        {
            col.enabled = true;

            // Ha ez egy MeshCollider, kötelező Convex-re állítani, mielőtt Trigger lesz!
            if (col is MeshCollider meshCol)
            {
                meshCol.convex = true;
            }

            col.isTrigger = true; 
        }
        // ---------------------

        LootableBody lootScript = GetComponent<LootableBody>();
        if (lootScript != null) lootScript.enabled = true;

        this.enabled = false;
        Destroy(gameObject, 300f); 
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}