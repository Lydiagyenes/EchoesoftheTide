using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillMenuUI : MonoBehaviour
{
    public static SkillMenuUI Instance;

    [Header("Általános")]
    public TextMeshProUGUI availablePointsText; // Bal felül: "Pontjaid: 5"

    [Header("Részletek Panel (Ezeket hoztad most létre)")]
    public GameObject detailsPanel; // Maga a doboz, hogy elrejthessük, ha nincs kijelölés
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescText;
    public TextMeshProUGUI costText;
    public Button unlockButton;
    public TextMeshProUGUI unlockButtonLabel; // A gomb felirata (opcionális)

    // A jelenleg kiválasztott skill gombja
    private SkillUI_Node currentSelectedNode;

   private void Awake()
    {
        Instance = this;
        
        // AZONNALI ELREJTÉS INDÍTÁSKOR
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // 1. Frissítjük az ikonokat (hogy látszódjon, mi van megvéve)
        RefreshUI();
        
        // 2. Törlünk minden kijelölést (ez el is rejti a panelt biztonságból)
        DeselectAll();
    }

    // Ez frissíti az összes ikont (színek, lakatok) és a pontszámot
    public void RefreshUI()
    {
        // Frissítjük a pontszám kijelzőt
        if (SkillManager.Instance != null && availablePointsText != null)
        {
            availablePointsText.text = $"Elérhető pontok: {SkillManager.Instance.availablePoints}";
        }

        // Frissítjük az összes ikont a fán
        SkillUI_Node[] nodes = GetComponentsInChildren<SkillUI_Node>(true);
        foreach (var node in nodes)
        {
            node.UpdateVisuals();
        }

        // Ha van kiválasztott elem, annak az állapotát is frissítjük 
        // (pl. ha épp most vettük meg, a gomb tűnjön el)
        if (currentSelectedNode != null)
        {
            UpdateDetailsPanel(currentSelectedNode);
        }
    }

    
      // Ezt hívjuk meg, ha üres helyre kattintasz
    public void DeselectAll()
    {
        currentSelectedNode = null;
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    // Ezt hívja a Node, ha rákattintasz
    public void SelectSkill(SkillUI_Node node)
    {
        // Ha ugyanarra kattintunk, ami már nyitva van, zárjuk be (Toggle)
        if (currentSelectedNode == node && detailsPanel.activeSelf)
        {
            DeselectAll();
            return;
        }

        currentSelectedNode = node;
        
        // Megjelenítjük a panelt
        if (detailsPanel != null) detailsPanel.SetActive(true);
        
        UpdateDetailsPanel(node);
    }

    private void UpdateDetailsPanel(SkillUI_Node node)
    {
        Skill skill = node.skillData;

        // 1. Szövegek kitöltése (Ez eddig is ment)
        if (skillNameText) skillNameText.text = skill.skillName;
        if (skillDescText) skillDescText.text = skill.description;

        // --- BIZTONSÁGI ELLENŐRZÉS ---
        if (SkillManager.Instance == null)
        {
            Debug.LogError("CRITICAL HIBA: Nincs SkillManager a játékban! Nem tudok árat számolni.");
            if (costText) costText.text = $"Költség: {skill.cost} (Offline)";
            if (unlockButton) unlockButton.interactable = false;
            return; // Kilépünk, hogy ne legyen fagyás
        }
        // -----------------------------

        // 2. Logika folytatása (Csak ha van Manager)
        bool isUnlocked = SkillManager.Instance.HasSkill(skill);
        bool canUnlock = SkillManager.Instance.CanUnlock(skill);

        if (unlockButton) unlockButton.onClick.RemoveAllListeners();

        if (isUnlocked)
        {
            if (costText) costText.text = "<color=green>MEGSZEREZVE</color>";
            if (unlockButton) 
            {
                unlockButton.interactable = false;
                if (unlockButtonLabel) unlockButtonLabel.text = "Megtanulva";
            }
        }
        else
        {
            string color = (SkillManager.Instance.availablePoints >= skill.cost) ? "white" : "red";
            if (costText) costText.text = $"Költség: <color={color}>{skill.cost}</color> Pont";

            if (canUnlock)
            {
                if (unlockButton)
                {
                    unlockButton.interactable = true;
                    if (unlockButtonLabel) unlockButtonLabel.text = "Feloldás";
                    unlockButton.onClick.AddListener(TryUnlockCurrent);
                }
            }
            else
            {
                if (unlockButton)
                {
                    unlockButton.interactable = false;
                    if (unlockButtonLabel) unlockButtonLabel.text = "Zárolt";
                }
            }
        }
    }

    // Ez fut le, amikor megnyomod a gombot
    public void TryUnlockCurrent()
    {
        if (currentSelectedNode != null)
        {
            SkillManager.Instance.UnlockSkill(currentSelectedNode.skillData);
            
            // 1. Frissítjük az összes ikont (hogy zöld legyen)
            RefreshUI();
            
            // 2. Frissítjük a Részletek Panelt is (hogy kiírja: Megszerezve)
            // KÖZVETLENÜL meghívjuk az UpdateDetailsPanel-t, nem csak a Select-et!
            UpdateDetailsPanel(currentSelectedNode); 
        }
    }
}