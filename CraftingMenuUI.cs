using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GDS.Core;

public class CraftingMenuUI : MonoBehaviour
{
    [Header("Bal Oldal (Lista)")]
    public Transform recipeListContent;
    public GameObject recipeButtonPrefab;

    [Header("Jobb Oldal (Részletek)")]
    public GameObject detailsPanel;
    public TextMeshProUGUI recipeNameText;
    public Image resultIcon;
    public TextMeshProUGUI ingredientsText;
    public Button craftButton;

    private CraftingRecipe currentRecipe;
    private Button selectedButton;

    private void OnEnable()
    {
        Debug.Log("[CraftingUI] Menü megnyitva. Lista frissítése...");
        RefreshRecipeList();
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    public void RefreshRecipeList()
    {
        foreach (Transform child in recipeListContent) Destroy(child.gameObject);

        if (CraftingManager.Instance == null)
        {
            Debug.LogError("[CraftingUI] HIBA: Nincs CraftingManager!");
            return;
        }

        Debug.Log($"[CraftingUI] Talált receptek száma: {CraftingManager.Instance.allRecipes.Count}");

        foreach (var recipe in CraftingManager.Instance.allRecipes)
        {
            // Debug: Kiírjuk minden recept nevét, amit betölt
            // Debug.Log($"[CraftingUI] Recept feldolgozása: {(recipe.resultTool != null ? recipe.resultTool.itemName : "HIBÁS RECEPT")}");

            if (!string.IsNullOrEmpty(recipe.requiredSkillID))
            {
                if (SkillManager.Instance != null && !SkillManager.Instance.HasSkill(recipe.requiredSkillID))
                    continue; 
            }

            GameObject btnObj = Instantiate(recipeButtonPrefab, recipeListContent);
            
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                // Ha Tool, annak a neve. Ha Plant, annak a neve.
                if (recipe.resultTool != null) 
                    btnText.text = recipe.resultTool.itemName;
                else if (recipe.resultPlant != null)
                    btnText.text = recipe.resultPlant.itemName;
            }

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => 
            {
                ShowRecipeDetails(recipe);
                SetSelectedButton(btn);
            });
        }
    }

    public void ShowRecipeDetails(CraftingRecipe recipe)
    {
       currentRecipe = recipe;
        if (detailsPanel != null) detailsPanel.SetActive(true);

        // --- ADATOK MEGJELENÍTÉSE ---
        if (recipe.resultTool != null)
        {
            recipeNameText.text = recipe.resultTool.itemName;
            Sprite icon = Resources.Load<Sprite>(recipe.resultTool.iconPath);
            if (icon != null) resultIcon.sprite = icon;
        }
        else if (recipe.resultPlant != null)
        {
            // HA NÖVÉNY / POTION:
            recipeNameText.text = recipe.resultPlant.itemName;
            // Itt figyelj: a BotanyItem iconPath-ja már nem tartalmazza a "Resources/"-t a javítás óta
            Sprite icon = Resources.Load<Sprite>(recipe.resultPlant.iconPath);
            if (icon != null) resultIcon.sprite = icon;
        }

        UpdateIngredientsUI();
        
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftClick);
    }

    private void UpdateIngredientsUI()
    {
        if (currentRecipe == null) return;

        string ingredientsList = "<b>Szükséges anyagok:</b>\n\n";
        bool canCraftAll = true;

        foreach (var ingredient in currentRecipe.ingredients)
        {
            int currentAmount = 0;

            // --- JAVÍTÁS: ITT IS KEZELNI KELL A JOKEREKET! ---
            
            if (ingredient.itemID == "ANY_EDIBLE")
            {
                // Ha a recept ehető bogyót kér, a speciális számlálót hívjuk
                currentAmount = InventoryManager.Instance.GetBotanyAmountByType(false);
            }
            else if (ingredient.itemID == "ANY_POISON")
            {
                // Ha mérgezőt kér
                currentAmount = InventoryManager.Instance.GetBotanyAmountByType(true);
            }
            else
            {
                // Ha konkrét tárgyat kér (pl. Stone)
                currentAmount = InventoryManager.Instance.GetItemAmount(ingredient.itemID);
            }
            // -------------------------------------------------

            int requiredAmount = ingredient.amount;

            string color = (currentAmount >= requiredAmount) ? "green" : "#FF4444";
            
            if (currentAmount < requiredAmount) canCraftAll = false;

            string displayName = GetItemNamePretty(ingredient.itemID);

            ingredientsList += $"- {displayName}: <color={color}>{currentAmount} / {requiredAmount}</color>\n";
        }

        ingredientsText.text = ingredientsList;
        craftButton.interactable = canCraftAll; 
    }

    private void OnCraftClick()
    {
        if (currentRecipe != null)
        {
            CraftingManager.Instance.Craft(currentRecipe);
            UpdateIngredientsUI();
            if (InventoryManager.Instance != null) InventoryManager.Instance.MainInventory.Data.Notify();
        }
    }

    private void SetSelectedButton(Button newButton)
    {
        if (selectedButton != null)
        {
            var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null) text.fontStyle = FontStyles.Normal;
        }
        selectedButton = newButton;
        if (selectedButton != null)
        {
            var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null) text.fontStyle = FontStyles.Bold;
        }
    }

    private string GetItemNamePretty(string id)
    {
        if (id == "Stone") return "Kő";
        if (id == "Wood") return "Faág";

         if (id == "ANY_EDIBLE") return "Bármilyen Ehető Bogyó";
        if (id == "ANY_POISON") return "Bármilyen Mérgező Bogyó";
        
        if (BotanyManager.Instance != null)
        {
            var plant = BotanyManager.Instance.GetPlantByID(id);
            if (plant != null) return plant.itemName;
        }
        if (string.IsNullOrEmpty(id)) return "Ismeretlen ID";
        return char.ToUpper(id[0]) + id.Substring(1); 
    }
}