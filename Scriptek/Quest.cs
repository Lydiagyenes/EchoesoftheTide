using UnityEngine;
using System.Collections.Generic;

public enum QuestType
{
    TalkToNPC,
    CollectItem,
    ReachLocation,
    KillEnemy // Ha később lesz harc alapú quest
}

// Ez egyetlen részfeladatot ír le (pl. "Gyűjts 5 követ")
[System.Serializable]
public class QuestObjective
{
    public string objectiveDescription; // Pl. "Gyűjts követ"
    public QuestType type;
    public string targetID;       // Pl. "Stone" vagy "Wolf"
    public int requiredAmount = 1;
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea(3, 10)]
    public string description;

    [Header("Feladatok")]
    // Most már listánk van, tehát egy questhez több dolog is kellhet!
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [Header("Logika")]
    [Tooltip("Ha ez be van pipálva, a quest azonnal befejeződik, amint a feltételek teljesülnek (nem kell visszamenni az NPC-hez).")]
    public bool autoComplete = false;
    public bool removeItemsOnCompletion = true;
    [Tooltip("Ezek a questek indulnak el automatikusan, amint ez befejeződik (Láncolat).")]
    public List<Quest> nextQuests = new List<Quest>(); 

    [Header("Jutalom")]
     [Tooltip("Hány Skill pontot kapjon a játékos? (0 = semmit)")]
    public int rewardSkillPoints;

    [Tooltip("Kapjon tárgyat? Írd ide az ID-t (pl. 'potion_health'). Ha üres, nincs tárgy.")]
    public string rewardItemID;

    [Tooltip("Hány darabot kapjon a tárgyból?")]
    public int rewardItemAmount = 1;
}