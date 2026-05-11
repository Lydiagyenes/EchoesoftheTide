using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Loading Screen - húzd be a Manager Scene-ből!")]
    public GameObject loadingScreenObject; // NEM prefab, hanem a scene-beli Canvas!
    public Image backgroundImage;          // Direktben húzd be az Image-t
    public Slider loadingBar;              // Direktben húzd be a Slider-t

    private void Awake()
    {
        Debug.Log($"[SceneTransitionManager] Awake fut ezen: {gameObject.name}");

        if (Instance != null && Instance != this)
        {
            if (Instance.gameObject == null) Instance = this;
            else { Destroy(gameObject); return; }
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);

        // A LoadingCanvas-t is DontDestroyOnLoad-ba tesszük
        if (loadingScreenObject != null)
        {
            DontDestroyOnLoad(loadingScreenObject);
            loadingScreenObject.SetActive(false);
            Debug.Log($"[SceneTransitionManager] LoadingScreen kész. Image: {backgroundImage != null}, Slider: {loadingBar != null}");
        }
        else
        {
            Debug.LogError("[SceneTransitionManager] loadingScreenObject nincs behúzva!");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void LoadScene(string sceneName, Vector3 spawnPosition, Quaternion spawnRotation, Sprite loadingImage)
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneRoutine(sceneName, spawnPosition, spawnRotation, loadingImage));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Vector3 spawnPosition, Quaternion spawnRotation, Sprite loadingImage)
    {
        Debug.Log($"[Transition] Betöltés indítása: {sceneName}");

        if (loadingScreenObject == null)
        {
            Debug.LogError("[Transition] loadingScreenObject NULL!");
            yield break;
        }

        // Kép beállítása
        if (backgroundImage != null)
        {
            if (loadingImage != null)
            {
                backgroundImage.color = Color.white;
                backgroundImage.sprite = loadingImage;
            }
            else
            {
                backgroundImage.sprite = null;
                backgroundImage.color = Color.black;
            }
        }

        if (loadingBar != null) loadingBar.value = 0f;

        // MEGJELENÍTÉS
        loadingScreenObject.SetActive(true);

        yield return null;
        yield return null;

        // Spawn pont
        if (GameManager.Instance != null)
            GameManager.Instance.SetNextSpawnPoint(spawnPosition, spawnRotation);

        // Töltés
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            if (loadingBar != null) loadingBar.value = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }

        if (loadingBar != null) loadingBar.value = 1f;
        yield return new WaitForSecondsRealtime(1.0f);

        operation.allowSceneActivation = true;
        while (!operation.isDone) yield return null;

        Debug.Log($"[Transition] SIKERES VÁLTÁS: {sceneName}");

        loadingScreenObject.SetActive(false);
        Time.timeScale = 1f;
    }
}