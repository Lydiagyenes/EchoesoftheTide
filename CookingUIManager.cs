using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    public static CookingUIManager Instance { get; private set; }

    [Header("Referenciák")]
    public GameObject panel; // Húzd be ide a CookingPanelt (saját magát)
    public Slider slider;    // Húzd be ide a Slidert alóla

    private void Awake()
    {
        // Singleton minta: Így bárki elérheti bárhonnan a "CookingUIManager.Instance" hívással
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        // Elrejtjük indításkor
        if (panel != null) panel.SetActive(false);
    }
}