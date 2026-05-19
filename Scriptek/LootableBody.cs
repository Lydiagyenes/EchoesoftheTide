using UnityEngine;
using GDS.Core;

public class LootableBody : MonoBehaviour
{
    [Header("Loot Beállítások")]
    public int meatAmount = 2;
    public int boneAmount = 1;
    public int skinAmount = 1;

    private bool isLooted = false;

    // Ezt a függvényt kell majd meghívnia a játékosnak (pl. "E" gomb vagy kattintás)
    public void Interact()
    {
        if (isLooted) return;
        
        Debug.Log("[Loot] Farkas megnyúzása...");

        // Biztonsági ellenőrzés
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("Nincs InventoryManager!");
            return;
        }

        // Hozzáadjuk a tárgyakat
        if (meatAmount > 0)
            InventoryManager.Instance.AddItemToInventory(MyItemDatabase.RawMeat, meatAmount);
        
        if (boneAmount > 0)
            InventoryManager.Instance.AddItemToInventory(MyItemDatabase.Bone, boneAmount);

        if (skinAmount > 0)
            InventoryManager.Instance.AddItemToInventory(MyItemDatabase.WolfSkin, skinAmount);

        // Visszajelzés
        Debug.Log("Sikeres zsákmányolás!");
        isLooted = true;

        // Test eltüntetése
        // Adunk neki egy kis időt, vagy azonnal töröljük.
        // Itt most azonnal töröljük a modellt a pályáról.
        Destroy(gameObject);
    }
}