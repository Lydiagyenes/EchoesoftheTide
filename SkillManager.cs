using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour, ISaveable
{
    public static SkillManager Instance { get; private set; }

    // Itt tároljuk, mennyi pontunk van épp
    public int availablePoints = 0;
    
    // Itt tároljuk a feloldott skillek ID-jait (gyors kereséshez)
    private HashSet<string> unlockedSkills = new HashSet<string>();

    public event System.Action OnSkillUnlocked; // UI frissítéshez

    private void Awake()
    {
       // if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Ha a _GameSystems alatt van, nem kell DontDestroy, de a biztonság kedvéért:
       // if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }

    // --- LEKÉRDEZÉS (Ezt hívják majd a játékrendszerek) ---
    public bool HasSkill(string skillID)
    {
        return unlockedSkills.Contains(skillID);
    }
    
    public bool HasSkill(Skill skill)
    {
        if (skill == null) return false;
        return unlockedSkills.Contains(skill.skillID);
    }

    // --- FELOLDÁS LOGIKA ---
    public bool CanUnlock(Skill skill)
    {
        // 1. Már megvan?
        if (HasSkill(skill)) return false;

        // 2. Van elég pont?
        if (availablePoints < skill.cost) return false;

        // 3. Megvan az előfeltétel? (Ha van szülője)
        if (skill.requiredSkill != null)
        {
            if (!HasSkill(skill.requiredSkill)) return false;
        }

        return true;
    }

    public void UnlockSkill(Skill skill)
    {
        if (CanUnlock(skill))
        {
            availablePoints -= skill.cost;
            unlockedSkills.Add(skill.skillID);
            
            Debug.Log($"Skill feloldva: {skill.skillName}");
            OnSkillUnlocked?.Invoke();

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.RecalculateStats();
            }
        }
    }

    // --- PONT SZERZÉS (Questek után hívd meg!) ---
    public void AddSkillPoint(int amount = 1)
    {
        availablePoints += amount;
        Debug.Log($"Kaptál {amount} képességpontot! Összesen: {availablePoints}");
        OnSkillUnlocked?.Invoke(); // UI frissítés miatt
    }

    // --- MENTÉS / BETÖLTÉS ---
    public void SaveData(ref GameData data)
    {
        data.skillPoints = this.availablePoints;
        data.unlockedSkillIDs = new List<string>(this.unlockedSkills);
    }

    public void LoadData(GameData data)
    {
        this.availablePoints = data.skillPoints;
        this.unlockedSkills = new HashSet<string>(data.unlockedSkillIDs);
        OnSkillUnlocked?.Invoke();
    }
}