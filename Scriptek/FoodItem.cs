using UnityEngine;
using GDS.Core; 

[CreateAssetMenu(fileName = "New Food Item", menuName = "RPG/Food Item")]
public class FoodItem : ScriptableObject
{
    [Header("Inventory Adatok")]
    public string id;           // EGYEZZEN a MyItemDatabase ID-vel! (pl. "CookedMeat")
    public string itemName;     // Pl. "Sült Hús"
    public string iconPath;     // Pl. "Items/Food/CookedMeat"
    public int maxStack = 20;

    [Header("Fogyasztási Hatások")]
    [Tooltip("Mennyi életet ad")]
    public float healthEffect = 20f; 

    [Tooltip("Mennyi ideig tart a stamina boost (másodperc)")]
    public float staminaBuffDuration = 0f;

    [Tooltip("Hányszorosára növeli a stamina töltést?")]
    public float staminaRegenMultiplier = 1f;

    // Ez a függvény készít belőle Inventory kompatibilis tárgyat
    public ItemBase CreateRuntimeItem()
    {
        return new ItemBase()
        {
            Id = this.id,
            Name = this.itemName,
            Icon = this.iconPath,
            Stack = new Stack((ushort)this.maxStack)
        };
    }
}