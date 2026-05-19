using UnityEngine;

[CreateAssetMenu(fileName = "New Help Topic", menuName = "Game/Help Topic")]
public class HelpTopic : ScriptableObject
{
    public string topicName; // Pl. "Irányítás"
    
    [Header("Tartalom")]
    public Sprite illustration; // Pl. billentyűzetkiosztás képe (ha van)
    
    [TextArea(10, 20)]
    public string description; // A magyarázó szöveg
}