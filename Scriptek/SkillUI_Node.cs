using UnityEngine;
using UnityEngine.UI;

public class SkillUI_Node : MonoBehaviour
{
    public Skill skillData; 
    
    [Header("UI Elemek (Húzd be őket!)")]
    public Image skillIcon;     // Az ikon képe
    public Button btn;          // Maga a gomb
    public GameObject lockOverlay; // A lakat objektum
    public Image borderImage;   // A keret képe (ha van)

    private void Start()
    {
        if (skillData != null)
        {
            if (skillIcon != null) skillIcon.sprite = skillData.icon;
            if (btn != null) btn.onClick.AddListener(OnClick);
        }
    }

    public void UpdateVisuals()
    {
        if (skillData == null) return;

        bool isUnlocked = SkillManager.Instance.HasSkill(skillData);
        bool canUnlock = SkillManager.Instance.CanUnlock(skillData);

        // 1. HA MÁR MEGVAN (Zöld / Halvány)
        if (isUnlocked)
        {
            if (lockOverlay != null) lockOverlay.SetActive(false);
            if (borderImage != null) borderImage.color = Color.green;
            
            // FONTOS: Itt volt a hiba valószínűleg!
            if (skillIcon != null) 
                skillIcon.color = new Color(1f, 1f, 1f, 0.5f); // 50% áttetsző (Halványítás)
        }
        // 2. HA MEGSZEREZHETŐ (Sárga / Normál)
        else if (canUnlock)
        {
            if (lockOverlay != null) lockOverlay.SetActive(false); 
            if (borderImage != null) borderImage.color = Color.yellow;
            if (skillIcon != null) skillIcon.color = Color.white;
        }
        // 3. HA ZÁROLT (Szürke / Sötét)
        else
        {
            if (lockOverlay != null) lockOverlay.SetActive(true);
            if (borderImage != null) borderImage.color = Color.grey;
            if (skillIcon != null) skillIcon.color = new Color(0.3f, 0.3f, 0.3f, 1f); 
        }
    }

    void OnClick()
    {
        if (SkillMenuUI.Instance != null)
        {
            SkillMenuUI.Instance.SelectSkill(this);
        }
    }
}