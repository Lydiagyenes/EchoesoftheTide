using UnityEngine;
using System.Collections.Generic;

public enum SkillBranch
{
    Survivor, // Túlélő
    Hunter,   // Vadász
    Explorer  // Felfedező
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
public class Skill : ScriptableObject
{
    [Header("Alapadatok")]
    public string skillID; // Pl. "survivor_botany_1"
    public string skillName; // Pl. "Tapasztalt Botanikus"
    [TextArea(3, 10)]
    public string description;
    public Sprite icon; 
    
    [Header("Szabályok")]
    public SkillBranch branch;
    public int cost = 1; // Mennyibe kerül (Skill Point)
    
    [Header("Függőségek")]
    // Melyik skill kell ahhoz, hogy ezt feloldhasd? (A Szülő skill)
    // Ha üres, akkor ez egy "Szint 1" skill.
    public Skill requiredSkill; 
}