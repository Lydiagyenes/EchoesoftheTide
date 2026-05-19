using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ButterflyManager : MonoBehaviour
{
    [Header("Beállítások")]
    public GameObject butterflyPrefab;
    public int butterflyCount = 20;
    public string targetSceneName = "The_Viking_Village";
    
    // Hol spawnoljanak? (A sziget közepe és mérete)
    public Vector3 islandCenter = new Vector3(0, 10, 0);
    public Vector3 spawnAreaSize = new Vector3(100, 20, 100);

    private List<GameObject> activeButterflies = new List<GameObject>();

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ha a szigetre értünk, spawnolunk
        if (scene.name == targetSceneName)
        {
            SpawnButterflies();
        }
        else
        {
            // Ha elmentünk a szigetről (pl. kabin), töröljük őket
            ClearButterflies();
        }
    }

    void SpawnButterflies()
    {
        ClearButterflies(); // Biztonsági törlés

        for (int i = 0; i < butterflyCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(islandCenter.x - spawnAreaSize.x / 2, islandCenter.x + spawnAreaSize.x / 2),
                Random.Range(islandCenter.y, islandCenter.y + spawnAreaSize.y), // Magasból indulnak
                Random.Range(islandCenter.z - spawnAreaSize.z / 2, islandCenter.z + spawnAreaSize.z / 2)
            );

            GameObject bf = Instantiate(butterflyPrefab, randomPos, Quaternion.identity);
            
            // Beállítjuk a wanderRadius-t a scripten, hogy ne repüljenek ki a világból
            var ai = bf.GetComponent<ButterflyAI>();
            if (ai != null)
            {
                // A start pozíciójuk körül fognak keringeni
                ai.wanderRadius = 20f; 
            }
            
            activeButterflies.Add(bf);
        }
        Debug.Log($"[ButterflyManager] {butterflyCount} pillangó létrehozva.");
    }

    void ClearButterflies()
    {
        foreach (var bf in activeButterflies)
        {
            if (bf != null) Destroy(bf);
        }
        activeButterflies.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        // Sárga szín a doboznak
        Gizmos.color = Color.yellow;
        
        // A spawn terület közepének kiszámítása a rajzoláshoz
        // (Mivel a kód szerint az islandCenter.y az ALJA a területnek)
        Vector3 drawCenter = new Vector3(
            islandCenter.x, 
            islandCenter.y + (spawnAreaSize.y / 2), 
            islandCenter.z
        );

        // Kirajzoljuk a dobozt, ahol a pillangók születnek
        Gizmos.DrawWireCube(drawCenter, spawnAreaSize);
        
        // Kirajzoljuk a középpontot is egy gömbbel
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(islandCenter, 1f);
    }
}