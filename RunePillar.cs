using UnityEngine;

public class RunePillar : MonoBehaviour
{
    [Header("Beállítások")]
    public RunePuzzleManager manager; // Ki a főnök?
    public GameObject activeLight;    // Fény vagy effekt, ha aktív
    
    [HideInInspector]
    public bool isActivated = false;

    void Start()
    {
        // Induláskor kapcsoljuk ki a fényt
        if (activeLight != null) activeLight.SetActive(false);
    }

    public void HitByWeapon()
    {
        // Ha már aktív, ne lehessen újra megütni
        if (isActivated) return;

        isActivated = true;

        // Vizuális visszajelzés (Fény bekapcsolása)
        if (activeLight != null) activeLight.SetActive(true);

        // Szólunk a Managernek, hogy "Engem ütöttek meg!"
        if (manager != null)
        {
            manager.RuneActivated(this);
        }
    }

    public void ResetRune()
    {
        isActivated = false;
        if (activeLight != null) activeLight.SetActive(false);
    }
}