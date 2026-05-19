using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JournalUI : MonoBehaviour
{
    [Header("Adatok")]
    public List<LoreNote> allNotes; // Ide húzd be az összes létrehozott LoreNote fájlt!

    [Header("UI Referenciák")]
    public Transform listContainer;   // Bal oldal (Content)
    public GameObject buttonPrefab;   // A gomb minta
    public TextMeshProUGUI titleText; // Jobb oldal Cím
    public TextMeshProUGUI contentText; // Jobb oldal Szöveg
    public GameObject detailsPanel;   // Jobb oldal kerete
    public ScrollRect descriptionScrollRect; // Ezt majd húzd be az Inspectorban!

    private void OnEnable()
    {
        RefreshList();
        
        // Alapból üres a jobb oldal
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }

    public void RefreshList()
    {
        // 1. Töröljük a régi gombokat
        foreach (Transform child in listContainer) Destroy(child.gameObject);

        if (InventoryManager.Instance == null) return;

        bool foundAny = false;

        // 2. Végigmegyünk az összes létező bejegyzésen
        foreach (var note in allNotes)
        {
             if (QuestLog.Instance.HasJournalEntry(note.requiredItemID))
            {
                CreateButton(note);
                foundAny = true;
            }
        }

        if (!foundAny)
        {
            // Opcionális: Kiírhatod, hogy "Még nem találtál feljegyzéseket."
        }
    }

    void CreateButton(LoreNote note)
    {
        GameObject btnObj = Instantiate(buttonPrefab, listContainer);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = note.title;

        btnObj.GetComponent<Button>().onClick.AddListener(() => 
        {
            ShowDetails(note);
        });
    }

    void ShowDetails(LoreNote note)
    {
        if (detailsPanel != null) detailsPanel.SetActive(true);
        titleText.text = note.title;
        contentText.text = note.content;
        if (descriptionScrollRect != null)
        {
            // Visszaállítjuk a csúszkát a legtetejére (1 = fent, 0 = lent)
            descriptionScrollRect.verticalNormalizedPosition = 1f;
            
            // Néha kell egy Canvas frissítés, hogy a Content Size Fitter észbe kapjon
            Canvas.ForceUpdateCanvases();
        }
    }
}