using UnityEngine;
using System.Collections;

public class ButterflyAI : MonoBehaviour
{
    [Header("Beállítások")]
    public float flySpeed = 3f;
    public float wanderRadius = 15f; // Ilyen messzire mehet el a start ponttól
    public float restTime = 3f;      // Mennyi ideig pihenjen
    
    [Header("Játékos Interakció")]
    public float detectPlayerRadius = 5f; // Milyen közelről veszi észre a játékost
    public float damageInterval = 1f;     // Milyen gyakran sebez

    [Header("Rétegek")]
    public LayerMask landableLayers; // Fák, kövek, föld (Default, Terrain)
    
    private Vector3 startPosition;
    private Vector3 currentTarget;
    // private bool isResting = false;
    private bool isOnPlayer = false;
    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private Animator animator;

    // Állapotok
    private enum State { Flying, Landing, Resting, Attacking }
    private State currentState = State.Flying;

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        PickNewTarget();

        // Megkeressük a játékost
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Flying:
                HandleFlying();
                CheckForPlayer();
                break;
            case State.Landing:
                // A Coroutine kezeli
                break;
            case State.Resting:
                // Csak várunk, de ha a játékos közel jön, felrebbenünk
                if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) < 2f)
                {
                    Scare();
                }
                break;
            case State.Attacking:
                HandleAttacking();
                break;
        }
    }

    // --- REPÜLÉS ---
    void HandleFlying()
    {
        // Mozgás a célpont felé
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, flySpeed * Time.deltaTime);
        
        // Forgás a célpont felé
        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);
        }

        // Ha odaértünk
        if (Vector3.Distance(transform.position, currentTarget) < 0.5f)
        {
            // Vagy pihenünk egyet, vagy új pontot választunk
            if (Random.value > 0.5f) 
            {
                TryToLand();
            }
            else
            {
                PickNewTarget();
            }
        }
    }

    void PickNewTarget()
    {
        // Random pont a levegőben
        Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
        randomPoint.y = Mathf.Max(randomPoint.y, startPosition.y + 1f); // Ne menjen a föld alá
        currentTarget = randomPoint;
        
        if (animator) animator.SetBool("isFlying", true);
        currentState = State.Flying;
    }

    // --- LESZÁLLÁS (Környezetre) ---
    void TryToLand()
    {
        // Lefelé lövünk egy sugarat, hogy találunk-e fát/követ
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3f, landableLayers))
        {
            StartCoroutine(LandRoutine(hit.point, hit.normal, null));
        }
        else
        {
            PickNewTarget(); // Ha nincs alattunk semmi, repülünk tovább
        }
    }

    // --- TÁMADÁS (Játékosra szállás) ---
    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        
        // Ha közel van, és NEM mozog
        if (dist < detectPlayerRadius)
        {
            // Ellenőrizzük a PlayerMovement-et, hogy mozog-e
            // (A te limitedben az 'Input.GetAxis' vagy animáció alapján lehetne tudni, 
            // de egyszerűbb, ha megnézzük a pozíció változását vagy a scriptet)
            bool playerIsMoving = false;
            if (playerMovement != null)
            {
                 // Feltételezzük, hogy az animátorból ki tudjuk olvasni, vagy a velocityből
                 // Egyszerűsítve: ha a karakterevezérlő sebessége > 0.1
                 var cc = playerTransform.GetComponent<CharacterController>();
                 if (cc != null && cc.velocity.magnitude > 0.1f) playerIsMoving = true;
            }

            if (!playerIsMoving)
            {
                currentState = State.Attacking;
                // A vállára célzunk (kb 1.5 magas)
                currentTarget = playerTransform.position + Vector3.up * 1.5f + playerTransform.right * 0.3f;
            }
        }
    }

    void HandleAttacking()
    {
        // Repülés a játékos felé
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, flySpeed * 1.5f * Time.deltaTime);
        transform.LookAt(currentTarget);

        // Ha a játékos megmozdul közben, megijedünk
        var cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null && cc.velocity.magnitude > 0.1f)
        {
            Scare();
            return;
        }

        // Ha odaértünk (rászálltunk)
        if (Vector3.Distance(transform.position, currentTarget) < 0.1f && !isOnPlayer)
        {
            // Leszállás a játékosra (szülővé tesszük, hogy vele mozogjon)
            StartCoroutine(LandOnPlayerRoutine());
        }
    }

    IEnumerator LandOnPlayerRoutine()
    {
        isOnPlayer = true;
        if (animator) animator.SetBool("isFlying", false);
        transform.SetParent(playerTransform); // Rátapadunk
        
        // Sebzés ciklus
        while (isOnPlayer)
        {
            // Ha a játékos megmozdul, lerepülünk
            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null && cc.velocity.magnitude > 0.1f)
            {
                Scare();
                yield break;
            }

            // Sebzés
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.TakeDamage(1);
                // Opcionális: Pici vér effekt vagy hang
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    // --- KÖZÖS RUTINOK ---

    IEnumerator LandRoutine(Vector3 pos, Vector3 normal, Transform parent)
    {
        currentState = State.Landing;
        float duration = 1f;
        float timer = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, normal); // Igazodás a felülethez

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.position = Vector3.Lerp(startPos, pos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        if (animator) animator.SetBool("isFlying", false);
        currentState = State.Resting;
        
        yield return new WaitForSeconds(restTime);
        
        Scare(); // Pihenés után továbbáll
    }

    void Scare()
    {
        isOnPlayer = false;
        transform.SetParent(null); // Leválás a játékosról/fáról
        currentState = State.Flying;
        PickNewTarget();
    }
}