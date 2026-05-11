using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void LoadLastSave()
    {
        Debug.Log("Utolsó mentés betöltése...");

        // 1. Betöltjük az adatokat a fájlból a memóriába
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
            
            // 2. Lekérjük, melyik pályán voltunk
            string sceneToLoad = SaveManager.Instance.gameData.lastSceneName;
            
            // Ha valamiért üres lenne, alapértelmezett a Viking falu
            if (string.IsNullOrEmpty(sceneToLoad)) sceneToLoad = "The_Viking_Village";

            // 3. Újratöltjük a pályát (ez reseteli a világot)
            // A SaveManager 'OnSceneLoaded' eseménye majd ráhúzza a betöltött adatokat
            Time.timeScale = 1f; // Fontos: Visszaállítjuk az időt, ha megállítottuk volna
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Nincs SaveManager! Csak újratöltjük a jelenlegi pályát.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu_Scene");
    }
}