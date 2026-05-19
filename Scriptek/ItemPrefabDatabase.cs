using UnityEngine;
using System.Collections.Generic;

public class ItemPrefabDatabase : MonoBehaviour
{
    public static ItemPrefabDatabase Instance { get; private set; }

    [System.Serializable]
    public struct ItemPrefabEntry
    {
        public string itemID;    // Pl: "wood_log"
        public GameObject prefab; // A 3D modell prefabja
    }

    public List<ItemPrefabEntry> itemPrefabs;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetPrefab(string id)
    {
        foreach (var entry in itemPrefabs)
        {
            if (entry.itemID == id) return entry.prefab;
        }
        return null;
    }
}