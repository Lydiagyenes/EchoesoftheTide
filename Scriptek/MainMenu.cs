using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstGameSceneName = "The_Viking_Village"; // A szigetes scene neve

    void Start()
    {
        // Amikor betölt a menü, elindítjuk a zenét
        if (AudioManager.Instance != null)
        {
            Debug.Log("Menü betöltve -> Zene indítása");
            AudioManager.Instance.SetState(MusicState.Default); 
            // Vagy ha van külön menüzenéd: AudioManager.Instance.PlayEventMusic(menuMusicClip);
        }
        
        // ... (többi kód, pl. kurzor beállítása) ...
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void NewGame()
    {
         if (GameManager.Instance != null)
        {
            GameManager.Instance.NewGame();
        }
        else // Vészmegoldás, ha a GameManager valamiért nem elérhető
        {
                        Debug.LogError("GameManager not found!");
        }
    }

     public void LoadGame()
    {

        Debug.Log("--- LOAD GAME PROCESS STARTED ---");
         if (GameManager.Instance != null)
        {
           // GameManager.Instance.LoadGameFromMenu(); // Egy új, dedikált függvény
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }
    }
    

    public void QuitGame()
    {
        Debug.Log("Kilépés...");
        Application.Quit();
    }
}
