using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Settings")]
    public GameObject playerPrefab;
    public string mainMenuSceneName = "MainMenu_Scene";
    public string firstLevelSceneName = "The_Viking_Village";

    [Header("State")]
    public static bool isPaused = false;
    public static bool isInventoryOpen = false;

    private Vector3 nextSpawnPosition;
    private Quaternion nextSpawnRotation;
    private GameObject pauseMenuPanel;
    public bool menusUnlocked = false;
  

   private void Awake()
    {
        // if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
       // DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshPauseMenuReference();
        // Megkeressük a gombokat kezelő scriptet, akár inaktív állapotban is
        var buttonsScript = FindFirstObjectByType<PauseMenuButtons>(FindObjectsInactive.Include);
        if (buttonsScript != null)
        {
            // A panel maga az a GameObject, amin a script van (vagy annak szülője)
            pauseMenuPanel = buttonsScript.gameObject;
            pauseMenuPanel.SetActive(false);
        }
    }

    void Start()
{
    // Meglévő kódod...
    if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    
    // --- ÚJ RÉSZ: AUTOMATIKUS INDÍTÁS ---
    // Ha a Manager Scene-ben vagyunk, azonnal indítsuk az Intrót (vagy a menüt)
   /* if (SceneManager.GetActiveScene().name == "Manager_Scene")
    {
        Debug.Log("Manager Scene betöltve. Indul az Intro...");
        // Fontos: null-t adunk át képnek és pozíciónak, mert Intrónál nem számít
        SceneTransitionManager.Instance.LoadScene("Intro_Scene", Vector3.zero, Quaternion.identity, null);
    } */
}

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    

    private void Update()
    {

        string currentScene = SceneManager.GetActiveScene().name;

        // JAVÍTÁS: Tiltólista bővítése
        // Ha Menüben VAGY Intróban VAGY Töltés közben vagyunk, NE működjön a Pause/Inventory!
        if (currentScene == mainMenuSceneName || 
            currentScene == "Intro_Scene" || 
            currentScene == "Loading_Scene") 
        {
            return; // Kilépünk, nem vizsgáljuk tovább a gombokat
        }
        
/*
        // Inventory Toggle (I vagy Tab)
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            // Ha a pause menü nincs nyitva, akkor kapcsolhatjuk az inventory-t
            if (!isPaused)
            {
                isInventoryOpen = !isInventoryOpen;
                UpdateGameState();
            }
        }
*/
        // Pause Toggle (Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Ha az inventory nyitva van, az Escape először azt csukja be
            if (isInventoryOpen)
            {
               SetInventoryState(false);
            }
            else // Ha az inventory már be van csukva, akkor a pause menüt kapcsolgatja
            {
                isPaused = !isPaused;
                  UpdateGameState();
            }
            
                    }
    }
      public void SetInventoryState(bool isOpen)
    {
        isInventoryOpen = isOpen;
        UpdateGameState(); // Ez majd kezeli a kurzort meg az időt
    }
    
    // --- EZ A KÖZPONTI FÜGGVÉNY IRÁNYÍT MINDENT ---
    private void UpdateGameState()
    {
        bool isAnyMenuOpen = isPaused || isInventoryOpen;

        // Játékidő és kurzor kezelése
        Time.timeScale = isAnyMenuOpen ? 0f : 1f;
        Cursor.lockState = isAnyMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isAnyMenuOpen;

        // UI Panelek láthatóságának kezelése
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        var inventoryView = FindFirstObjectByType<InventoryView>();
        if (inventoryView != null)
        {
            inventoryView.SetVisible(isInventoryOpen);
        }
    }
    
   // --- UI GOMBOK FÜGGVÉNYEI ---
    
    public void ResumeGame()
    {
        isPaused = false;
        UpdateGameState();
    }
    
    // Ezt már nem a gomb hívja direktben, hanem a SaveSlotUI
    public void SaveToSlot(int slotIndex)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(slotIndex);
            Debug.Log($"[GameManager] Játék mentve a {slotIndex}. slotra!");
        }
    }

    public void LoadFromSlot(int slotIndex)
    {
        Debug.Log($"[GameManager] Betöltés indítása a {slotIndex}. slotból...");
        
        // 1. Elmentjük, hogy melyik slotot kérték
        PlayerPrefs.SetInt("LoadGameSlot", slotIndex);
        
        // 2. Kiolvassuk, hogy a kiválasztott slot mentésében melyik pálya van
        string sceneToLoad = SaveManager.Instance.GetSceneNameFromSaveFile(slotIndex);
        
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Reset state before loading
            isPaused = false;
            isInventoryOpen = false;
            Time.timeScale = 1f;
            
            SceneTransitionManager.Instance.LoadScene(sceneToLoad, Vector3.zero, Quaternion.identity, null);
        }
        else
        {
            Debug.LogWarning($"[GameManager] A {slotIndex}. slot üres vagy sérült! Új játék indul.");
            NewGame();
        }
    }
    
    public void LoadMainMenu()
    {
        isPaused = false;
        isInventoryOpen = false;
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.LoadScene("MainMenu_Scene", Vector3.zero, Quaternion.identity, null);
    }
    
    // --- SCENE KEZELÉS ---
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    Debug.Log($"<color=yellow>SCENE BETÖLTVE: '{scene.name}'</color>");
 // Pause panel referencia frissítése minden scene váltáskor
    RefreshPauseMenuReference();

    // 1. Menü és Töltőképernyő kezelése
    if (scene.name == mainMenuSceneName || scene.name == "Loading_Scene" || 
        scene.name == "Intro_Scene" || scene.name == "Manager_Scene")
    {
        Debug.Log($"<color=cyan>Menü/Intro mód: '{scene.name}'</color>");
        isPaused = false;
        isInventoryOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Panel biztos elrejtése
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        return;
    }

    int slotToLoad = PlayerPrefs.GetInt("LoadGameSlot", 0);
    bool loadFromMenu = slotToLoad > 0;

    // HA MENÜBŐL TÖLTÜNK → ELŐSZÖR TÖLTSÜK BE A FÁJLT, AZTÁN ELLENŐRIZZÜK
    if (loadFromMenu)
    {
        SaveManager.Instance.LoadGame(slotToLoad);
        Debug.Log($"<color=green>SaveManager.LoadGame({slotToLoad}) meghívva.</color>");
    }

    // NULL ELLENŐRZÉS – ha valami hiba van a mentési fájlban
    if (SaveManager.Instance == null || SaveManager.Instance.gameData == null)
    {
        Debug.LogError("<color=red>HIBA: gameData null a betöltés után! Új játékos spawn az alapértelmezett pozícióba.</color>");
        SpawnPlayer(this.nextSpawnPosition + Vector3.up * 1.5f, this.nextSpawnRotation);
        PlayerPrefs.SetInt("LoadGameSlot", 0);
        this.nextSpawnPosition = Vector3.zero;
        this.nextSpawnRotation = Quaternion.identity;
        UpdateGameState();
        return;
    }

    bool hasDataForThisScene = SaveManager.Instance.gameData.lastSceneName == scene.name;
    
    // DEBUG: Látjuk, mit talált
    Debug.Log($"<color=cyan>loadFromMenu={loadFromMenu}, slotToLoad={slotToLoad}, lastSceneName='{SaveManager.Instance.gameData.lastSceneName}', currentScene='{scene.name}', hasDataForThisScene={hasDataForThisScene}</color>");

    bool shouldUseSaveData = loadFromMenu || (hasDataForThisScene && this.nextSpawnPosition == Vector3.zero);

    Vector3 spawnPos;
    Quaternion spawnRot;

    if (shouldUseSaveData)
    {
        spawnPos = SaveManager.Instance.gameData.playerPosition;
        spawnRot = SaveManager.Instance.gameData.playerRotation;
        Debug.Log($"<color=green>MENTÉS ALKALMAZÁSA. Spawn: {spawnPos}</color>");
    }
    else
    {
        spawnPos = this.nextSpawnPosition;
        spawnPos.y += 1.5f;
        spawnRot = this.nextSpawnRotation;
        Debug.Log($"OnSceneLoaded: Portál/Új spawn. Spawn: {spawnPos}");
    }

    SpawnPlayer(spawnPos, spawnRot);

    if (shouldUseSaveData)
    {
        SaveManager.Instance.ApplyDataToScene();
    }

    PlayerPrefs.SetInt("LoadGameOnStart", 0);
    PlayerPrefs.SetInt("LoadGameSlot", 0); // ← FONTOS: reset után töröljük!
    this.nextSpawnPosition = Vector3.zero;
    this.nextSpawnRotation = Quaternion.identity;

    UpdateGameState();
}

private void RefreshPauseMenuReference()
{
    var buttonsScript = FindFirstObjectByType<PauseMenuButtons>(FindObjectsInactive.Include);
    if (buttonsScript != null)
    {
        pauseMenuPanel = buttonsScript.gameObject;
        pauseMenuPanel.SetActive(false);
        Debug.Log("[GameManager] PauseMenuPanel referencia frissítve.");
    }
    else
    {
        // Ha nincs (pl. MainMenu-ben), nullázzuk ki hogy ne mutasson régi panelre
        pauseMenuPanel = null;
        Debug.Log("[GameManager] PauseMenuPanel nem található ebben a scene-ben (ez normális MainMenu-nél).");
    }
}

// Külön spawn helper hogy ne duplikáljuk a kódot
private void SpawnPlayer(Vector3 spawnPos, Quaternion spawnRot)
{
    GameObject playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);

    CameraTargetSetter[] cameraSetters = FindObjectsByType<CameraTargetSetter>(FindObjectsSortMode.None);
    foreach (var setter in cameraSetters)
    {
        setter.SetTarget(playerInstance.transform);
    }
}
    
  public void NewGame()
    {
        // 1. Töröljük a betöltési memóriát
        PlayerPrefs.SetInt("LoadGameSlot", 0);
        
        // 2. HA ÚJ JÁTÉKOT KEZDÜNK, KÖNYÖRTELENÜL LETÖRÖLJÜK A RÉGI FÁJLOKAT!
        if (SaveManager.Instance != null) 
        {
            SaveManager.Instance.ClearAllSaveFiles(); // <-- EZ A KULCS!
            SaveManager.Instance.NewGame(); // És csinálunk egy üres memóriát
        }
        
        // A pozíciót beállítjuk, de a váltást az új menedzser indítja
        SetNextSpawnPoint(new Vector3(-40f, 4f, 34f), Quaternion.identity);
        SceneTransitionManager.Instance.LoadScene(firstLevelSceneName, nextSpawnPosition, nextSpawnRotation, null);
    }
   
    /* public void LoadGameFromMenu()
    {

         Debug.Log("GameManager.LoadGameFromMenu() called.");
        // 1. Beállítjuk a jelzőt, hogy az OnSceneLoaded tudja, hogy be kell tölteni a mentést.
        PlayerPrefs.SetInt("LoadGameOnStart", 1);
        
        // 2. Kiolvassuk a mentési fájlból, hogy melyik scene-t kell betölteni.
        string sceneToLoad = SaveManager.Instance.GetSceneNameFromSaveFile();
        
        // 3. Ellenőrizzük, hogy van-e érvényes mentés.
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Found scene in save file: " + sceneToLoad + ". Starting transition..."); // <-- 3. LOG
        // Itt az új SceneTransitionManager-t kell hívni!
        SceneTransitionManager.Instance.LoadScene(sceneToLoad, Vector3.zero, Quaternion.identity, null);
        }
        else
        {
            // Ha nincs mentési fájl, vagy az hibás, akkor nem csinálunk semmit,
            // vagy indíthatunk egy új játékot is vészmegoldásként.
            // A legjobb, ha a "Játék Betöltése" gomb inaktív, ha nincs mentés.
            // De egyelőre egy log üzenet is elég.
            Debug.LogWarning("No save file found to load! Starting a new game instead.");
            NewGame();
        }
    }*/
    
        
    public void SetNextSpawnPoint(Vector3 position, Quaternion rotation)
    {
        nextSpawnPosition = position;
        nextSpawnRotation = rotation;
    }
}