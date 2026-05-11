using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZone : MonoBehaviour
{
    // STATIC: Ez a változó közös az összes CameraZone szkript között!
    // Számolja, hány zónában áll a játékos egyszerre.
    private static int activeZoneCount = 0;

    [Header("Beállítások")]
    public bool targetBodyInsteadOfHead = true;

    // Amikor új pálya töltődik be, nullázzuk a számlálót, nehogy beragadjon
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
        activeZoneCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeZoneCount++; // Növeljük a számlálót
            UpdateCamera();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeZoneCount--; // Csökkentjük
            
            // Biztonsági védelem, ne legyen negatív
            if (activeZoneCount < 0) activeZoneCount = 0;

            UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        CameraTargetSetter setter = FindFirstObjectByType<CameraTargetSetter>();
        if (setter != null)
        {
            // Ha legalább EGY zónában benne vagyunk (> 0), akkor Test nézet
            if (activeZoneCount > 0)
            {
                setter.SetZoneMode(true);
            }
            // Ha már EGYIKBEN sem vagyunk (0), akkor Fej nézet
            else
            {
                setter.SetZoneMode(false);
            }
        }
    }
}