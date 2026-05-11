using UnityEngine;
using GDS.Core;

// FONTOS: Bekerült az ISaveable interfész!
public class TreasureChest : MonoBehaviour, ISaveable
{[Header("Mentés Beállítások")][Tooltip("Minden ládának a pályán MÁS nevet kell ide beírni! (pl. cave_chest_01)")]
    public string chestID; // Ez alapján ismeri meg a SaveManager, hogy melyik láda ez![Header("Mit rejt a láda?")]
    public string itemID;
    public int amount = 1;[Header("Referenciák")]
    public Animator animator;
    public GameObject itemInsideModel; // A modell a ládában

    // Két állapotunk van most már
    private bool isOpen = false;   // Kinyílt már?
    private bool isLooted = false; // Kivették már belőle a cuccot?

    public void Interact()
    {
        // Ha már ki van fosztva, nem csinálunk semmit
        if (isLooted) return;

        // 1. FÁZIS: KINYITÁS
        if (!isOpen)
        {
            Debug.Log("[Chest] Láda kinyitása...");

            if (animator != null)
            {
                animator.SetTrigger("Open");
            }

            isOpen = true;
            return; // Itt kilépünk, és várjuk a következő "E" nyomást
        }

        // 2. FÁZIS: FELVÉTEL (Csak akkor fut le, ha már isOpen = true)
        if (isOpen && !isLooted)
        {
            Debug.Log("[Chest] Tárgy kivétele...");

            if (InventoryManager.Instance != null)
            {
                ItemBase itemToAdd = GetItemFromDatabase(itemID);

                if (itemToAdd != null)
                {
                    InventoryManager.Instance.AddItemToInventory(itemToAdd, amount);
                    Debug.Log($"[Chest] Kaptál: {amount} db {itemToAdd.Name}");
                }
            }

            // Modell eltüntetése
            if (itemInsideModel != null)
            {
                itemInsideModel.SetActive(false);
            }

            // Lezárjuk a folyamatot
            isLooted = true;

            // Kikapcsoljuk az interakciót (Collider & Highlight)
            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            var highlight = GetComponent<GDS.Common.Scripts.IHighlight>();
            if (highlight != null) highlight.Unhighlight();
        }
    }

    private ItemBase GetItemFromDatabase(string id)
    {
        if (id == MyItemDatabase.Sword.Id) return MyItemDatabase.Sword;
        if (id == MyItemDatabase.Stone.Id) return MyItemDatabase.Stone;
        if (id == MyItemDatabase.WoodLog.Id) return MyItemDatabase.WoodLog;
        if (id == MyItemDatabase.RawMeat.Id) return MyItemDatabase.RawMeat;
        if (id == MyItemDatabase.Axe.Id) return MyItemDatabase.Axe;
        return null;
    }

    // ==========================================
    // --- MENTÉS ÉS BETÖLTÉS LOGIKA ---
    // ==========================================

    public void SaveData(ref GameData data)
    {
        // 1. fázis mentése
        if (isOpen && !data.openedChestIDs.Contains(chestID))
        {
            data.openedChestIDs.Add(chestID);
        }

        // 2. fázis mentése
        if (isLooted && !data.lootedChestIDs.Contains(chestID))
        {
            data.lootedChestIDs.Add(chestID);
        }
    }

    public void LoadData(GameData data)
    {
        // 1. Fázis visszaállítása: Nyitva hagytuk?
        if (data.openedChestIDs.Contains(chestID))
        {
            isOpen = true;

            // Szólunk az animátornak, hogy nyíljon ki
            if (animator != null)
            {
                animator.SetTrigger("Open");
                // Tipp: Ha betöltéskor furcsa, hogy újra lejátssza a nyitás animációt, 
                // ide írhatod az animator.Play("NyitottAllapotNeve"); parancsot is, 
                // hogy azonnal a végső pózba ugorjon.
            }
        }

        // 2. Fázis visszaállítása: Ki is fosztottuk?
        if (data.lootedChestIDs.Contains(chestID))
        {
            isLooted = true;

            if (itemInsideModel != null)
            {
                itemInsideModel.SetActive(false); // Eltüntetjük a kardot/tárgyat
            }

            // Kikapcsoljuk a collidert, hogy ne is mutassa a felvétel opciót
            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            var highlight = GetComponent<GDS.Common.Scripts.IHighlight>();
            if (highlight != null) highlight.Unhighlight();
        }
    }
}