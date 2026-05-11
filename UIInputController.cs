using UnityEngine;

public class UIInputController : MonoBehaviour, ISaveable
{
    [Header("Követelmények")]
    [Tooltip("Ennek a Questnek kell kész lennie, hogy megnyíljanak a menük.")]
    public string requiredQuestID = "02_ship_1";

   [Header("UI Referenciák")]
    public GameObject inventoryPanel; 
    public GameObject journalPanel;   
    public GameObject minimapUI;      

    // A menük állapota
    private bool isInventoryOpen = false;
    private bool isJournalOpen = false;

    // --- ÚJ: Ezt fogjuk menteni és betölteni! ---
    public bool areMenusUnlocked = false; 

    private void Start()
    {
        if (journalPanel != null) journalPanel.SetActive(false);
        isJournalOpen = false;
        
        // Induláskor a minimap állapota attól függ, fel van-e oldva
        if (minimapUI != null) minimapUI.SetActive(areMenusUnlocked);

         if (QuestLog.Instance != null)
        {
            QuestLog.Instance.OnQuestLogUpdated += CheckUnlockCondition;
            CheckUnlockCondition(); // Azonnali ellenőrzés indításkor/betöltéskor
        }
    }
     private void OnDestroy()
    {
        if (QuestLog.Instance != null)
        {
            QuestLog.Instance.OnQuestLogUpdated -= CheckUnlockCondition;
        }
    }

    void Update()
    {
       // if (GameManager.isPaused) return; 

        // Csak akkor figyeljük a gombokat, ha már fel vannak oldva!
        if (areMenusUnlocked)
        {
            // --- INVENTORY (TAB vagy I) ---
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            {
                if (GameManager.isPaused && !isInventoryOpen) return;
                ToggleInventory();
            }

            // --- JOURNAL / NAPLÓ (J) ---
            if (Input.GetKeyDown(KeyCode.J))
            { if (GameManager.isPaused && !isJournalOpen) return;
                ToggleJournal();
            }
        }
        else
        {
            // Ha a játékos próbálkozik, de még nincs feloldva (Opcionális üzenet)
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("A menük még nincsenek feloldva!");
            }
        }
    }

    // --- EZT A FÜGGVÉNYT HÍVJUK A QUEST VÉGÉN! ---
    public void UnlockMenus()
    {
        areMenusUnlocked = true;
        
        // A minimap azonnal megjelenik
        if (minimapUI != null) minimapUI.SetActive(true); 
        
        Debug.Log("<color=yellow>[UIInputController] Menük és Minimap FELOLDVA!</color>");
    }

    // --- SEGÉDFÜGGVÉNYEK ---

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        // Ha kinyitjuk az inventoryt, a naplót bezárjuk
        if (isInventoryOpen) 
        {
            isJournalOpen = false;
            if (journalPanel != null) journalPanel.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
            
            // Szólunk a GameManagernek
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetInventoryState(isInventoryOpen);
            }
            
            Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isInventoryOpen;
        }
    }

    private void ToggleJournal()
    {
        isJournalOpen = !isJournalOpen;

        // Ha kinyitjuk a naplót, az inventoryt bezárjuk
        if (isJournalOpen)
        {
            isInventoryOpen = false;
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            
            if (GameManager.Instance != null) GameManager.Instance.SetInventoryState(false);
        }
        
        if (journalPanel != null)
        {
            journalPanel.SetActive(isJournalOpen);
            
            Cursor.lockState = isJournalOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isJournalOpen;
        }
    }

    // ==========================================
    // --- MENTÉS ÉS BETÖLTÉS (ISaveable) ---
    // ==========================================

    public void SaveData(ref GameData data)
    {
        // Rögzítjük a GameData-ban az állapotot
        data.areMenusUnlocked = this.areMenusUnlocked;
    }

    public void LoadData(GameData data)
    {
        // Kiolvassuk a GameData-ból az állapotot
        this.areMenusUnlocked = data.areMenusUnlocked;
        
        // Frissítjük a minimapot (ha fel volt oldva, látszik, ha nem, eltűnik)
        if (minimapUI != null) minimapUI.SetActive(this.areMenusUnlocked);

        // Biztonsági reset: Töltéskor minden ablak legyen zárva
        isInventoryOpen = false;
        isJournalOpen = false;
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (journalPanel != null) journalPanel.SetActive(false);
    }

    // EZ A FÜGGVÉNY FUT LE, HA EGY QUEST BEFEJEZŐDIK VAGY BETÖLT A JÁTÉK!
    private void CheckUnlockCondition()
    {
        // Ha már fel van oldva, nem kell tovább keresgélni
        if (areMenusUnlocked) return;

        if (QuestLog.Instance != null)
        {
            foreach (var q in QuestLog.Instance.completedQuests)
            {
                if (q != null)
                {
                    // --- NYOMOZÓ LOG ---
                    Debug.Log($"<color=orange>[UIInputController] Ellenőrzés: A teljesített quest ID-ja: '{q.questID}'. Én ezt a questet várom: '{requiredQuestID}'. Egyezik?</color>");

                    if (q.questID.Trim() == requiredQuestID.Trim())
                    {
                        UnlockMenus();
                        break;
                    }
                }
            }
        }
    }
}
