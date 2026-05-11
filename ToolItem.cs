using UnityEngine;
using GDS.Core;

public enum ToolType
{
    None,
    Axe,        // Fa vágás
    Pickaxe,    // Kő bányászás
    Weapon,     // Harc
    Key         // Ajtó nyitás
}

[CreateAssetMenu(fileName = "New Tool Item", menuName = "Crafting System/Tool Item")]
public class ToolItem : ScriptableObject // <-- VÁLTOZÁS: ScriptableObject lett!
{
    [Header("Inventory Adatok")]
    public string id;           // Pl. "stone_axe"
    public string itemName;     // Pl. "Kőbalta"
    public string iconPath;     // Pl. "Items/Tools/axe" (Resources mappa!)
    
    [Header("Eszköz Beállítások")]
    public ToolType toolType;
    
    [Tooltip("Hányszor használható, mielőtt eltörik?")]
    public int maxDurability = 20;

    // Ez a függvény készíti el a tárgyat az inventory számára a játékban
    public ItemBase CreateRuntimeItem()
    {
        return new ItemBase()
        {
            Id = this.id,
            Name = this.itemName,
            Icon = this.iconPath,
            Stack = new Stack(1) // Eszközök nem stackelhetők (1 db)
        };
    }
}