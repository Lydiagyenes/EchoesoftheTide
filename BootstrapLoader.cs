// BootstrapLoader.cs

using UnityEngine;
using System.Collections;

public class BootstrapLoader : MonoBehaviour
{
    public string initialSceneToLoad = "Intro_Scene"; 

    void Start()
    {
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // Várunk, hogy a Managerek felébredjenek
        yield return new WaitForSeconds(0.1f); 

        // 1. Próbáljuk elérni a Singletons-t
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("[Bootstrap] Az Instance NULL. Megpróbálom manuálisan megkeresni...");
            
            // MENTŐÖV: Megkeressük a scene-ben lévő aktív managert
            var foundManager = FindFirstObjectByType<SceneTransitionManager>();
            
            if (foundManager != null)
            {
                Debug.Log("[Bootstrap] Megtaláltam manuálisan! Folytatás...");
                // Itt akár be is hívhatnánk a LoadScene-t a foundManager-en keresztül,
                // de a tisztaság kedvéért feltételezzük, hogy most már jó lesz.
                
                // Mivel megtaláltuk, használjuk közvetlenül ezt a referenciát:
                foundManager.LoadScene(initialSceneToLoad, Vector3.zero, Quaternion.identity, null);
                Destroy(gameObject);
                yield break; // Kilépünk, mert elvégeztük a dolgot
            }
            else
            {
                Debug.LogError("CRITICAL HIBA: Tényleg nincs SceneTransitionManager a Scene-ben (vagy inaktív)!");
                yield break;
            }
        }

        // Ha alapból megvolt az Instance (normál működés)
        Debug.Log($"[Bootstrap] Instance rendben. Indulás: {initialSceneToLoad}");
        SceneTransitionManager.Instance.LoadScene(
            initialSceneToLoad, 
            Vector3.zero, 
            Quaternion.identity, 
            null 
        );

        Destroy(gameObject);
    }
}