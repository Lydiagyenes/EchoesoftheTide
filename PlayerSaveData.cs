using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveData : MonoBehaviour, ISaveable
{
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    
    
    public void LoadData(GameData data)
    {
         // Ha a 'controller' valamiért még null, próbáljuk meg most megszerezni.
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
        
        // Ha még mindig null (ez nem fordulhat elő, de biztonsági ellenőrzés), lépjünk ki.
        if (controller == null)
        {
            Debug.LogError("CharacterController not found on Player! Cannot load position.");
            return;
        }
        // A CharacterController-t ideiglenesen ki kell kapcsolni a pozícióállításhoz
        controller.enabled = false;
        transform.position = data.playerPosition;
        transform.rotation = data.playerRotation;
        controller.enabled = true;
        Debug.Log("Player data loaded to position: " + data.playerPosition);
    }

     public void SaveData(ref GameData data)
    {
        data.playerPosition = transform.position;
        data.playerRotation = transform.rotation;
        data.lastSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Player data saved from position: " + transform.position);
    }
}
