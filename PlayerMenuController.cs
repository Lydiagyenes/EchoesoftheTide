using UnityEngine;
using UnityEngine.UI; 

public class PlayerMenuController : MonoBehaviour
{
    public static PlayerMenuController Instance { get; private set; }

    [Header("Fő komponensek")]
    public GameObject menuRoot; 
    
    [Header("Tartalom Panelek")]
    public GameObject[] contentPanels; 

    [Header("Navigációs Gombok")]
    public Button[] tabButtons;

    private int currentTabIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // --- 1. JAVÍTÁS: INDÍTÁSKOR AZONNAL BEZÁRJUK! ---
        if (menuRoot != null) 
        {
            menuRoot.SetActive(false);
        }
        // ------------------------------------------------

        // Gombok bekötése
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; 
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
    }

    private void OnEnable()
    {
         if (menuRoot != null) 
        {
            menuRoot.SetActive(true);
        }
        Time.timeScale = 0f;
        GameManager.isPaused = true; // Statikus, így jó

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SwitchTab(0); 

        var tutorial = GetComponent<MenuTutorialController>();
        if (tutorial != null)
        {
            tutorial.ShowExplanation(0);
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        GameManager.isPaused = false; // Statikus, így jó

        // --- 2. JAVÍTÁS: STATIC VÁLTOZÓ ELÉRÉSE HELYESEN ---
        // GameManager.Instance.isInventoryOpen HELYETT GameManager.isInventoryOpen
        if (!GameManager.isInventoryOpen) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // ----------------------------------------------------
    }

    public void SwitchTab(int tabIndex)
    {
        currentTabIndex = tabIndex;

        foreach (var panel in contentPanels)
        {
            if(panel != null) panel.SetActive(false);
        }

        if (tabIndex >= 0 && tabIndex < contentPanels.Length)
        {
            if(contentPanels[tabIndex] != null) 
                contentPanels[tabIndex].SetActive(true);
        }
        
        UpdateTabButtons(tabIndex);
    }

    private void UpdateTabButtons(int activeIndex)
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].interactable = (i != activeIndex);
        }
    }
}