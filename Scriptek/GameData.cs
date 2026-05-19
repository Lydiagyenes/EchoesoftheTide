using UnityEngine;
using System;
using System.Collections.Generic; 

[Serializable]
public class GameData
{
    // --- ALAP ADATOK ---
    public float currentHealth;
    public float currentStamina;
    public string lastSceneName;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public bool journalTutorialPlayed = false; 
     public string lastSaveTime; 
     public List<string> openedChestIDs; 
     public List<string> lootedChestIDs;
      public bool areMenusUnlocked = false; 

    // --- LISTÁK ---
    public List<string> collectedPickupIDs; 
    public List<string> activeQuestIDs;
    public List<string> activatedAltars;
    public List<string> completedQuestIDs;
    public List<string> storyDecisions;
     public List<int> seenTutorialTabs; 
     public List<string> unlockedJournalEntries;


    // --- SKILL RENDSZER ---
    public int skillPoints = 0; 
    public List<string> unlockedSkillIDs = new List<string>(); 

    // --- ÚJ: INVENTORY TARTALMA ---
    // Ez a kis segédosztály tárolja egy darab tárgy adatait a mentésben
    [System.Serializable]
    public struct SavedItemData
    {
        public string itemID;    // Mi az? (pl. "verfurt")
        public int amount;       // Mennyi? (pl. 5)
        public int slotIndex;    // Hol van? (melyik kockában)
         // ÚJ:
        public float currentDurability; 
    }
    
    // Ez pedig a lista, ami az összes tárgyat tárolja
    public List<SavedItemData> inventoryContents; 
    // ----------------------------------

    public GameData()
    {
        this.lastSceneName = "The_Viking_Village";
        this.playerPosition = new Vector3(-40f, 4f, 34f);
        this.playerRotation = Quaternion.identity;
        journalTutorialPlayed = false;
        this.currentHealth = 100f;
        this.currentStamina = 100f;

        // Listák inicializálása (hogy ne legyenek null hibák)
        this.collectedPickupIDs = new List<string>();
        this.activeQuestIDs = new List<string>();
        this.completedQuestIDs = new List<string>();
        this.storyDecisions = new List<string>();
        this.unlockedSkillIDs = new List<string>();
         this.openedChestIDs = new List<string>();
         this.lootedChestIDs = new List<string>();
        this.areMenusUnlocked = false;
        this.seenTutorialTabs = new List<int>(); 
         this.lastSaveTime = "";
        // ÚJ LISTA INICIALIZÁLÁSA
        this.inventoryContents = new List<SavedItemData>();
         activatedAltars = new List<string>();
          this.unlockedJournalEntries = new List<string>();
    }
}