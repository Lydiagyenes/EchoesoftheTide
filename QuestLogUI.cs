using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections; 
using GDS.Core; // Ez kell a ListSlot típushoz!

// Figyelj rá, hogy az osztály neve (QuestTrackerUI vagy QuestLogUI) 
// pontosan egyezzen a fájlod nevével!
public class QuestTrackerUI : MonoBehaviour 
{
    public TextMeshProUGUI trackerText; 

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(SubscribeToEvents());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (QuestLog.Instance != null)
        {
            QuestLog.Instance.OnQuestLogUpdated -= UpdateTracker;
        }

        // Biztonságos leiratkozás az Inventory változásokról
        if (InventoryManager.Instance != null && InventoryManager.Instance.MainInventory != null)
        {
            InventoryManager.Instance.MainInventory.Data.OnChange -= OnInventoryChanged;
        }
    }

    private IEnumerator SubscribeToEvents()
    {
        // 1. Feliratkozunk a QuestLog-ra
        while (QuestLog.Instance == null) yield return null;
        QuestLog.Instance.OnQuestLogUpdated += UpdateTracker;
        
        // 2. Megvárjuk, amíg az InventoryManager és a táska (MainInventory) feláll
        while (InventoryManager.Instance == null || InventoryManager.Instance.MainInventory == null) yield return null;
        
        // 3. A legbiztosabb módszer: rákötünk a táska belső változás-figyelőjére!
        // Ez pontosan ugyanaz, amit a vizuális Inventory-d is használ.
        InventoryManager.Instance.MainInventory.Data.OnChange += OnInventoryChanged;

        UpdateTracker();
    }

    // Ezt hívja meg a táska, ha BÁRMI történik benne (felvétel, eldobás, craftolás, evés)
    private void OnInventoryChanged(System.Collections.Generic.IReadOnlyList<GDS.Core.ListSlot> slots)
    {
        UpdateTracker(); 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (trackerText == null) return;
        
        if (scene.name == "MainMenu_Scene" || scene.name == "Manager_Scene" || scene.name == "Intro_Scene" || scene.name == "Loading_Scene") 
            trackerText.gameObject.SetActive(false);
        else
        {
            trackerText.gameObject.SetActive(true);
            UpdateTracker();
        }
    }

    void UpdateTracker()
    {
        if (trackerText == null) return;
        
        // Menükben ne frissítsen
        if (SceneManager.GetActiveScene().name == "MainMenu_Scene" || 
            SceneManager.GetActiveScene().name == "Intro_Scene") return;

        trackerText.text = "<b>Jelenlegi feladatok:</b>\n"; 

        if (QuestLog.Instance != null)
        {
            if (QuestLog.Instance.activeQuests.Count == 0)
            {
                trackerText.text += "<i>Nincs aktív feladat.</i>"; 
                return;
            }

            foreach (Quest q in QuestLog.Instance.activeQuests)
            {
                trackerText.text += $"- <color=yellow>{q.questName}</color>\n";
                
                // Részfeladatok listázása
                foreach(var obj in q.objectives)
                {
                    if (obj.type == QuestType.CollectItem)
                    {
                        // Biztonsági ellenőrzés az indulás pillanatára
                        int current = 0;
                        if (InventoryManager.Instance != null && InventoryManager.Instance.MainInventory != null)
                        {
                            current = InventoryManager.Instance.GetItemAmount(obj.targetID);
                        }

                        // Kijelezzük a számot (pl: 1/5)
                        trackerText.text += $"  • {obj.objectiveDescription} ({current}/{obj.requiredAmount})\n";
                    }
                    else
                    {
                         trackerText.text += $"  • {obj.objectiveDescription}\n";
                    }
                }
                trackerText.text += "\n";
            }
        }
    }
}