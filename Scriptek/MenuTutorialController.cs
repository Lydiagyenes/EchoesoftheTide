using UnityEngine;
using System.Collections.Generic;

// FONTOS: Bekerült az ISaveable!
public class MenuTutorialController : MonoBehaviour, ISaveable
{
    [Header("Magyarázó Szövegek")]
    public DialogueLine questTabLine;    // 0. fül
    public DialogueLine journalTabLine;  // 1. fül
    public DialogueLine skillTabLine;    // 2. fül
    public DialogueLine craftingTabLine; // 3. fül
    public DialogueLine botanyTabLine;   // 4. fül
    public DialogueLine helpTabLine;     // 5. fül

    // Itt tartjuk nyilván játék közben, hogy miket láttunk már
    private List<int> shownTabs = new List<int>();

    // Ezt a függvényt hívják majd a gombok
    public void ShowExplanation(int tabIndex)
    {
        if (SubtitleManager.Instance == null) return;

        // 1. VÉDELEM: Ha ezt a fület már meghallgattuk, azonnal kilépünk!
        if (shownTabs.Contains(tabIndex))
        {
            return; 
        }

        DialogueLine lineToPlay = null;

        switch (tabIndex)
        {
            case 0: lineToPlay = questTabLine; break;
            case 1: lineToPlay = journalTabLine; break;
            case 2: lineToPlay = skillTabLine; break;
            case 3: lineToPlay = craftingTabLine; break;
            case 4: lineToPlay = botanyTabLine; break;
            case 5: lineToPlay = helpTabLine; break;
        }

        if (lineToPlay != null)
        {
            // 2. FELJEGYZÉS: Hozzáadjuk a memóriához, hogy többet ne induljon el!
            shownTabs.Add(tabIndex);

            // 3. LEJÁTSZÁS
            SubtitleManager.Instance.PlayDialogue(lineToPlay, null);
        }
    }

    // ==========================================
    // --- MENTÉS ÉS BETÖLTÉS (ISaveable) ---
    // ==========================================

    public void SaveData(ref GameData data)
    {
        // Elmentjük a listát a JSON-be
        data.seenTutorialTabs = new List<int>(this.shownTabs);
    }

    public void LoadData(GameData data)
    {
        // Betöltjük a listát, így indítás után is emlékezni fog
        this.shownTabs = new List<int>(data.seenTutorialTabs);
    }
}