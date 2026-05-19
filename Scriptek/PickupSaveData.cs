using UnityEngine;

public class PickupSaveData : MonoBehaviour, ISaveable
{
    [Tooltip("Ennek teljesen EGYEDINEK kell lennie minden példánynál! (Ha üres, a kód generálja).")]
    public string uniqueID; 
    
    // 1. VÁLTOZÁS: Awake-et használunk Start helyett, hogy biztosan meglegyen az ID
    private void Awake() 
    {
        GenerateID();
    }

    // Segédfüggvény, hogy bárhonnan meghívhassuk, ha hiányzik az ID
    private void GenerateID()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            // Pl: Stone_-24.5_1.2_45.6
            // "F1" formátum: Csak 1 tizedesjegy, hogy stabilabb legyen
            uniqueID = $"{gameObject.name}_{transform.position.x:F1}_{transform.position.y:F1}_{transform.position.z:F1}";
        }
    }

    // --- ISaveable implementáció ---

    // Betöltéskor
    public void LoadData(GameData data)
    {
        // 2. VÁLTOZÁS: Biztonsági ellenőrzés. 
        // Ha a SaveManager előbb futna, mint az Awake, itt pótoljuk az ID-t!
        if (string.IsNullOrEmpty(uniqueID)) GenerateID();

        if (data.collectedPickupIDs.Contains(uniqueID))
        {
            // Debug.Log($"[PickupSave] {uniqueID} már fel volt véve. Eltüntetés.");
            gameObject.SetActive(false);
        }
    }

    // Mentéskor
    public void SaveData(ref GameData data)
    {
        // Csak akkor mentjük, ha inaktív (tehát felvették)
        if (!gameObject.activeSelf)
        {
            if (!data.collectedPickupIDs.Contains(uniqueID))
            {
                data.collectedPickupIDs.Add(uniqueID);
            }
        }
    }

    // Ezt hívja az ItemPickup script
    public void CompletePickup()
    {
        // Biztos ami biztos
        if (string.IsNullOrEmpty(uniqueID)) GenerateID();

        if (SaveManager.Instance != null)
        {
            if (!SaveManager.Instance.gameData.collectedPickupIDs.Contains(uniqueID))
            {
                SaveManager.Instance.gameData.collectedPickupIDs.Add(uniqueID);
                Debug.Log($"[PickupSave] {uniqueID} azonnal regisztrálva a mentésbe.");
            }
        }
    }
}