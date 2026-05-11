using UnityEngine;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{[Header("Szöveg Referenciák (Húzd be a gombok Text-jét!)")]
    public TextMeshProUGUI slot1Text;
    public TextMeshProUGUI slot2Text;
    public TextMeshProUGUI slot3Text;

    [Header("Beállítások")][Tooltip("Pipáld be, ha ez a Főmenüben van (Betöltés). Vedd ki, ha a Pause menüben (Mentés).")]
    public bool isLoadMode = false; 

    // Amikor a panelt bekapcsolják (pl. a Mentés gombra kattintva), ez automatikusan lefut
    private void OnEnable()
    {
        RefreshSlots();
    }

    // Kiolvassa a dátumokat és ráírja a gombokra
    public void RefreshSlots()
    {
        if (SaveManager.Instance != null)
        {
            if (slot1Text != null) slot1Text.text = "1. Hely: " + SaveManager.Instance.GetSlotInfo(1);
            if (slot2Text != null) slot2Text.text = "2. Hely: " + SaveManager.Instance.GetSlotInfo(2);
            if (slot3Text != null) slot3Text.text = "3. Hely: " + SaveManager.Instance.GetSlotInfo(3);
        }
    }

    // Ezt hívják majd a gombok (1, 2 vagy 3-as paraméterrel)
    public void OnSlotClicked(int slotIndex)
    {
        if (GameManager.Instance == null) return;

        if (isLoadMode)
        {
            // Ha a főmenüben vagyunk: BETÖLTÉS
            GameManager.Instance.LoadFromSlot(slotIndex);
        }
        else
        {
            // Ha a játékban (Pause menüben) vagyunk: MENTÉS
            GameManager.Instance.SaveToSlot(slotIndex);
            
            // Frissítjük a feliratokat, hogy a játékos azonnal lássa a mostani dátumot!
            RefreshSlots(); 
        }
         gameObject.SetActive(false); 
    }
}