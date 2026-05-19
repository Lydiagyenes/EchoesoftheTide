using UnityEngine;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance { get; private set; }

    [Header("Minden étel listája")]
    public List<FoodItem> allFoods; // Húzd be ide a létrehozott FoodItemeket!

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // Kereső függvény
    public FoodItem GetFoodByID(string searchID)
    {
        foreach (var food in allFoods)
        {
            if (food.id == searchID) return food;
        }
        return null;
    }
}