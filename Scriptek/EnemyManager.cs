using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Beállítások")]
    public GameObject enemyPrefab;   // Húzd be ide a Farkas Prefabot!
    public string targetSceneName = "The_Viking_Village"; // A pálya neve, ahol spawnolni kell
    public int maxEnemies = 10;      // Maximum ennyi lehet egyszerre
    public float spawnInterval = 10f; // Hány másodpercenként jöjjön új

    [Header("Nehezedés (Scaling)")]
    public float difficultyMultiplier = 1.1f; // Minden új ellenfél ennyiszer erősebb
    private float currentStatMultiplier = 1.0f; // Ezt növeljük folyamatosan

    [Header("Spawn Terület")]
    public Vector3 spawnCenter = Vector3.zero; // A sziget közepe (kb.)
    public float spawnRadius = 20f; // Milyen körben rakja le őket

    private float spawnTimer;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // 1. Csak akkor fusson, ha a megfelelő pályán vagyunk
        if (SceneManager.GetActiveScene().name != targetSceneName) return;

        // 2. Töröljük a listából a már meghalt (eltűnt) ellenfeleket
        // (A 'RemoveAll' egy trükk: minden elemet töröl, ami null)
        activeEnemies.RemoveAll(item => item == null);

        // 3. Időzítő kezelése
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            
            // Csak akkor spawnolunk, ha még nincs tele a pálya
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
            else
            {
                Debug.Log("[EnemyManager] Elértük a max létszámot (10), nem jön új farkas.");
            }
        }
    }

    void SpawnEnemy()
    {
        // 1. Érvényes pozíció keresése a NavMeshen
        Vector3 spawnPos = GetRandomNavMeshPosition();
        if (spawnPos == Vector3.zero) 
        {
            Debug.LogWarning("[EnemyManager] Nem találtam érvényes spawn helyet!");
            return;
        }

        // 2. Létrehozás
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        // 3. Erősítés (Scaling) alkalmazása
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            // Felszorozzuk az értékeket
            controller.maxHealth *= currentStatMultiplier;
            controller.damageAmount *= currentStatMultiplier;
            
            // Opcionális: A méretét is növelhetjük kicsit, hogy látszódjon az erő
            // newEnemy.transform.localScale *= (1 + (currentStatMultiplier - 1) * 0.2f);

            Debug.Log($"[EnemyManager] Új ellenfél létrehozva! Szorzó: {currentStatMultiplier:F2}. HP: {controller.maxHealth}");
        }

        // 4. Hozzáadás a listához és a szorzó növelése a következőhöz
        activeEnemies.Add(newEnemy);
        
        // Növeljük az erőt a következő körre (pl. 1.0 -> 1.1 -> 1.21 -> 1.33...)
        currentStatMultiplier *= difficultyMultiplier;
    }

    Vector3 GetRandomNavMeshPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = spawnCenter + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = 10; // Kicsit magasabbról indítjuk a keresést lefelé
            
            NavMeshHit hit;
            // Megpróbáljuk letenni a NavMeshre
            if (NavMesh.SamplePosition(randomPoint, out hit, 20.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.zero; // Ha nem sikerült
    }
    
    // Debug célból kirajzoljuk a spawn kört
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
    }
}