using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Fontos a listák szűréséhez!

public class BotanyManager : MonoBehaviour
{
    public static BotanyManager Instance { get; private set; }

    [Header("A Teljes Növény Adatbázis")]
    // Ide kell majd behúznod az ÖSSZES (100 db) növény ScriptableObject-jét
    public List<BotanyItem> allPlantsDatabase = new List<BotanyItem>();

    // A játék indításakor szétválogatjuk őket listákba a gyors sorsoláshoz
    private Dictionary<(PlantSourceColor, PlantType), List<BotanyItem>> lootTables = new Dictionary<(PlantSourceColor, PlantType), List<BotanyItem>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Ha a _GameSystems alatt van, nem kell DontDestroy
        
        InitializeLootTables();
    }

    private void InitializeLootTables()
    {
        // Végigmegyünk az összes növényen és kategóriákba rendezzük őket
        foreach (var plant in allPlantsDatabase)
        {
            var key = (plant.sourceColor, plant.plantType);
            
            if (!lootTables.ContainsKey(key))
            {
                lootTables[key] = new List<BotanyItem>();
            }
            lootTables[key].Add(plant);
        }
        
        Debug.Log($"Botanikai adatbázis inicializálva. Összes növény: {allPlantsDatabase.Count}");
    }

    // Ezt a függvényt hívja majd a bokor/gomba prefab a világban
    public BotanyItem GetRandomPlant(PlantSourceColor color, PlantType type)
    {
        var key = (color, type);

        if (lootTables.ContainsKey(key) && lootTables[key].Count > 0)
        {
            List<BotanyItem> possiblePlants = lootTables[key];
            // Véletlenszerű választás a listából
            int randomIndex = Random.Range(0, possiblePlants.Count);
            return possiblePlants[randomIndex];
        }

        Debug.LogError($"HIBA: Nincs ilyen növény az adatbázisban! Szín: {color}, Típus: {type}");
        return null;
    }
    // ID alapján megkeresi a növény adatait (hogy tudjuk, mennyit gyógyít)
    public BotanyItem GetPlantByID(string id)
    {
        // Végigmegyünk a teljes adatbázison
        foreach (var plant in allPlantsDatabase)
        {
            if (plant.id == id) return plant;
        }
        return null;
    }
}