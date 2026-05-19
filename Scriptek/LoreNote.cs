using UnityEngine;

[CreateAssetMenu(fileName = "New Lore Note", menuName = "RPG/Lore Note")]
public class LoreNote : ScriptableObject
{
    [Header("Kapcsolat")]
    [Tooltip("Melyik tárgyat kell birtokolni ahhoz, hogy ez olvasható legyen?")]
    public string requiredItemID; // Pl. "JournalPage_1" vagy "BottleMessage"

    [Header("Tartalom")]
    public string title; // Pl. "Elias első bejegyzése"
    [TextArea(5, 20)]
    public string content; // A hosszú szöveg
}