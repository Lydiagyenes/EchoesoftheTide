using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Linq; 

// FONTOS: Rajta van az ISaveable
public class QuestLog : MonoBehaviour, ISaveable 
{
    public static QuestLog Instance { get; private set; }

    [Header("UI Referencia")]
    public GameObject questLogWindow; 

    [Header("Adatbázis")][Tooltip("Húzd be ide az összes Quest ScriptableObject-et!")]

    public List<string> unlockedJournalEntries = new List<string>();
    public List<Quest> allQuestsDatabase = new List<Quest>();

    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    public List<string> storyDecisions = new List<string>();

    public event System.Action OnQuestLogUpdated;

    private void Awake()
    {
        // 1. JAVÍTÁS: Nincs Destroy(gameObject), csak szimpla referencia!
        // A SystemAnchor úgyis megvédi a duplikációtól.
        Instance = this;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu_Scene" && questLogWindow != null) 
        {
            questLogWindow.SetActive(false);
        }
    }

    public void AddQuest(Quest newQuest)
    {
        if (!activeQuests.Contains(newQuest) && !completedQuests.Contains(newQuest))
        {
            activeQuests.Add(newQuest);
            Debug.Log("Új küldetés: " + newQuest.questName);
            OnQuestLogUpdated?.Invoke(); 
            
            if(newQuest.autoComplete) CheckAutoCompleteQuests();
        }
    }

// Ezt hívjuk meg, amikor a játékos elolvas egy papírt
    public void UnlockJournalEntry(string entryID)
    {
        if (!unlockedJournalEntries.Contains(entryID))
        {
            unlockedJournalEntries.Add(entryID);
            Debug.Log($"<color=cyan>[QuestLog] Új naplóbejegyzés véglegesen feloldva: {entryID}</color>");
            OnQuestLogUpdated?.Invoke(); // Szólunk a UI-nak, hogy frissítsen
        }
    }

    // A Napló UI ezt a függvényt fogja kérdezni, hogy megmutassa-e a szöveget!
    public bool HasJournalEntry(string entryID)
    {
        return unlockedJournalEntries.Contains(entryID);
    }
    public bool CheckAndCompleteQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest)) return false;

        // 1. LÉPÉS: ELLENŐRZÉS (Van-e mindenből elég?)
        foreach (var obj in quest.objectives)
        {
            if (obj.type == QuestType.CollectItem)
            {
                int currentAmount = InventoryManager.Instance.GetItemAmount(obj.targetID);
                if (currentAmount < obj.requiredAmount)
                {
                    // Ha akár egyből is hiány van, nem teljesíthető
                    return false; 
                }
            }
        }

        // 2. LÉPÉS: LEVONÁS (A VARÁZSLAT ITT TÖRTÉNIK)
        // Csak akkor vonjuk le a tárgyakat a táskából, ha a Quest-en be van pipálva a levonás!
        if (quest.removeItemsOnCompletion)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == QuestType.CollectItem)
                {
                    InventoryManager.Instance.RemoveItems(obj.targetID, obj.requiredAmount);
                }
            }
        }

        // 3. LÉPÉS: TELJESÍTÉS
        CompleteQuest(quest);
        return true;
    }

    public void CheckAutoCompleteQuests()
    {
        List<Quest> currentQuests = new List<Quest>(activeQuests);
        foreach (var quest in currentQuests)
        {
            if (quest.autoComplete)
            {
                CheckAndCompleteQuest(quest);
            }
        }
    }

    public void CompleteQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            Debug.Log($"<color=green>KÜLDETÉS TELJESÍTVE: {quest.questName}</color>");
            
            if (quest.rewardSkillPoints > 0 && SkillManager.Instance != null)
            {
                SkillManager.Instance.AddSkillPoint(quest.rewardSkillPoints);
            }

            if (!string.IsNullOrEmpty(quest.rewardItemID) && InventoryManager.Instance != null)
            {
                GDS.Core.ItemBase rewardItem = InventoryManager.Instance.FindItemBaseByID(quest.rewardItemID);

                if (rewardItem != null)
                {
                    InventoryManager.Instance.AddItemToInventory(rewardItem, quest.rewardItemAmount);
                }
            }

            if (quest.nextQuests != null && quest.nextQuests.Count > 0)
                {
                    foreach (Quest nextQ in quest.nextQuests)
                    {
                        if (nextQ != null)
                        {
                            AddQuest(nextQ); // A függvény automatikusan hozzáadja és szól a UI-nak
                        }
                    }
                }

            // --- INTRO QUEST FELOLDÁS ---
            if (quest.questID == "02_ship_1") 
            {
                UIInputController uiController = FindFirstObjectByType<UIInputController>();
                if (uiController != null) 
                {
                    uiController.UnlockMenus();
                }
            }

            OnQuestLogUpdated?.Invoke();
        }
    }

    public void AddDecision(string decisionID)
    {
        if (!storyDecisions.Contains(decisionID))
            storyDecisions.Add(decisionID);
    }
    
    public bool HasDecision(string decisionID) => storyDecisions.Contains(decisionID);

    // ==========================================
    // --- MENTÉS ÉS BETÖLTÉS (ISaveable) ---
    // ==========================================

    public void SaveData(ref GameData data)
    {
        data.activeQuestIDs.Clear();
        data.completedQuestIDs.Clear();
        data.storyDecisions.Clear();
        data.unlockedJournalEntries = new List<string>(unlockedJournalEntries);
        foreach (var quest in activeQuests)
        {
            if (quest != null) data.activeQuestIDs.Add(quest.questID);
        }

        foreach (var quest in completedQuests)
        {
            if (quest != null) data.completedQuestIDs.Add(quest.questID);
        }

        data.storyDecisions = new List<string>(storyDecisions);
    }

    public void LoadData(GameData data)
    {
        activeQuests.Clear();
        completedQuests.Clear();
        storyDecisions.Clear();

        // BIZTONSÁGI HÁLÓ: Betöltjük a Resources/Quests mappából is a Questeket,
        // arra az esetre, ha lefelejtetted volna őket az Inspectorban a listáról!
        // (Ehhez kell, hogy a questjeid egy "Resources/Quests" nevű mappában legyenek a Unity-n belül!)
        Quest[] fallbackQuests = Resources.LoadAll<Quest>("Quests");

        foreach (string id in data.activeQuestIDs)
        {
            Quest questObj = FindQuestSafely(id, fallbackQuests);
            if (questObj != null) activeQuests.Add(questObj);
            else Debug.LogError($"[QuestLog] HIBA: Nem találom a mentett questet: {id}");
        }

        foreach (string id in data.completedQuestIDs)
        {
            Quest questObj = FindQuestSafely(id, fallbackQuests);
            if (questObj != null) completedQuests.Add(questObj); 
        }

        storyDecisions = new List<string>(data.storyDecisions);

        if (data.unlockedJournalEntries != null)
        {
            unlockedJournalEntries = new List<string>(data.unlockedJournalEntries); // Ezt a sort add hozzá!
        }

        // Késleltetjük a UI frissítést 0.1 másodperccel!
        // Miért? Hogy megvárjuk, amíg az InventoryManager is betölti a maga LoadData-ját,
        // különben a UI 0 fát fog kiírni!
        Invoke(nameof(RefreshUI), 0.1f);
    }

    private void RefreshUI()
    {
        OnQuestLogUpdated?.Invoke();
    }

    private Quest FindQuestSafely(string id, Quest[] fallbackQuests)
    {
        // 1. Először megnézzük az Inspectorban behúzott listában (ez a leggyorsabb)
        Quest foundQuest = allQuestsDatabase.FirstOrDefault(q => q != null && q.questID == id);

        // 2. Ha ott nincs, megnézzük a Resources/Quests mappában lévő biztonsági másolatokban
        if (foundQuest == null && fallbackQuests != null)
        {
            foundQuest = fallbackQuests.FirstOrDefault(q => q != null && q.questID == id);
        }

        // 3. Ha sehol sem találjuk, szólunk a konzolon (hogy könnyebb legyen a hibakeresés)
        if (foundQuest == null)
        {
            Debug.LogWarning($"[QuestLog] Nem található '{id}' azonosítójú küldetés sem az Inspector adatbázisában, sem a Resources mappában!");
        }

        return foundQuest;
    }
}