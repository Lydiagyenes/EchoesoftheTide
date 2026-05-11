using UnityEngine;
using GDS.Core; // Ha az ItemBase innen jön
using System.Collections.Generic;

public class AltarTrigger : MonoBehaviour, ISaveable
{
    [Header("Követelmények")]
    public string requiredItemID; // Pl: "WolfSkin" (Pontosan egyezzen a MyItemDatabase ID-vel!)
    public int requiredAmount = 1;

    [Header("Jutalom (Mit kapcsoljon be?)")]
    public GameObject objectToActivate; // Húzd be ide a Barlang bejáratát vagy a Spawn Pointot!

    [Header("Párbeszédek")]
    public Dialogue requirementDialogue; // "Hozz nekem 2 bőrt..."
    public Dialogue successDialogue;     // "Az ősök elégedettek. Az út szabad."
    public Dialogue alreadyCompletedDialogue; // "Már áldoztál."

    private bool isCompleted = false;

    // --- MENTÉS RENDSZER ---
    public string id; // Adj neki egyedi ID-t az Inspectorban! (pl. "cave_altar_1")

    [ContextMenu("Generate ID")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

   public void Interact()
    {
        // 1. Ha már készen vagyunk
        if (isCompleted)
        {
            if (alreadyCompletedDialogue != null)
                // JAVÍTÁS: ", null"-t adtunk a végére
                DialogueManager.Instance.StartDialogue(alreadyCompletedDialogue, null);
            return;
        }

        // 2. Ellenőrizzük az Inventory-t
        if (InventoryManager.Instance != null)
        {
            int currentAmount = InventoryManager.Instance.GetItemAmount(requiredItemID);

            if (currentAmount >= requiredAmount)
            {
                // --- SIKER ---
                CompleteAltar();
                
                InventoryManager.Instance.RemoveItems(requiredItemID, requiredAmount);

                if (successDialogue != null)
                    // JAVÍTÁS: ", null" a végére
                    DialogueManager.Instance.StartDialogue(successDialogue, null);
            }
            else
            {
                // --- KUDARC ---
                Debug.Log($"[Altar] Nincs elég tárgy. Kell: {requiredAmount}, Van: {currentAmount}");
                if (requirementDialogue != null)
                    // JAVÍTÁS: ", null" a végére
                    DialogueManager.Instance.StartDialogue(requirementDialogue, null);
            }
        }
    }

    private void CompleteAltar()
    {
        isCompleted = true;
        
        // Bekapcsoljuk a barlang bejáratot
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log("[Altar] Az út megnyílt!");
        }
        
        // Opcionális: Effekt, hang, részecske lejátszása itt
    }

    // --- MENTÉS / BETÖLTÉS ---
    // (Feltételezem, hogy a GameData osztályodban van egy lista az aktivált objektumoknak)
    
    public void SaveData(ref GameData data)
    {
        // Ezt majd hozzá kell adnunk a GameData-hoz, ha még nincs "activatedAltars" listád.
        // Ha nincs, egyelőre hagyd üresen, vagy használj egy egyszerű bool checket a PlayerPrefs-el teszteléshez.
        if (isCompleted)
        {
            if (!data.activatedAltars.Contains(id))
            {
                data.activatedAltars.Add(id);
            }
        }
    }

    public void LoadData(GameData data)
    {
        if (data.activatedAltars.Contains(id))
        {
            isCompleted = true;
            if (objectToActivate != null) objectToActivate.SetActive(true);
        }
    }
}