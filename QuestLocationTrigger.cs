using UnityEngine;

public class QuestLocationTrigger : MonoBehaviour
{[Header("Mit fejezzen be?")]
    public Quest questToComplete;[Header("Mit indítson el?")]
    public Quest questToStart;[Header("Opcionális: Mondjon is valamit?")]
    public DialogueLine locationMonologue; 

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            // BIZTOSÍTÉK: Azonnal kikapcsoljuk az érzékelőt!
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            if (questToComplete != null && QuestLog.Instance != null)
            {
                if (QuestLog.Instance.activeQuests.Contains(questToComplete))
                {
                    QuestLog.Instance.CompleteQuest(questToComplete);
                }
            }

            if (questToStart != null && QuestLog.Instance != null)
            {
                QuestLog.Instance.AddQuest(questToStart);
            }

            if (!string.IsNullOrEmpty(locationMonologue.text) && SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.PlayDialogue(locationMonologue);
            }
            
            // 0.1 másodperc múlva végleg töröljük az objektumot
            Destroy(gameObject, 0.1f); 
        }
    }
}