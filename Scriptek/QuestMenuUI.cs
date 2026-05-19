using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestMenuUI : MonoBehaviour
{
    [Header("Bal Oldal (Lista)")]
    public Transform questListContent; 
    public GameObject questButtonPrefab; 

    [Header("Jobb Oldal (Részletek)")]
    // Ellenőrizd az Inspectorban, hogy ezek be vannak-e húzva!
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI objectiveText; // Itt jelenítjük meg a feladatokat
    public GameObject detailsPanel; 

    private Button selectedButton;

    private void OnEnable()
    {
        RefreshQuestList();
        ClearDetails();
    }

    public void RefreshQuestList()
    {
        foreach (Transform child in questListContent) Destroy(child.gameObject);

        if (QuestLog.Instance == null) return;

        foreach (var quest in QuestLog.Instance.activeQuests)
        {
            CreateQuestButton(quest, false);
        }

        foreach (var quest in QuestLog.Instance.completedQuests)
        {
            CreateQuestButton(quest, true);
        }
    }

    private void CreateQuestButton(Quest quest, bool isCompleted)
    {
        GameObject btnObj = Instantiate(questButtonPrefab, questListContent);
        Button btn = btnObj.GetComponent<Button>();
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

        if (btnText != null)
        {
            if (isCompleted) btnText.text = $"<color=#888888><s>{quest.questName}</s></color>";
            else btnText.text = quest.questName;
        }

        btn.onClick.AddListener(() => 
        {
            ShowQuestDetails(quest, isCompleted);
            SetSelectedButton(btn);
        });
    }

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

    // --- ITT VOLT A HIBA, EZT JAVÍTOTTUK ---
    public void ShowQuestDetails(Quest quest, bool isCompleted)
    {
        if (detailsPanel != null) detailsPanel.SetActive(true);

        titleText.text = quest.questName;
        descriptionText.text = quest.description;

        if (isCompleted)
        {
            objectiveText.text = "<color=green>Minden feladat teljesítve.</color>";
        }
        else
        {
            // ÚJ LOGIKA: Lista bejárása
            string objectivesStr = "<b>Feladatok:</b>\n";

            foreach (var obj in quest.objectives)
            {
                string status = "";

                switch (obj.type)
                {
                    case QuestType.CollectItem:
                        // Az 'obj'-ból vesszük az adatokat!
                        int current = InventoryManager.Instance.GetItemAmount(obj.targetID);
                        string color = (current >= obj.requiredAmount) ? "green" : "red";
                        status = $"{obj.objectiveDescription}: <color={color}>{current} / {obj.requiredAmount}</color>";
                        break;

                    case QuestType.TalkToNPC:
                        status = $"{obj.objectiveDescription} (Beszélj vele)";
                        break;
                    
                    default:
                        status = obj.objectiveDescription;
                        break;
                }

                objectivesStr += $"- {status}\n";
            }
            
            objectiveText.text = objectivesStr;
        }
    }

    private void ClearDetails()
    {
        titleText.text = "";
        descriptionText.text = "Válassz egy küldetést...";
        objectiveText.text = "";
        if (detailsPanel != null) detailsPanel.SetActive(false);
        selectedButton = null;
    }
}