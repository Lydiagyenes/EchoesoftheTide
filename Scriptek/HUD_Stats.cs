using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // <-- EZ KELL!

public class HUD_Stats : MonoBehaviour
{
    [Header("Megjelenítés")]
    // Húzd be ide a Canvas alatt lévő Panelt vagy üres objektumot, 
    // ami összefogja a Slidereket és a szövegeket!
    public GameObject uiContainer; 

    public Slider healthSlider;
    public Slider staminaSlider;
    
    public TextMeshProUGUI hpText; 
    public TextMeshProUGUI staminaText;

    private void OnEnable()
    {
        // Feliratkozunk a pályaváltásra
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsChanged += UpdateUI;
            UpdateUI(); 
        }
        
        // Kézi ellenőrzés indításkor is
        CheckVisibility(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsChanged -= UpdateUI;
        }
    }

    // Pályaváltáskor fut le
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckVisibility(scene);
    }

    // Itt döntjük el, hogy látható legyen-e
    private void CheckVisibility(Scene scene)
    {
        if (uiContainer == null) return;

        // Ha Intróban, Menüben vagy Töltőképernyőn vagyunk -> ELREJTÉS
        if (scene.name == "Manager_Scene" || scene.name == "Intro_Scene" || scene.name == "MainMenu_Scene" || scene.name == "Loading_Scene")
        {
            uiContainer.SetActive(false);
        }
        else
        {
            // Minden más (játék) pályán -> MEGJELENÍTÉS
            uiContainer.SetActive(true);
            UpdateUI(); // Frissítünk is egyet, hogy aktuális legyen
        }
    }

    void UpdateUI()
    {
        // ... (Ez a rész változatlan marad, csak a null checket érdemes megtartani) ...
        if (PlayerStats.Instance == null) return;

        float hp = PlayerStats.Instance.currentHealth;
        float maxHp = PlayerStats.Instance.MaxHealth;
        float stam = PlayerStats.Instance.currentStamina;
        float maxStam = PlayerStats.Instance.MaxStamina;

        if (healthSlider != null) healthSlider.value = hp / maxHp;
        if (staminaSlider != null) staminaSlider.value = stam / maxStam;

        if (hpText != null) hpText.text = $"{Mathf.Round(hp)} / {maxHp}";
        if (staminaText != null) staminaText.text = $"{Mathf.Round(stam)} / {maxStam}";
    }
}