using UnityEngine;
using GDS.Core; 

public enum PlantSourceColor { Red, Black, Yellow, Potion }
public enum PlantType { Berry, Mushroom }

[CreateAssetMenu(fileName = "New Botany Item", menuName = "Botany System/Botany Item")]
public class BotanyItem : ScriptableObject // <-- MOST MÁR LÉTREHOZHATÓ LESZ!
{
    [Header("Inventory Adatok")]
    public string id;           // Pl. "verfurt"
    public string itemName;     // Pl. "Vérfürt"
    public string iconPath;     // Pl. "Items/Berries/verfurt_icon" (Resources mappa útvonal!)
    
    [Tooltip("A Botanikus Könyvben megjelenő kép (Drag & Drop)")]
    public Sprite bookIllustration; 

    public int maxStack = 64;

    [Header("Botanikai Adatok")]
    [TextArea(5, 10)]
    public string botanicalDescription; 
    
    public bool isPoisonous; 
    
    [Header("Sorsolási Szabályok")]
    public PlantSourceColor sourceColor; 
    public PlantType plantType;          

    [Header("Fogyasztási Hatások")]
    [Tooltip("Mennyi életet ad (negatív = sebzés)")]
    public float healthEffect = 5f; 

    [Tooltip("Mennyi ideig tart a stamina boost (másodperc)")]
    public float staminaBuffDuration = 30f;

    [Tooltip("Hányszorosára növeli a stamina töltést? (1.5 = +50%)")]
    public float staminaRegenMultiplier = 1.5f;

    // --- EZ A FÜGGVÉNY KAPCSOLJA ÖSSZE A GDS RENDSZERREL ---
    // Amikor felvesszük a növényt, ez a függvény csinál belőle egy Inventoryba rakható tárgyat
    public ItemBase CreateRuntimeItem()
    {
        return new ItemBase()
        {
            Id = this.id,
            Name = this.itemName,
            Icon = this.iconPath, // A GDS rendszer útvonalat vár (string)
            Stack = new Stack((ushort)this.maxStack)
        };
    }
}