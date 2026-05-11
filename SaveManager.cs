using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine.SceneManagement;
using System; // Kell az Exception kezeléshez

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public GameData gameData;
    public int currentSaveSlot = 1;

    private void Awake()
    {
        Instance = this;
        // saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json"); // Kompatibilitás miatt maradhat
        NewGame(); 
    }

    public void NewGame()
    {
        this.gameData = new GameData();
        this.currentSaveSlot = 1; // Új játéknál alapból az 1-es slotra állunk be
        Debug.Log("[SaveManager] Tiszta GameData létrehozva a memóriában.");
    }

    // ====================================================================
    // --- ÚJ FÜGGVÉNY: MINDEN FÁJL FIZIKAI TÖRLÉSE ---
    // ====================================================================
    public void ClearAllSaveFiles()
    {
        for (int i = 1; i <= 3; i++) // Feltételezzük, hogy 3 slotod van
        {
            string path = GetSaveFilePath(i);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    Debug.Log($"<color=red>[SaveManager] {i}. slot mentési fájlja TÖRÖLVE: {path}</color>");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Nem sikerült törölni a(z) {i}. mentést: {e.Message}");
                }
            }
        }
    }

    // ====================================================================
    // --- FÜGGVÉNYEK A SLOTOK KEZELÉSÉHEZ ---
    // ====================================================================

    private string GetSaveFilePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"savegame_{slot}.json");
    }

    public string GetSlotInfo(int slot)
    {
        string path = GetSaveFilePath(slot);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                GameData data = JsonUtility.FromJson<GameData>(json);
                return string.IsNullOrEmpty(data.lastSaveTime) ? "Foglalt mentés" : data.lastSaveTime;
            }
            catch
            {
                return "Sérült fájl!";
            }
        }
        return "Üres hely";
    }

    // ====================================================================
    // --- MEGLÉVŐ FÜGGVÉNYEK BIZTONSÁGOS FRISSÍTÉSE ---
    // ====================================================================

    private List<ISaveable> FindAllSaveableObjects()
    {
        List<ISaveable> saveables = new List<ISaveable>();
        
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            saveables.AddRange(rootObject.GetComponentsInChildren<ISaveable>(true));
        }

        if (QuestLog.Instance != null && QuestLog.Instance is ISaveable) saveables.Add((ISaveable)QuestLog.Instance);
        if (InventoryManager.Instance != null && InventoryManager.Instance is ISaveable) saveables.Add((ISaveable)InventoryManager.Instance);
        if (PlayerStats.Instance != null && PlayerStats.Instance is ISaveable) saveables.Add((ISaveable)PlayerStats.Instance);
        if (SkillManager.Instance != null && SkillManager.Instance is ISaveable) saveables.Add((ISaveable)SkillManager.Instance);
        if (BotanyManager.Instance != null && BotanyManager.Instance is ISaveable) saveables.Add((ISaveable)BotanyManager.Instance);
            
        UIInputController uiController = FindFirstObjectByType<UIInputController>();
        if (uiController != null && uiController is ISaveable) saveables.Add((ISaveable)uiController);

        return saveables;
    }

    public void SaveGame() { SaveGame(currentSaveSlot); }

    public void SaveGame(int slot)
    {
        currentSaveSlot = slot; 

        if (gameData == null) gameData = new GameData();

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "MainMenu_Scene" && currentScene != "Intro_Scene" && currentScene != "Loading_Scene" && currentScene != "Manager_Scene")
        {
            gameData.lastSceneName = currentScene;
        }

        gameData.lastSaveTime = DateTime.Now.ToString("yyyy. MM. dd. - HH:mm");

        List<ISaveable> saveableObjects = FindAllSaveableObjects();
       
        foreach (ISaveable saveable in saveableObjects)
        {
            saveable.SaveData(ref gameData);
        }

        try
        {
            string json = JsonUtility.ToJson(gameData, true); 
            string path = GetSaveFilePath(slot); 
            File.WriteAllText(path, json);
            Debug.Log($"<color=green>[SaveManager] Játék sikeresen mentve: {path}</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] HIBA A MENTÉS SORÁN! \n{e.Message}");
        }
    }

    public void LoadGame() { LoadGame(currentSaveSlot); }

    public void LoadGame(int slot)
    {
        currentSaveSlot = slot;
        string path = GetSaveFilePath(slot); 

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                gameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"<color=cyan>[SaveManager] {slot}. slot mentési fájlja betöltve!</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] HIBA A BETÖLTÉS SORÁN!\n{e.Message}");
                NewGame(); 
            }
        }
        else
        {
            Debug.LogWarning($"[SaveManager] A {slot}. slot üres. Új játék inicializálása.");
            NewGame();
        }
    }

    public string GetSceneNameFromSaveFile() { return GetSceneNameFromSaveFile(currentSaveSlot); }

    public string GetSceneNameFromSaveFile(int slot)
    {
        string path = GetSaveFilePath(slot);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                GameData data = JsonUtility.FromJson<GameData>(json);
                return data.lastSceneName;
            }
            catch (Exception) { return null; }
        }
        return null;
    }

    public void ApplyDataToScene()
    {
        if (gameData == null) return;

        List<ISaveable> saveableObjects = FindAllSaveableObjects();
        foreach (ISaveable saveable in saveableObjects)
        {
            saveable.LoadData(this.gameData);
        }
        
        Debug.Log("<color=cyan>[SaveManager] Adatok sikeresen ráhúzva a pályára!</color>");
    }
}