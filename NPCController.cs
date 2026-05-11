using UnityEngine;
using UnityEngine.AI; // Fontos, hogy ezt a névteret is használjuk a NavMeshAgenthez!

[RequireComponent(typeof(NavMeshAgent))] // Biztosítja, hogy mindig legyen NavMeshAgent komponens az objektumon
public class NPCController : MonoBehaviour
{
    // === BEÁLLÍTÁSOK AZ INSPECTORBAN ===
    [Header("Célpont és Távolságok")]
   private Transform playerTarget;
    public float detectionRange = 15f;  // Milyen távolságból veszi észre a játékost
    public float stoppingDistance = 2f; // Milyen közel álljon meg a játékoshoz

    // === BELSŐ VÁLTOZÓK ===
    private NavMeshAgent navMeshAgent;  // Referencia a mozgást végző komponensre
    private Animator animator;          // Referencia az animációkat vezérlő komponensre
    private float distanceToPlayer;     // Az aktuális távolság a játékostól

    void Start()
    {
        // Komponensek automatikus megszerzése az induláskor
        navMeshAgent = GetComponent<NavMeshAgent>();
         GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
    if (playerObject != null)
    {
        playerTarget = playerObject.transform;
    }
    else
    {
        Debug.LogError("Nem található 'Player' címkével ellátott objektum a pályán! Kérlek, címkézd fel a játékos prefabot.");
        this.enabled = false;
    }
        if (navMeshAgent.isOnNavMesh)
    {
        navMeshAgent.Warp(transform.position);
    }
        animator = GetComponent<Animator>();

        // Ellenőrizzük, hogy a játékos be van-e állítva
        if (playerTarget == null)
        {
            Debug.LogError("Nincs beállítva a 'playerTarget' az NPCController szkriptben! Kérlek, húzd be a játékos objektumot az Inspectorban.");
            this.enabled = false; // Letiltjuk a szkriptet, hogy ne okozzon hibát
        }
    }

    void Update()
    {
        // Ha nincs célpont, ne csináljunk semmit
        if (playerTarget == null) return;

        // Távolság kiszámítása minden képkockában
        distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // --- A VISELKEDÉS LOGIKÁJA ---

        // 1. HA a játékos az észlelési távolságon belül van, DE a megállási távolságon kívül...
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stoppingDistance)
        {
            // ...akkor mozogjon a játékos felé.
            MoveTowardsPlayer();
            UpdateAnimation(true); // Séta animáció bekapcsolása
        }
        // 2. EGYÉBKÉNT (ha túl messze van VAGY már elég közel van)...
        else
        {
            // ...álljon meg.
            StopMovement();
            UpdateAnimation(false); // Séta animáció kikapcsolása (vissza nyugalmi állapotba)
        }
    }

    void MoveTowardsPlayer()
    {
        // A NavMeshAgent megkapja a célpontot, és automatikusan odanavigál
        navMeshAgent.SetDestination(playerTarget.position);
        navMeshAgent.isStopped = false;
    }

    void StopMovement()
    {
        // Megállítjuk az ügynököt
        navMeshAgent.isStopped = true;
    }

    void UpdateAnimation(bool isWalking)
    {
        // Ha van animator komponens, beállítjuk az 'isWalking' paraméter értékét
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

    // Segítség a vizualizációhoz a Scene nézetben (nem kötelező, de hasznos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Észelelési távolság kirajzolása
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance); // Megállási távolság kirajzolása
    }
}