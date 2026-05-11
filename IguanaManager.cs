using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class IguanaManager : MonoBehaviour
{
    public static IguanaManager Instance { get; private set; }

    [Header("Beállítások")]
    public GameObject iguanaPrefab;   // Húzd be az Iguana Prefabot
    public string targetSceneName = "The_Viking_Village"; 
    public int maxIguanas = 5;        // Kevesebb legyen, mint farkas
    public float spawnInterval = 15f; 

    [Header("Spawn Terület")]
    public Vector3 spawnCenter = Vector3.zero; 
    public float spawnRadius = 30f; 

    private float spawnTimer;
    private List<GameObject> activeIguanas = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != targetSceneName) return;

        // Lista takarítása
        activeIguanas.RemoveAll(item => item == null);

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            if (activeIguanas.Count < maxIguanas)
            {
                SpawnIguana();
            }
        }
    }

    void SpawnIguana()
    {
        Vector3 spawnPos = GetRandomNavMeshPosition();
        if (spawnPos == Vector3.zero) return;

        GameObject newIguana = Instantiate(iguanaPrefab, spawnPos, Quaternion.identity);
        activeIguanas.Add(newIguana);
        
        Debug.Log("[IguanaManager] Új iguána született.");
    }

    Vector3 GetRandomNavMeshPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = spawnCenter + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = 10; 
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 20.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.zero;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
    }
}