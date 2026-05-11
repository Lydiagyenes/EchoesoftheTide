using UnityEngine;

public class QuestObjectActivator : MonoBehaviour
{
    [Header("Beállítások")]
    public Quest questToWatch; // A figyelt küldetés
    
    [Tooltip("Ezt az objektumot fogjuk ki/be kapcsolni.")]
    public GameObject objectToControl; 

    [Tooltip("Ha kész a quest: True = Megjelenjen, False = Eltűnjön")]
    public bool activateOnComplete = true;

    private bool debugLogged = false; 

    void Update()
    {
        // Ha nincs beállítva célpont, nem tudunk mit tenni
        if (objectToControl == null || QuestLog.Instance == null || questToWatch == null) return;

        bool isCompleted = CheckIfQuestCompleted();

        // Állapot frissítése a CÉLPONTON (objectToControl)
        if (isCompleted)
        {
            if (objectToControl.activeSelf != activateOnComplete)
            {
                objectToControl.SetActive(activateOnComplete);
                Debug.Log($"[QuestActivator] SIKER! A '{questToWatch.questID}' kész, a kapu állapota mostantól: {activateOnComplete}");
            }
        }
        else
        {
            // Ha még nincs kész...
            if (objectToControl.activeSelf == activateOnComplete)
            {
                objectToControl.SetActive(!activateOnComplete);
                
                if (!debugLogged)
                {
                    Debug.Log($"[QuestActivator] Futok és várakozom... Figyelt Quest: '{questToWatch.questID}'. Célpont: {objectToControl.name}");
                    debugLogged = true;
                }
            }
        }
    }

    private bool CheckIfQuestCompleted()
    {
        foreach (Quest q in QuestLog.Instance.completedQuests)
        {
            if (q.questID.Trim() == questToWatch.questID.Trim()) 
                return true;
        }
        return false;
    }
}