using UnityEngine;
using System.Collections.Generic; 
using GDS.Common.Scripts; 

public class NearbyAction : MonoBehaviour
{
    // --- LISTÁK ---
    private List<ItemPickup> itemsInRange = new List<ItemPickup>();
    private List<LootableBody> bodiesInRange = new List<LootableBody>();
    private List<AltarTrigger> altarsInRange = new List<AltarTrigger>();
    private List<TreasureChest> chestsInRange = new List<TreasureChest>();
    private List<StoryItemPickup> storyItemsInRange = new List<StoryItemPickup>();

    // --- AKTUÁLIS CÉLPONTOK ---
    private ItemPickup nearbyItemPickup;
    private LootableBody nearbyLootBody;
    private AltarTrigger nearbyAltar;
    private TreasureChest nearbyChest;
    private StoryItemPickup nearbyStoryItem;

    private IHighlight currentHighlightedObject;
    private NPCInteraction nearbyNPC;

    void Update()
    {
        // 1. Mindig keressük a legközelebbit
        UpdateClosestInteractable();

        // Ha beszélgetünk, nem csinálunk semmit
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            return; 
        }

        // --- NPC KEZELÉS (R gomb) ---
        if (!GameManager.isPaused && Input.GetKeyDown(KeyCode.R))
        {
            if (nearbyNPC != null)
            {
                bool questCompletedAction = false;
                if (QuestLog.Instance != null)
                {
                    List<Quest> questsToCheck = new List<Quest>(QuestLog.Instance.activeQuests);
                    foreach (var quest in questsToCheck)
                    {
                        if (QuestLog.Instance.CheckAndCompleteQuest(quest))
                        {
                            Debug.Log($"<color=green>SIKER! Teljesítetted a '{quest.questName}' küldetést!</color>");
                            questCompletedAction = true;
                        }
                    }
                }

                if (!questCompletedAction)
                {
                    if (nearbyNPC.isTalking) DialogueManager.Instance.ContinueDialogue();
                    else nearbyNPC.TriggerDialogue();
                }
            }
        }

        // --- INTERAKCIÓ (E gomb) ---
        if (!GameManager.isPaused && Input.GetKeyDown(KeyCode.E))
        {
            // A) Tárgy
            if (nearbyItemPickup != null)
            {
                nearbyItemPickup.Interact();
                UpdateClosestInteractable(); // Azonnali frissítés
            }
            // B) Hulla (Loot)
            else if (nearbyLootBody != null)
            {
                nearbyLootBody.Interact();
                UpdateClosestInteractable();
            }
            // C) Oltár
            else if (nearbyAltar != null)
            {
                nearbyAltar.Interact();
                UpdateClosestInteractable();
            }
            // D) Láda
            else if (nearbyChest != null)
            {
                nearbyChest.Interact();
                UpdateClosestInteractable();
            }
            // E) Story Tárgy (Palack)
            else if (nearbyStoryItem != null)
            {
                nearbyStoryItem.Interact();
                UpdateClosestInteractable();
            }
        }
    }

    // --- LOGIKA: LEGKÖZELEBBI KERESÉSE ---

    void UpdateClosestInteractable()
    {
        Vector3 myPos = transform.position;

        // 1. TÁRGYAK
        for (int i = itemsInRange.Count - 1; i >= 0; i--)
            if (itemsInRange[i] == null || itemsInRange[i].gameObject == null) itemsInRange.RemoveAt(i);
        
        ItemPickup closestItem = null;
        float minItemDist = float.MaxValue;
        foreach (var item in itemsInRange)
        {
            if (item != null && item.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(myPos, item.transform.position);
                if (dist < minItemDist) { minItemDist = dist; closestItem = item; }
            }
        }

        // 2. HULLÁK
        for (int i = bodiesInRange.Count - 1; i >= 0; i--)
            if (bodiesInRange[i] == null || bodiesInRange[i].gameObject == null) bodiesInRange.RemoveAt(i);

        LootableBody closestBody = null;
        float minBodyDist = float.MaxValue;
        foreach (var body in bodiesInRange)
        {
            if (body != null && body.enabled && body.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(myPos, body.transform.position);
                if (dist < minBodyDist) { minBodyDist = dist; closestBody = body; }
            }
        }

        // 3. OLTÁROK
        for (int i = altarsInRange.Count - 1; i >= 0; i--)
            if (altarsInRange[i] == null || altarsInRange[i].gameObject == null) altarsInRange.RemoveAt(i);

        AltarTrigger closestAltar = null;
        float minAltarDist = float.MaxValue;
        foreach (var altar in altarsInRange)
        {
            if (altar != null && altar.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(myPos, altar.transform.position);
                if (dist < minAltarDist) { minAltarDist = dist; closestAltar = altar; }
            }
        }

        // 4. LÁDÁK
        for (int i = chestsInRange.Count - 1; i >= 0; i--)
            if (chestsInRange[i] == null || chestsInRange[i].gameObject == null) chestsInRange.RemoveAt(i);

        TreasureChest closestChest = null;
        float minChestDist = float.MaxValue;
        foreach (var chest in chestsInRange)
        {
            if (chest != null && chest.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(myPos, chest.transform.position);
                if (dist < minChestDist) { minChestDist = dist; closestChest = chest; }
            }
        }

        // 5. STORY TÁRGYAK (Palack)
        for (int i = storyItemsInRange.Count - 1; i >= 0; i--)
            if (storyItemsInRange[i] == null || storyItemsInRange[i].gameObject == null) storyItemsInRange.RemoveAt(i);

        StoryItemPickup closestStory = null;
        float minStoryDist = float.MaxValue;
        foreach (var story in storyItemsInRange)
        {
            if (story != null && story.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(myPos, story.transform.position);
                if (dist < minStoryDist) { minStoryDist = dist; closestStory = story; }
            }
        }

        // --- DÖNTÉS ---
        
        nearbyItemPickup = null;
        nearbyLootBody = null;
        nearbyAltar = null;
        nearbyChest = null;
        nearbyStoryItem = null;
        IHighlight targetHighlight = null;

        // Kiszámoljuk a legkisebb távolságot az 5 közül
        float distItem = (closestItem != null) ? minItemDist : float.MaxValue;
        float distBody = (closestBody != null) ? minBodyDist : float.MaxValue;
        float distAltar = (closestAltar != null) ? minAltarDist : float.MaxValue;
        float distChest = (closestChest != null) ? minChestDist : float.MaxValue;
        float distStory = (closestStory != null) ? minStoryDist : float.MaxValue;

        float winnerDist = Mathf.Min(distItem, Mathf.Min(distBody, Mathf.Min(distAltar, Mathf.Min(distChest, distStory))));

        if (winnerDist == float.MaxValue) 
        {
            if (currentHighlightedObject != null) {
                if (currentHighlightedObject as Object != null) currentHighlightedObject.Unhighlight();
                currentHighlightedObject = null;
            }
            return; 
        }

        // Kiválasztjuk a nyertest
        if (winnerDist == distItem) nearbyItemPickup = closestItem;
        else if (winnerDist == distBody) nearbyLootBody = closestBody;
        else if (winnerDist == distAltar) nearbyAltar = closestAltar;
        else if (winnerDist == distChest) nearbyChest = closestChest;
        else if (winnerDist == distStory) nearbyStoryItem = closestStory;

        // Highlight kezelés
        if (nearbyItemPickup != null) targetHighlight = nearbyItemPickup.GetComponent<IHighlight>();
        else if (nearbyLootBody != null) targetHighlight = nearbyLootBody.GetComponent<IHighlight>();
        else if (nearbyAltar != null) targetHighlight = nearbyAltar.GetComponent<IHighlight>();
        else if (nearbyChest != null) targetHighlight = nearbyChest.GetComponent<IHighlight>();
        else if (nearbyStoryItem != null) targetHighlight = nearbyStoryItem.GetComponent<IHighlight>();

        if (currentHighlightedObject != targetHighlight)
        {
            if (currentHighlightedObject as Object != null) currentHighlightedObject.Unhighlight();
            
            if (targetHighlight != null)
            {
                targetHighlight.Highlight();
                currentHighlightedObject = targetHighlight;
            }
        }
    }

    // --- TRIGGEREK ---

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ItemPickup pickup))
            if (!itemsInRange.Contains(pickup)) itemsInRange.Add(pickup);

        if (other.TryGetComponent(out LootableBody body))
            if (!bodiesInRange.Contains(body)) bodiesInRange.Add(body);
        
        if (other.TryGetComponent(out AltarTrigger altar))
            if (!altarsInRange.Contains(altar)) altarsInRange.Add(altar);

        if (other.TryGetComponent(out TreasureChest chest))
            if (!chestsInRange.Contains(chest)) chestsInRange.Add(chest);
            
        if (other.TryGetComponent(out StoryItemPickup story))
            if (!storyItemsInRange.Contains(story)) storyItemsInRange.Add(story);

        if (other.TryGetComponent(out NPCInteraction npc))
            nearbyNPC = npc;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ItemPickup pickup))
        {
            if (itemsInRange.Contains(pickup)) itemsInRange.Remove(pickup);
            if (pickup == nearbyItemPickup) nearbyItemPickup = null;
        }

        if (other.TryGetComponent(out LootableBody body))
        {
            if (bodiesInRange.Contains(body)) bodiesInRange.Remove(body);
            if (body == nearbyLootBody) nearbyLootBody = null;
        }

        if (other.TryGetComponent(out AltarTrigger altar))
        {
            if (altarsInRange.Contains(altar)) altarsInRange.Remove(altar);
            if (altar == nearbyAltar) nearbyAltar = null;
        }

        if (other.TryGetComponent(out TreasureChest chest))
        {
            if (chestsInRange.Contains(chest)) chestsInRange.Remove(chest);
            if (chest == nearbyChest) nearbyChest = null;
        }

        if (other.TryGetComponent(out StoryItemPickup story))
        {
            if (storyItemsInRange.Contains(story)) storyItemsInRange.Remove(story);
            if (story == nearbyStoryItem) nearbyStoryItem = null;
        }

        if (other.GetComponent<NPCInteraction>() != null)
            nearbyNPC = null;
    }
}