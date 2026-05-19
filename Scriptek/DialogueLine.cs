using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string text;      
    public AudioClip audio;  
    public float duration = 3f; // Adjunk alapértéket!
}