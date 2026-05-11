using UnityEngine;

public class NarratorTrigger : MonoBehaviour
{[Header("Mit mondjon?")]
    public DialogueLine lineToSay;[Header("Quest (Opcionális)")]
    public Quest questToStartAfterDialogue; 

    [Header("Beállítások")]
    public bool playOnce = true;     
    public bool playOnStart = false; 

    private bool hasPlayed = false;

    private void Start()
    {
        if (playOnStart) TriggerDialogue();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playOnStart) 
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (playOnce && hasPlayed) return;
        hasPlayed = true;

        // BIZTOSÍTÉK: Ha csak egyszer játszható le, kikapcsoljuk a zónát, 
        // így fizikai képtelenség másodszor is belépni!
        if (playOnce)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
        
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.PlayDialogue(lineToSay, () => 
            {
                if (questToStartAfterDialogue != null && QuestLog.Instance != null)
                {
                    QuestLog.Instance.AddQuest(questToStartAfterDialogue);
                }
            });
        }
    }
}