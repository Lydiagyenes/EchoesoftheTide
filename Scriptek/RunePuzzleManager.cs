using UnityEngine;
using System.Collections.Generic;

public class RunePuzzleManager : MonoBehaviour
{
    [Header("A Helyes Sorrend (Húzd be a köveket!)")]
    public List<RunePillar> correctOrder;

    [Header("Jutalom")]
    public GameObject rewardObject; // Pl. egy Láda vagy Ajtó, ami megjelenik/kinyílik
    public bool openDoorLogic = false; // Ha True: SetActive(false) lesz a jutalom (eltűnik a fal)

    private int currentIndex = 0; // Hol tartunk a sorban?

    public void RuneActivated(RunePillar rune)
    {
        // Ellenőrizzük: A megütött kő az-e, aminek következnie kell?
        if (rune == correctOrder[currentIndex])
        {
            // JÓ TALÁLAT
            Debug.Log($"[Puzzle] Helyes rúna! ({currentIndex + 1} / {correctOrder.Count})");
            currentIndex++;

            // Megvan az összes?
            if (currentIndex >= correctOrder.Count)
            {
                CompletePuzzle();
            }
        }
        else
        {
            // ROSSZ TALÁLAT -> RESET
            Debug.Log("[Puzzle] ROSSZ SORREND! Reset.");
            ResetPuzzle();
        }
    }

    void ResetPuzzle()
    {
        currentIndex = 0;
        // Minden követ lekapcsolunk
        foreach (var r in correctOrder)
        {
            r.ResetRune();
        }
        
        // Opcionális: Hangeffekt (Hiba hang)
    }

    void CompletePuzzle()
    {
        Debug.Log("[Puzzle] PUZZLE MEGOLDVA!");
        
        if (rewardObject != null)
        {
            if (openDoorLogic)
                rewardObject.SetActive(false); // Eltüntetjük a falat
            else
                rewardObject.SetActive(true);  // Megjelenítjük a ládát
        }
        
        // Opcionális: Hangeffekt (Siker hang), Zene, XP adás
    }
}