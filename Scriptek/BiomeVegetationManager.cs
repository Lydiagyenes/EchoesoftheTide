using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VegetationPoint
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public bool isActive;
    public GameObject activeObject;
    public bool isHarvested;
}

public class BiomeVegetationManager : MonoBehaviour
{
    [Header("Referenciák")]
    public Transform player;
    public LayerMask groundLayer;

    [Header("Biom Beállítások")]
    public List<GameObject> biomePrefabs;
    public List<BoxCollider> spawnZones; 

    [Header("Generálás")]
    public float density = 5.0f;    
    public int maxActiveObjects = 300; 

    [Header("Láthatóság")]
    public float viewDistance = 60f; 
    public float updateRate = 0.5f;

    [Header("Korrekciók")]
    public float waterLevel = 3.5f; 
    public float heightOffset = 0.2f; 

    // BELSŐ ADATOK
    private List<VegetationPoint> allPoints = new List<VegetationPoint>();
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private float timer;

    // DIAGNOSZTIKAI VÁLTOZÓK
    private int totalHits = 0;
    private int skippedWater = 0;
    private int successfulPoints = 0;

    void Start()
    {

        // Reseteljük a számlálókat
        totalHits = 0;
        skippedWater = 0;
        successfulPoints = 0;

        foreach (var zone in spawnZones)
        {
            if(zone != null) GeneratePointsInZone(zone);
        }

        InitializePool();

        // --- A NAGY JELENTÉS KIÍRÁSA ---
        Debug.Log($"<color=yellow>[VegManager JELENTÉS]</color>");
        Debug.Log($"Találatok a földön: {totalHits}");
        Debug.Log($"Víz miatt kihagyva: {skippedWater}");
        Debug.Log($"LÉTREHOZOTT PONTOK SZÁMA: {successfulPoints}");
        Debug.Log($"MAX AKTÍV OBJEKTUMOK (Pool): {maxActiveObjects}");

        if (successfulPoints > maxActiveObjects)
        {
            Debug.LogError($"<color=red>FIGYELEM! Több a fa ({successfulPoints}), mint a keret ({maxActiveObjects})! A fák egy része sosem fog megjelenni! Nöld meg a Max Active Objects-et!</color>");
        }
        else
        {
            Debug.Log("<color=green>A keret elegendő, minden fa meg tud jelenni.</color>");
        }
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                Debug.Log("[VegManager] Játékos SIKERESEN megtalálva! A vegetáció mostantól követi.");
                
                // Azonnal frissítünk egyet, hogy ne kelljen várni
                UpdateVisibility(); 
            }
            else
            {
                // Ha még nincs játékos, kilépünk, és várunk a következő frame-re
                return; 
            }
        }

        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            UpdateVisibility();
            timer = 0f;
        }
    }

    void GeneratePointsInZone(BoxCollider zone)
    {
        Bounds bounds = zone.bounds;
        float raycastStartHeight = 200f; 
        
        for (float x = bounds.min.x; x < bounds.max.x; x += density)
        {
            for (float z = bounds.min.z; z < bounds.max.z; z += density)
            {
                float offsetX = Random.Range(-density / 2f, density / 2f);
                float offsetZ = Random.Range(-density / 2f, density / 2f);

                Vector3 rayStart = new Vector3(x + offsetX, raycastStartHeight, z + offsetZ);
                
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 500f, groundLayer))
                {
                    totalHits++;

                    // Vízszint ellenőrzés
                    if (hit.point.y < waterLevel) 
                    {
                        skippedWater++;
                        // Debug.DrawLine(rayStart, hit.point, Color.blue, 10f); // Kék, ha víz alatt van
                        continue;
                    }

                    // SIKERES PONT
                    VegetationPoint newPoint = new VegetationPoint();
                    newPoint.position = hit.point + Vector3.up * heightOffset;
                    newPoint.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    newPoint.scale = Vector3.one * Random.Range(0.8f, 1.3f);
                    newPoint.isActive = false;

                    allPoints.Add(newPoint);
                    successfulPoints++;
                    
                    // Debug.DrawLine(rayStart, hit.point, Color.green, 10f); // Zöld, ha jó
                }
            }
        }
    }

    void InitializePool()
    {
        if (biomePrefabs.Count == 0) return;

        // Csak annyit hozunk létre, amennyi a limit
        for (int i = 0; i < maxActiveObjects; i++)
        {
            GameObject prefab = biomePrefabs[i % biomePrefabs.Count];
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
        }
    }

    void UpdateVisibility()
    {
        Vector3 playerPos = player.position;
        float distSq = viewDistance * viewDistance;

        foreach (var point in allPoints)
        {
              if (point.isHarvested) continue;

            float d = (point.position - playerPos).sqrMagnitude;
            
            // Láthatónak kell lennie?
            bool shouldBeVisible = d < distSq;

            if (shouldBeVisible)
            {
                 if (point.isActive)
                {
                    // ...de az objektum NULL (mert kivágták!), akkor regisztráljuk a halálát
                    if (point.activeObject == null)
                    {
                        point.isHarvested = true; // Kivágva!
                        point.isActive = false;
                        // Nem tudjuk visszatenni a poolba, mert megsemmisült. 
                        // A pool mérete eggyel csökkent, de ez nem baj.
                    }
                }
                else
                {
               
                    if (objectPool.Count > 0)
                    {
                        GameObject obj = objectPool.Dequeue();
                        obj.transform.position = point.position;
                        obj.transform.rotation = point.rotation;
                        obj.transform.localScale = point.scale;
                        obj.SetActive(true);

                        point.activeObject = obj;
                        point.isActive = true;
                    }
                    else
                    {
                        // Elfogyott a pool! Ezt nem tudjuk megjeleníteni.
                        // Ez okozhatja a hiányzó fákat.
                    }
                }
            }
            
            else
            {
                if (point.isActive)
                {
                    if (point.activeObject != null)
                    {
                        GameObject obj = point.activeObject;
                        obj.SetActive(false);
                        objectPool.Enqueue(obj); // Vissza a raktárba
                    }
                    else
                    {
                        // Ha aktív volt, de null, akkor kivágták, miközben távolodtunk (ritka, de lehetséges)
                        point.isHarvested = true;
                    }

                    point.activeObject = null;
                    point.isActive = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnZones != null)
        {
            Gizmos.color = Color.green;
            foreach (var zone in spawnZones)
            {
                if (zone != null) Gizmos.DrawWireCube(zone.bounds.center, zone.bounds.size);
            }
        }
    }
}