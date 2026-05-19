using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    public DialogueNode[] nodes; 
}

[System.Serializable]
public class DialogueNode
{
    public string speakerName;
    [TextArea(3, 10)]
    public string sentence;
    public PlayerResponse[] playerResponses; 
}

[System.Serializable]
public class PlayerResponse
{
    public string responseText;

    [Header("Következmények")]
    // 1. JAVÍTÁS: Nem Node-ra, hanem komplett Dialógus fájlra hivatkozunk
    public Dialogue nextDialogue; 

    // 2. JAVÍTÁS: Nem Eventet használunk, hanem közvetlen Quest referenciát
    public Quest questToStart; 

    // 3. JAVÍTÁS: Ha csak döntést kell tárolni (pl. "elias_refused")
    public string decisionID; 
}