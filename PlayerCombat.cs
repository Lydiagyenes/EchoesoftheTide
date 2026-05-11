using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Fegyver Modellek (Húzd be a gyerekobjektumokat!)")]
    public GameObject swordModel; // "Equipped_Sword"
    public GameObject axeModel;   // "Equipped_Axe"

    [Header("Beállítások")]
    public string swordItemID = "Sword"; // MyItemDatabase ID
    public string axeItemID = "Axe";     // MyItemDatabase ID
    public float attackCooldown = 1.0f;
    public float staminaCost = 15f;

    [Header("Referenciák")]
    public Animator animator;

    // --- BELSŐ VÁLTOZÓK ---
    private int currentWeaponIndex = 0; // 0: Üres kéz, 1: Kard, 2: Balta
    private WeaponDamage activeWeaponDamage; // Az épp használt fegyver sebzése
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    void Start()
    {
        // Induláskor elrejtünk mindent, hogy tiszta lappal induljunk
        // (A játékosnak majd elő kell vennie az 1-es vagy 2-es gombbal)
        if (swordModel) swordModel.SetActive(false);
        if (axeModel) axeModel.SetActive(false);
        currentWeaponIndex = 0;
        activeWeaponDamage = null;
    }

    void Update()
    {
        // Ha megállítva van a játék, vagy inventoryban matatunk, ne csináljunk semmit
        if (Time.timeScale == 0) return; 
        // (Ha van GameManager.isInventoryOpen, azt is írd ide: || GameManager.isInventoryOpen)

        // --- FEGYVER VÁLTÁS (1, 2) ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(1); // Kard
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(2); // Balta
        if (Input.GetKeyDown(KeyCode.X)) EquipWeapon(0);      // X = Elrakás (opcionális)

        // --- TÁMADÁS (Bal Klikk) ---
        // Feltételek: Bal klikk + Van fegyver a kézben + Cooldown lejárt + Nem támadunk épp
        if (Input.GetButtonDown("Fire1") && currentWeaponIndex != 0 && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            AttemptAttack();
        }
    }

    // --- FŐ LOGIKA: FEGYVER ELŐVÉTELE ---
    public void EquipWeapon(int index)
    {
        // Ha nincs Inventory Manager, nem tudjuk ellenőrizni, van-e fegyver
        if (InventoryManager.Instance == null) return;

        bool hasItem = false;

        // 1. Ellenőrizzük, hogy megvan-e a fegyver az inventoryban
        switch (index)
        {
            case 0: // Üres kéz (Mindig lehetséges)
                hasItem = true;
                break;
            case 1: // Kard
                hasItem = InventoryManager.Instance.GetItemAmount(swordItemID) > 0;
                break;
            case 2: // Balta
                hasItem = InventoryManager.Instance.GetItemAmount(axeItemID) > 0;
                break;
        }

        // 2. Ha megvan, beállítjuk
        if (hasItem)
        {
            // Ha ugyanazt nyomtuk meg, ami már a kézben van, akkor elrakjuk (toggle) - Opcionális
            if (currentWeaponIndex == index && index != 0) 
            {
                index = 0; 
            }

            currentWeaponIndex = index;
            UpdateWeaponVisibility();
            
            // UI visszajelzés (opcionális)
            if (index != 0) Debug.Log($"[Combat] Fegyver elővétele: {index}");
            else Debug.Log("[Combat] Fegyver elrakva.");
        }
        else
        {
            Debug.Log($"[Combat] Nincs nálad ez a fegyver! (Index: {index})");
        }
    }

    void UpdateWeaponVisibility()
    {
        // Először mindent elrejtünk
        if (swordModel) swordModel.SetActive(false);
        if (axeModel) axeModel.SetActive(false);
        
        activeWeaponDamage = null;

        // Aztán csak a kiválasztottat jelenítjük meg
        switch (currentWeaponIndex)
        {
            case 1:
                if (swordModel)
                {
                    swordModel.SetActive(true);
                    activeWeaponDamage = swordModel.GetComponent<WeaponDamage>();
                }
                break;
            case 2:
                if (axeModel)
                {
                    axeModel.SetActive(true);
                    activeWeaponDamage = axeModel.GetComponent<WeaponDamage>();
                }
                break;
        }
    }

    // --- TÁMADÁS LOGIKA (A régiből átmentve) ---
    void AttemptAttack()
    {
        if (PlayerStats.Instance != null)
        {
            if (PlayerStats.Instance.currentStamina >= staminaCost)
            {
                PlayerStats.Instance.ConsumeStamina(staminaCost);
                PerformAttack();
            }
            else
            {
                Debug.Log("Nincs elég staminád!");
            }
        }
        else
        {
            // Ha nincs stats rendszer, akkor is engedjük (teszteléshez)
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Animáció indítása
        if (animator != null)
        {
            // Ha akarunk külön animációt a baltának és a kardnak:
            // if (currentWeaponIndex == 2) animator.SetTrigger("AttackAxe");
            // else animator.SetTrigger("Attack");
            
            // Egyelőre maradjon a közös:
            animator.SetTrigger("Attack");
        }
    }

    // --- ANIMATION EVENTEK (A régiből, de dinamikussá téve) ---
    
    // Figyelem: A 'CombatEvents' szkripted (ami a modellen van) ezeket hívja!
    // A neveknek pontosan egyezniük kell: OpenWeaponHitbox, CloseWeaponHitbox.

    public void OpenWeaponHitbox()
    {
        if (activeWeaponDamage != null)
        {
            activeWeaponDamage.EnableDamage();
        }
    }

    public void CloseWeaponHitbox()
    {
        if (activeWeaponDamage != null)
        {
            activeWeaponDamage.DisableDamage();
        }
        isAttacking = false; // Támadás vége
    }
}