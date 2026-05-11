using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Beállítások")]
    public float height = 50f;
    public bool rotateWithPlayer = true;

    [Header("UI Referencia")]
    public GameObject minimapUI; 

    [Header("Tiltott Pályák")]
    public string[] disabledScenes = new string[] { 
        "MainMenu_Scene", "Intro_Scene", "Loading_Scene", "Credits_Scene" 
    };

    private Transform playerTarget;
    private Camera minimapCam;

    private void Awake()
    {
        minimapCam = GetComponent<Camera>();
        if (minimapCam == null) minimapCam = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckVisibility(scene.name);
        // Itt is keresünk egyet, de ha nem találjuk, nem baj, majd az Update megoldja
        FindPlayer();
    }

    void CheckVisibility(string sceneName)
    {
        bool shouldShow = true;
        foreach (string bannedScene in disabledScenes)
        {
            if (sceneName == bannedScene)
            {
                shouldShow = false;
                break;
            }
        }

        if (minimapCam != null) minimapCam.enabled = shouldShow;
        if (minimapUI != null) minimapUI.SetActive(shouldShow);
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            Debug.Log("[Minimap] Játékos megtalálva és csatlakoztatva!");
        }
    }

    private void LateUpdate()
    {
        // Ha le van tiltva a kamera (pl. menüben), ne fusson
        if (minimapCam != null && !minimapCam.enabled) return;

        // --- A JAVÍTÁS: ---
        // Ha elvesztettük a játékost (vagy még nem találtuk meg), keressük meg!
        if (playerTarget == null)
        {
            FindPlayer();
            return; // Ha még mindig nincs meg, várunk a következő frame-re
        }

        // Ha megvan, követjük
        Vector3 newPos = playerTarget.position;
        newPos.y = height;
        transform.position = newPos;

        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, playerTarget.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}