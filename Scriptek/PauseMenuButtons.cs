using UnityEngine;

public class PauseMenuButtons : MonoBehaviour
{
    public void OnResumeButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    public void OnSaveButtonPressed()
    {
       // if (GameManager.Instance != null)
           // GameManager.Instance.SaveGame();
    }

    public void OnQuitToMenuButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
    }
}
