using UnityEngine;

public class SystemAnchor : MonoBehaviour
{
    public static SystemAnchor Instance { get; private set; }

    private void Awake()
    {
        // SZELLEMIRTÁS:
        // Ha van Instance, de az objektuma már törlődött (null), akkor az egy szellem.
        if (Instance != null && (Instance == null || Instance.gameObject == null))
        {
            Instance = null; // Reseteljük a referenciát
        }

        // SINGLETON ELLENŐRZÉS:
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SystemAnchor] Duplikáció! Törlöm az újat, mert már van egy _GameSystems.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SystemAnchor] _GameSystems sikeresen rögzítve!");
    }
}