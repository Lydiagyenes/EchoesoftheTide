using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VegetationSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public List<GameObject> vegetationPrefabs;
    public int density = 30; // Kicsit visszavettem a sűrűségből a biztonság kedvéért
    public float minScale = 0.8f;
    public float maxScale = 1.5f;

    public LayerMask groundLayer;

    private BoxCollider spawnZone;
    private List<GameObject> spawnedVegetation = new List<GameObject>();
    private bool hasSpawned = false;

    void Awake()
    {
        spawnZone = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            StartCoroutine(SpawnVegetationRoutine());
            hasSpawned = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hasSpawned)
        {
            DespawnVegetation();
            hasSpawned = false;
        }
    }

    IEnumerator SpawnVegetationRoutine()
    {
        // 1. Ellenőrzés: Van mit lerakni?
        if (vegetationPrefabs == null || vegetationPrefabs.Count == 0)
        {
            Debug.LogError($"[Spawner] HIBA: A {gameObject.name} spawneren üres a 'Vegetation Prefabs' lista!");
            yield break;
        }

        Bounds bounds = spawnZone.bounds;
        int spawnedCount = 0;
        int maxAttempts = density * 10; 
        int attempts = 0;

        Debug.Log($"[Spawner] Start! Sűrűség: {density}. Zóna: {gameObject.name}. Próbálkozom a földet megtalálni...");

        while (spawnedCount < density && attempts < maxAttempts)
        {
            attempts++;
            
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            
            // Fentről indítjuk a sugarat
            Vector3 rayStart = new Vector3(randomX, bounds.max.y + 50f, randomZ); // Magasabbról indítjuk (50f)
            
            RaycastHit hit;
            
            // 2. Ellenőrzés: Eltaláljuk a földet?
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, groundLayer))
            {
                // TALÁLT!
                GameObject prefabToSpawn = vegetationPrefabs[Random.Range(0, vegetationPrefabs.Count)];
                
                if (prefabToSpawn == null)
                {
                     Debug.LogError("[Spawner] HIBA: A listában lévő egyik Prefab üres (null)!");
                     continue;
                }

                GameObject newVeg = Instantiate(prefabToSpawn, hit.point, Quaternion.identity);
                
                newVeg.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                float scale = Random.Range(minScale, maxScale);
                newVeg.transform.localScale = Vector3.one * scale;
                newVeg.transform.parent = this.transform;
                
                // Bekapcsoljuk, biztos ami biztos
                newVeg.SetActive(true);
                
                spawnedVegetation.Add(newVeg);
                spawnedCount++;

                if (spawnedCount % 5 == 0) yield return null;
            }
            else
            {
                // NEM TALÁLT! Rajzolunk egy piros vonalat a Scene-ben
                Debug.DrawRay(rayStart, Vector3.down * 100f, Color.red, 1.0f);
            }
        }

        if (spawnedCount == 0)
        {
            Debug.LogError($"[Spawner] KRITIKUS: A ciklus lefutott, de 0 bokrot sikerült lerakni! Valószínűleg a 'Ground Layer' beállítás rossz!");
        }
        else
        {
            Debug.Log($"[Spawner] Siker! {spawnedCount} bokor lerakva.");
        }
    }

    void DespawnVegetation()
    {
        foreach (GameObject veg in spawnedVegetation)
        {
            if (veg != null) Destroy(veg);
        }
        spawnedVegetation.Clear();
    }
}