using UnityEngine;

public class QuestPhaseObject : MonoBehaviour
{
    public enum PhaseMode
    {
        Visibility,     // Eltűnik/Megjelenik (Pl. a Palack)
        Interactability // Látszik, de nem kattintható (Pl. a Kőoltár)
    }

    [Header("Beállítások")]
    [Tooltip("Melyik Questnek kell AKTÍVNAK lennie?")]
    public Quest requiredQuest;

    [Tooltip("Hogyan viselkedjen?")]
    public PhaseMode mode = PhaseMode.Visibility;

    private Collider[] myColliders;
    private Renderer[] myRenderers;
    private bool isActivated = false;

    void Awake()
    {
        // Összeszedjük a komponenseket
        myColliders = GetComponentsInChildren<Collider>();
        myRenderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        // Induláskor ellenőrizzük az állapotot
        CheckState();
    }

    void Update()
    {
        // Ha még nincs aktiválva, folyamatosan figyeljük, hogy elindult-e a quest
        if (!isActivated)
        {
            CheckState();
        }
    }

    void CheckState()
    {
        if (QuestLog.Instance == null || requiredQuest == null) return;

        // Ellenőrizzük, hogy a quest AKTÍV-e (tehát elindult, de még nem végeztük el)
        // Vagy ha már befejeztük, akkor is maradjon ott? (Általában igen).
        bool isActive = QuestLog.Instance.activeQuests.Contains(requiredQuest) || 
                        QuestLog.Instance.completedQuests.Contains(requiredQuest);

        if (isActive)
        {
            ActivateObject();
        }
        else
        {
            DeactivateObject();
        }
    }

    void ActivateObject()
    {
        if (isActivated) return;
        isActivated = true;

        // Minden visszakapcsolunk
        foreach (var col in myColliders) col.enabled = true;
        foreach (var rend in myRenderers) rend.enabled = true;
    }

    void DeactivateObject()
    {
        // Ha Visibility mód: Kikapcsoljuk a látványt és a fizikát is
        if (mode == PhaseMode.Visibility)
        {
            foreach (var rend in myRenderers) rend.enabled = false;
            foreach (var col in myColliders) col.enabled = false;
        }
        // Ha Interactability mód: Csak a fizikát (kattinthatóságot) kapcsoljuk ki, a látvány marad
        else if (mode == PhaseMode.Interactability)
        {
            foreach (var col in myColliders) col.enabled = false;
            // A Renderer marad enabled!
        }
    }
}