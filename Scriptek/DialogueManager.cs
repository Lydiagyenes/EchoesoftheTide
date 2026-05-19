using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elemek")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public GameObject optionsContainer;
    public GameObject optionButtonPrefab;
    
    // Publikus, hogy a NearbyAction lássa!
    public bool isDialogueActive = false; 

    private Queue<DialogueNode> nodesQueue; 
    private DialogueNode currentNode;
    private NPCInteraction currentNPC;
    private List<GameObject> activeButtons = new List<GameObject>();

    private void Awake()
    {
       // if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        nodesQueue = new Queue<DialogueNode>();
    }
    
    void Start()
{
    // BIZTONSÁGI ELLENŐRZÉS
    if (dialoguePanel != null) 
    {
        dialoguePanel.SetActive(false);
    }
    else
    {
        Debug.LogWarning("DialogueManager: A dialoguePanel nincs hozzárendelve az Inspectorban!");
    }

    if (optionButtonPrefab != null) optionButtonPrefab.SetActive(false);
}

    // --- ÚJ RÉSZ: E BETŰ FIGYELÉSE ---
    private void Update()
    {
        // Csak akkor lépünk, ha aktív a dialógus, megnyomtuk az E-t, 
        // ÉS nincsenek válaszlehetőségek (mert ha vannak gombok, akkor kattintani kell!)
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            if (activeButtons.Count == 0)
            {
                DisplayNextNode();
            }
        }
    }
    
    // Ezt a függvényt hívja az 'R' gomb NPC-knél
    public void ContinueDialogue()
    {
        DisplayNextNode();
    }

    public void StartDialogue(Dialogue dialogue, NPCInteraction npc)
    {
         if (dialogue == null || dialogue.nodes == null || dialogue.nodes.Length == 0)
        {
            Debug.LogWarning("Üres dialógust próbáltál indítani!");
            return;
        }
        // --- VÁLTOZÁS: Aktív állapot beállítása ---
        isDialogueActive = true;
        currentNPC = npc;
        dialoguePanel.SetActive(true);
        
        // Mozgás tiltása
        var movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null) movement.canMove = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        nodesQueue.Clear();
        // Feltöltjük a sorba a dialógus csomópontjait
        foreach (var node in dialogue.nodes)
        {
            nodesQueue.Enqueue(node);
        }

        DisplayNextNode();
    }

     private void DisplayNextNode()
    {
        if (nodesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueNode nextNode = nodesQueue.Dequeue();
        DisplayNodeContent(nextNode);
    }

    private void DisplayNodeContent(DialogueNode node)
    {
        // Ha van Speaker Name a node-ban, azt használjuk, ha nincs, akkor az NPC nevét (ha van NPC)
        if (!string.IsNullOrEmpty(node.speakerName))
            nameText.text = node.speakerName;
        else if (currentNPC != null)
            nameText.text = currentNPC.npcName;
        else
            nameText.text = ""; // Oltárnál üres maradhat, vagy beállíthatsz alapértelmezettet

        dialogueText.text = node.sentence;
        
        // Gombok törlése
        foreach (GameObject button in activeButtons) Destroy(button);
        activeButtons.Clear();
        
        optionsContainer.SetActive(false);

        // Válaszlehetőségek generálása
        if (node.playerResponses != null && node.playerResponses.Length > 0)
        {
            nodesQueue.Clear(); // Ha elágazáshoz értünk, töröljük a lineáris sort
            optionsContainer.SetActive(true);
            
            for (int i = 0; i < node.playerResponses.Length; i++)
            {
                GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer.transform);
                buttonGO.SetActive(true);
                buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = node.playerResponses[i].responseText;
                
                int responseIndex = i;
                DialogueNode currentNodeRef = node; // Closure capture miatt
                
                buttonGO.GetComponent<Button>().onClick.AddListener(() => {
                    ChooseResponse(currentNodeRef, responseIndex);
                });
                activeButtons.Add(buttonGO);
            }
        }
    }

    private void ChooseResponse(DialogueNode node, int index)
    {
        PlayerResponse response = node.playerResponses[index];

        // 1. QUEST KEZELÉS
        if (response.questToStart != null)
        {
            QuestLog.Instance.AddQuest(response.questToStart);
        }

        // 2. DÖNTÉS KEZELÉS
        if (!string.IsNullOrEmpty(response.decisionID))
        {
            QuestLog.Instance.AddDecision(response.decisionID);
        }

        // 3. DIALÓGUS LÉPTETÉS
        if (response.nextDialogue != null)
        {
            StartDialogue(response.nextDialogue, currentNPC);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        // --- VÁLTOZÁS: Aktív állapot kikapcsolása ---
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = true;

        // Kurzor visszaállítása játék módba
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // --- VÁLTOZÁS: Null check, mert az Oltár nem NPC! ---
        if (currentNPC != null) 
        {
            currentNPC.isTalking = false; 
        }
    }
}