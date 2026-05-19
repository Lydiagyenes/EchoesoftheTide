using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(IguanaCharacter))] // Ez biztosítja, hogy ott legyen a másik script is
public class IguanaController : MonoBehaviour
{
    [Header("Mozgás")]
    public float runSpeed = 6f;
    public float wanderRadius = 15f;
    public float minRestTime = 3f;
    public float maxRestTime = 8f;

    [Header("Mérgezés")]
    public float poisonRange = 2.0f;
    public float poisonDamagePerTick = 1f;
    public float poisonDuration = 25f;
    public float poisonCooldown = 5f;

    [Header("Élet")]
    public float maxHealth = 20f;
    private float currentHealth;

    // REFERENCIÁK
    private NavMeshAgent agent;
    private IguanaCharacter character; // A te meglévő scripted!
    private Transform playerTarget;
    
    private bool isResting = false;
    private bool isDead = false;
    private float lastPoisonTime;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        character = GetComponent<IguanaCharacter>(); // Bekötjük a te scriptedet
        
        agent.speed = runSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        // Azonnal elindulunk
        SetRandomDestination();
    }

    void Update()
    {
        if (isDead) return;

        // 1. MÉRGEZÉS ELLENŐRZÉS
        CheckPoisonAttack();

        // 2. MOZGÁS LOGIKA
        // Ha épp nem pihenünk, és odaértünk a célhoz
        if (!isResting && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(RestRoutine());
        }

        // 3. ANIMÁCIÓ SZINKRONIZÁLÁS (A te scripteddel!)
        // A "v" (vertical/forward) paramétert a sebesség alapján állítjuk
        // A "h" (horizontal/turn) paramétert 0-ra hagyjuk, mert a NavMesh forgatja a modellt
        float speedPercent = agent.velocity.magnitude / agent.speed;
        character.Move(speedPercent, 0f);
    }

    void CheckPoisonAttack()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= poisonRange && Time.time >= lastPoisonTime + poisonCooldown)
        {
            lastPoisonTime = Time.time;
            
            // A te scripted hívása:
            character.Attack(); 

            // Mérgezés a játékosra
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ApplyPoison(poisonDamagePerTick, poisonDuration);
                Debug.Log("[Iguana] Megcsípett! Mérgezés!");
            }

            // Ha megcsípett, fusson el máshova (ne álljon ott)
            if (isResting)
            {
                StopCoroutine("RestRoutine");
                isResting = false;
            }
            SetRandomDestination();
        }
    }

    IEnumerator RestRoutine()
    {
        isResting = true;
        agent.isStopped = true;
        
        // Mivel állunk, a sebesség 0 lesz, az Update-ben a Move(0,0) lefut magától -> Idle animáció

        float restTime = Random.Range(minRestTime, maxRestTime);
        yield return new WaitForSeconds(restTime);

        isResting = false;
        SetRandomDestination();
    }

    void SetRandomDestination()
    {
        agent.isStopped = false;
        
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    // --- SEBZÉS KAPÁS (A fegyvered ezt hívja) ---
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        
        // A te scripted hívása fájdalomra:
        character.Hit();

        // Ha megütik, fusson el azonnal
        if (isResting)
        {
            StopCoroutine("RestRoutine");
            isResting = false;
            SetRandomDestination();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        agent.enabled = false; // Megállítjuk a NavMesh-t
        
        // Fizikai colliderek kikapcsolása vagy triggerre állítása lootoláshoz
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.isTrigger = true; // Hogy át lehessen rajta sétálni
        }

        // A te scripted hívása halálra:
        character.Death();

        // Loot engedélyezése (ha van rajta)
        LootableBody lootScript = GetComponent<LootableBody>();
        if (lootScript != null) lootScript.enabled = true;

        // Eltüntetés időzítve
        Destroy(gameObject, 60f);
        this.enabled = false; // Kikapcsoljuk ezt az AI-t
    }
}