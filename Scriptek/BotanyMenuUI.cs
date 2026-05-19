using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BotanyMenuUI : MonoBehaviour
{
    [Header("Bal Oldal (Lista)")]
    public Transform listContent;
    public GameObject listButtonPrefab; // A QuestItem_Prefab újrahasznosítva

    [Header("Jobb Oldal (Részletek)")]
    public GameObject detailsPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText; // Ide írjuk: Ehető/Mérgező
    public TextMeshProUGUI descriptionText;

    private Button selectedButton;

    private void OnEnable()
    {
        RefreshList();
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    public void RefreshList()
    {
        // 1. Törlés
        foreach (Transform child in listContent) Destroy(child.gameObject);

        // 2. Adatok lekérése a BotanyManagerből
        if (BotanyManager.Instance == null) return;

        // Végigmegyünk az összes létező növényen
        foreach (var plant in BotanyManager.Instance.allPlantsDatabase)
        {
            // Opcionális: Itt lehetne szűrni, hogy csak a felfedezetteket mutassa
            // De egyelőre listázzuk ki mindet, mint egy lexikont.

            GameObject btnObj = Instantiate(listButtonPrefab, listContent);
            
            // Gomb szöveg (Növény neve)
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = plant.itemName;

            // Gomb esemény
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => 
            {
                ShowPlantDetails(plant);
                SetSelectedButton(btn);
            });
        }
    }

    public void ShowPlantDetails(BotanyItem plant)
    {
        if (detailsPanel != null) detailsPanel.SetActive(true);

        // Név
        nameText.text = plant.itemName;

        // Státusz (Színezve)
        if (plant.isPoisonous)
        {
            statusText.text = "<color=red>MÉRGEZŐ</color>";
        }
        else
        {
            statusText.text = "<color=green>EHETŐ</color>";
        }

        // Leírás
        // Ha üres a leírás, kiírunk valami alapértelmezettet
        if (string.IsNullOrEmpty(plant.botanicalDescription))
        {
            descriptionText.text = "Erről a növényről nincsenek feljegyzések.";
        }
        else
        {
            descriptionText.text = plant.botanicalDescription;
        }
    }

    // Gomb kijelölés vizuális kezelése (Ugyanaz, mint a többinél)
    private void SetSelectedButton(Button newButton)
    {
        if (selectedButton != null)
        {
            var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null) text.fontStyle = FontStyles.Normal;
        }
        selectedButton = newButton;
        if (selectedButton != null)
        {
            var text = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null) text.fontStyle = FontStyles.Bold;
        }
    }
}