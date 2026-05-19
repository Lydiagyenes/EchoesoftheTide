using UnityEngine;
using System.Collections.Generic; // Ez KELL a Listákhoz!

public class NPCInteraction : MonoBehaviour
{
    [Header("Alap beállítások")]
    public Dialogue defaultDialogue; // Ez az Intro (amikor először találkozol vele)
    public string npcName = "Névtelen NPC"; 
    
    [HideInInspector]
    public bool isTalking = false;

    // Létrehozunk egy egyszerű adatszerkezetet a párosításhoz
    [System.Serializable] 
    public struct QuestDialogueEntry
    {
        public string description;      // Csak emlékeztetőnek (pl. "Ha felvette a sírásást")
        public Quest questToCheck;      // Melyik questet figyeljük?
        public Dialogue dialogueToPlay; // Mit mondjon, ha ez a quest aktív?
    }

    [Header("Változó szövegek (Sorrend számít!)")]
    public List<QuestDialogueEntry> conditionalDialogues;

    public void TriggerDialogue()
    {
        if (!isTalking)
        {
            isTalking = true;

            // 1. Alapból az Intro-t készítjük be
            Dialogue dialogueToStart = defaultDialogue;

            // 2. Megnézzük a listát: Van olyan quest a játékosnál, amihez külön szöveg tartozik?
            // Fontos: Ha QuestLog még nem létezik (pl. tesztelésnél), ne fagyjon le
            if (QuestLog.Instance != null)
            {
                foreach (var entry in conditionalDialogues)
                {
                    // Ellenőrizzük, hogy AKTÍV-e a quest
                    if (QuestLog.Instance.activeQuests.Contains(entry.questToCheck))
                    {
                        dialogueToStart = entry.dialogueToPlay;
                        // Ha találtunk egyezést, kilépünk a ciklusból (az első találat nyer)
                        break; 
                    }
                    
                    // KÉSŐBB IDE JÖHET: 
                    // else if (QuestLog.Instance.completedQuests.Contains(entry.questToCheck)) { ... }
                }
            }

            // 3. Elindítjuk a kiválasztott dialógust
            DialogueManager.Instance.StartDialogue(dialogueToStart, this);
        }
    }
}