using UnityEngine;
using GDS.Core;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();

      public List<ToolItem> allToolsDatabase = new List<ToolItem>(); 


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

     public ToolItem GetToolByID(string id)
    {
        foreach (var tool in allToolsDatabase)
        {
            if (tool.id == id) return tool;
        }
        return null;
    }

    // --- ELLENŐRZÉS ---
    public bool CanCraft(CraftingRecipe recipe)
    {
        // 1. Skill ellenőrzés
        if (!string.IsNullOrEmpty(recipe.requiredSkillID))
        {
            if (SkillManager.Instance != null && !SkillManager.Instance.HasSkill(recipe.requiredSkillID))
            {
                // Debug.Log("Nincs meg a szükséges képesség.");
                return false;
            }
        }

        // 2. Alapanyag ellenőrzés
        foreach (var ingredient in recipe.ingredients)
        {
            int currentAmount = 0;

            // --- JOKER KEZELÉS ---
            if (ingredient.itemID == "ANY_EDIBLE")
            {
                // Bármilyen ehető bogyót keresünk
                currentAmount = InventoryManager.Instance.GetBotanyAmountByType(false);
            }
            else if (ingredient.itemID == "ANY_POISON")
            {
                // Bármilyen mérgező bogyót keresünk
                currentAmount = InventoryManager.Instance.GetBotanyAmountByType(true);
            }
            else
            {
                // Normál, konkrét tárgy keresése (pl. Stone)
                currentAmount = InventoryManager.Instance.GetItemAmount(ingredient.itemID);
            }
            
            if (currentAmount < ingredient.amount) return false;
        }
        
        return true;
    }

    // --- GYÁRTÁS ---
    public void Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe)) return;

        // --- SKILL: GAZDASÁGOS ÉPÍTŐ (Economical Builder) ---
        bool freeCraft = false;
        if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Gazdasagos_Epito"))
        {
            // 20% esély (Random.value 0.0 és 1.0 között ad vissza számot)
            if (Random.value <= 0.2f)
            {
                freeCraft = true;
                Debug.Log("<color=green>GAZDASÁGOS ÉPÍTŐ! Nem fogyasztottál alapanyagot!</color>");
            }
        }
        // ----------------------------------------------------

        // 1. Alapanyagok elvétele (CSAK HA NEM INGYENES)
        if (!freeCraft)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.itemID == "ANY_EDIBLE")
                    InventoryManager.Instance.RemoveBotanyItemsByType(false, ingredient.amount);
                else if (ingredient.itemID == "ANY_POISON")
                    InventoryManager.Instance.RemoveBotanyItemsByType(true, ingredient.amount);
                else
                    InventoryManager.Instance.RemoveItems(ingredient.itemID, ingredient.amount);
            }
        }

        // 2. Késztermék létrehozása (Ez a rész változatlan marad)
        // ... (A Tool/Plant létrehozó logika) ...
        
        // MÁSOLÁS SEGÍTSÉG (csak a végét írom ide):
        ItemBase finalItem = null;
        if (recipe.resultTool != null) finalItem = recipe.resultTool.CreateRuntimeItem();
        else if (recipe.resultPlant != null) finalItem = recipe.resultPlant.CreateRuntimeItem();

        if (finalItem != null)
        {
            InventoryManager.Instance.AddItemToInventory(finalItem, recipe.resultAmount);
            Debug.Log($"Sikeres craftolás: {finalItem.Name}");
        }
    }
}