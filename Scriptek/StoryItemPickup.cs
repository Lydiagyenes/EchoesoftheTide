using UnityEngine;
using GDS.Core; 

public class StoryItemPickup : MonoBehaviour
{
    [Header("Tárgy Adatok")]
    public string itemID; // Pl. "JournalPage_1"
    public int amount = 1;

    [Header("Történet")]
    [Tooltip("Ez a szöveg jelenik meg felvételkor (Elias naplója)")]
    public DialogueLine journalEntryLine;

    [Tooltip("Ez a Quest indul el, MIUTÁN a szöveg lejátszódott")]
    public Quest nextQuestToStart;

    // Ezt hívja a NearbyAction
   public void Interact()
    {
        // =========================================================
        // 0. TUDÁS ELMENTÉSE (Beégetjük a naplóba!)
        // Még azelőtt rögzítjük, hogy bekerülne a táskába.
        // =========================================================
        if (QuestLog.Instance != null)
        {
            QuestLog.Instance.UnlockJournalEntry(itemID);
        }

        // 1. Fizikai tárgy felvétele (Ezt a játékos később akár el is dobhatja)
        if (InventoryManager.Instance != null)
        {
             ItemBase item = GetItemFromDB(itemID);
             if(item != null) InventoryManager.Instance.AddItemToInventory(item, amount);
        }

        // 2. Objektum eltüntetése a világból
        GetComponent<Collider>().enabled = false;
        GetComponentInChildren<Renderer>().enabled = false;
        
        var highlight = GetComponent<GDS.Common.Scripts.IHighlight>();
        if(highlight != null) highlight.Unhighlight();

        // 3. Narráció és Quest indítás
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.PlayDialogue(journalEntryLine, () => 
            {
                if (nextQuestToStart != null && QuestLog.Instance != null)
                {
                    QuestLog.Instance.AddQuest(nextQuestToStart);
                    Debug.Log($"[StoryItem] Új quest elindítva: {nextQuestToStart.questName}");
                }
                
                Destroy(gameObject);
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Segéd a DB-hez
   // Ez a verzió Tükrözést (Reflection) használ, így bármilyen 
    // MyItemDatabase-ben lévő tárgyat megtalál a neve alapján,
    // nem kell egyesével beírogatni őket!
    private ItemBase GetItemFromDB(string id)
    {
        // 1. Megpróbáljuk megkeresni a MyItemDatabase mezői között
        var field = typeof(MyItemDatabase).GetField(id);
        if (field != null)
        {
            return (ItemBase)field.GetValue(null);
        }

        // 2. Ha nem a változó neve az ID, hanem a tartalma, akkor manuális keresés kellene,
        // de az ID-kat általában ugyanúgy nevezzük el.
        // Ha nagyon biztosra akarsz menni, használhatod ezt a "csúnya" switch-et is, 
        // de a fenti 3 sor kiváltja az egészet, ha az Inspectorban "JournalItem"-et írsz be ID-nak.
        
        return null;
    }
}