using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting System/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Mit készítünk?")]
    // Ide húzd be a ToolItem fájlt (amit most csináltál)
    public ToolItem resultTool; 
    public BotanyItem resultPlant;
    public string resultItemID;
    public int resultAmount = 1;

    [Header("Hozzávalók (ID alapján)")]
    // Mivel az ItemBase-t nem lehet behúzni, ID-kat használunk (pl. "Stone", "Wood")
    public List<Ingredient> ingredients; 

    [Header("Feltételek")]
    public string requiredSkillID;

    [System.Serializable]
    public struct Ingredient
    {
        public string itemID; // Pl. "Stone" (Ugyanaz, mint a BotanyItem vagy MyItemDatabase ID-ja)
        public int amount;    // Pl. 2
    }
}