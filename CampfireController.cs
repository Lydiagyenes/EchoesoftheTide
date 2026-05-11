using UnityEngine;
using System.Collections;
using GDS.Core;
using UnityEngine.UI;

public class CampfireController : MonoBehaviour
{
    [Header("Tűz Tulajdonságai")]
    public float burnTime = 60f;
    public float staminaRegenMultiplier = 2.0f; 
    public float cookingTime = 3.0f;[Header("Vizuális")]
    public GameObject fireParticles;
    public Light fireLight;

    private bool isPlayerNearby = false;
    private bool isCookingNow = false;

    private PlayerMovement playerMovement;
    private Animator playerAnimator;

    private void Start()
    {
        StartCoroutine(BurnDownRoutine());
    }

    private void Update()
    {
        if (isPlayerNearby && !isCookingNow && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[Campfire] 'C' gomb észlelve! Indítom a rutint...");
            StartCoroutine(CookMeatRoutine());
        }
    }

    private IEnumerator CookMeatRoutine()
    {
        // 1. MANAGER ELLENŐRZÉS (Itt szokott elcsúszni!)
        if (InventoryManager.Instance == null) 
        {
            Debug.LogError("[Campfire] HIBA: Nincs InventoryManager a pályán!");
            yield break;
        }
        
        if (CookingUIManager.Instance == null)
        {
            Debug.LogError("[Campfire] HIBA: Nincs CookingUIManager a pályán! (Tedd rá a szkriptet a CookingPanelre!)");
            yield break;
        }

        string rawID = MyItemDatabase.RawMeat.Id;

        // 2. HÚS ELLENŐRZÉS
        int amount = InventoryManager.Instance.GetItemAmount(rawID);
        Debug.Log($"[Campfire] Hús ellenőrzése. Keresett ID: {rawID}, Talált mennyiség: {amount}");

        if (amount <= 0)
        {
            Debug.Log("[Campfire] Nincs nálad nyers hús, ezért nem sütünk.");
            yield break; 
        }

        // --- SÜTÉS KEZDETE ---
        isCookingNow = true;

        if (playerMovement != null) playerMovement.canMove = false;
        if (playerAnimator != null) playerAnimator.SetBool("isCooking", true);

        // UI megjelenítése a Manageren keresztül
        CookingUIManager.Instance.panel.SetActive(true);
        CookingUIManager.Instance.slider.value = 0;
        CookingUIManager.Instance.slider.maxValue = cookingTime;

        float timer = 0f;
        while (timer < cookingTime)
        {
            timer += Time.deltaTime;
            CookingUIManager.Instance.slider.value = timer;
            
            if (playerMovement == null) break; 
            yield return null; 
        }

        // --- SÜTÉS VÉGE ---
        if (playerMovement != null && InventoryManager.Instance.GetItemAmount(rawID) > 0)
        {
            InventoryManager.Instance.RemoveItems(rawID, 1);
            InventoryManager.Instance.AddItemToInventory(MyItemDatabase.CookedMeat, 1);
            Debug.Log("[Campfire] Hús megsütve és cserélve!");
        }

        if (playerAnimator != null) playerAnimator.SetBool("isCooking", false);
        if (playerMovement != null) playerMovement.canMove = true;
        
        CookingUIManager.Instance.panel.SetActive(false);

        isCookingNow = false;
    }

    IEnumerator BurnDownRoutine()
    {
        yield return new WaitForSeconds(burnTime);
        // Debug.Log("A tábortűz kialudt."); // Kivettem, hogy ne spamaljon
        
        if (fireParticles) fireParticles.SetActive(false);
        if (fireLight) fireLight.enabled = false;
        
        GetComponent<Collider>().enabled = false; 

        if (isCookingNow)
        {
            if (playerMovement != null) playerMovement.canMove = true;
            if (playerAnimator != null) playerAnimator.SetBool("isCooking", false);
            if (CookingUIManager.Instance != null) CookingUIManager.Instance.panel.SetActive(false);
        }
        
        Destroy(gameObject, 5f); 
    }

    private void OnTriggerEnter(Collider other)
    {
        // CSAK A JÁTÉKOST FIGYELJÜK! (A kardot ignoráljuk)
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Campfire] Játékos belépett a körbe.");
            isPlayerNearby = true;
            playerMovement = other.GetComponent<PlayerMovement>();
            playerAnimator = other.GetComponent<Animator>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Campfire] Játékos kilépett.");
            isPlayerNearby = false;
            playerMovement = null;
            playerAnimator = null;
        }
    }
}