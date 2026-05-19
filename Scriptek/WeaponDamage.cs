using UnityEngine;
using System.Collections.Generic;

public class WeaponDamage : MonoBehaviour
{
    [Header("Beállítások")]
    public float damage = 25f;
    public Collider damageCollider; 

    [Tooltip("Ennek pontosan egyeznie kell az Inventoryban lévő tárgy ID-jával! (pl. Sword vagy Axe)")]
    public string weaponItemID = "Sword"; // <-- ÚJ: Hogy tudjuk, mit koptassunk

    [Tooltip("Mennyit kopjon egy sikeres találatkor?")]
    public float durabilityCost = 1f;     // <-- ÚJ: Kopás mértéke

    // Lista, hogy egy suhintással ne sebezze meg többször ugyanazt
    private List<GameObject> hitEnemies = new List<GameObject>();

    private void Start()
    {
        if (damageCollider == null) 
            damageCollider = GetComponent<Collider>();
            
        if (damageCollider != null)
        {
            damageCollider.enabled = false; 
            damageCollider.isTrigger = true; 
        }
        else
        {
            Debug.LogError("HIBA: Nincs Collider a kardon!");
        }
    }

    public void EnableDamage()
    {
        // Debug.Log(">>> KARD HITBOX BEKAPCSOLVA <<<");
        hitEnemies.Clear();
        if (damageCollider != null) damageCollider.enabled = true;
    }

    public void DisableDamage()
    {
        // Debug.Log("<<< KARD HITBOX KIKAPCSOLVA >>>");
        if (damageCollider != null) damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"[Kard Fizika] Érintés: '{other.name}'");

        // 1. ELLENSÉG TALÁLAT
        if (other.CompareTag("Enemy")) 
        {
            EnemyController enemy = other.GetComponentInParent<EnemyController>();

            if (enemy != null)
            {
                // Ha ezt az ellenséget ebben a suhintásban még nem találtuk el
                if (!hitEnemies.Contains(enemy.gameObject))
                {
                    hitEnemies.Add(enemy.gameObject); // Hozzáadjuk, hogy többször ne sebezze
                    
                    // Sebzés
                    enemy.TakeDamage(damage);
                    Debug.Log($"[Kard] TALÁLAT! {enemy.name} sebződik: {damage}");

                    // --- ÚJ RÉSZ: TARTÓSSÁG CSÖKKENTÉSE ---
                    if (InventoryManager.Instance != null)
                    {
                        // Meghívjuk az InventoryManagert, hogy vonjon le a tartósságból
                        InventoryManager.Instance.DecreaseItemDurability(weaponItemID, durabilityCost);
                    }
                    // ---------------------------------------
                }
            }
        }
        // 2. PUZZLE TALÁLAT
        else if (other.CompareTag("Puzzle"))
        {
            RunePillar rune = other.GetComponent<RunePillar>();
            if (rune != null)
            {
                rune.HitByWeapon();
                // Itt döntsd el: A puzzle ütése is koptatja a kardot?
                // Ha igen, ide is másold be a fenti 4 soros blokkot.
            }
        }

        // 3. FA VÁGÁS (ÚJ RÉSZ)
        else if (other.CompareTag("Tree"))
        {
            // Ellenőrizzük, hogy ez a fegyver alkalmas-e favágásra?
            // (Pl. az ID-ben benne van, hogy "axe", vagy ToolType alapján)
            // Egyszerűsítve: Ha az ID "stone_axe" vagy hasonló.
            
            // Itt most feltételezzük, hogy bármivel meg lehet ütni, de a logika bővíthető
            if (weaponItemID.ToLower().Contains("axe")) 
            {
                ChoppableTree tree = other.GetComponent<ChoppableTree>();
                if (tree != null)
                {
                    tree.TakeDamage(1f); // 1 ütés = 1 sebzés a fának
                    Debug.Log("Megütöttél egy fát!");
                    
                    // A balta is kopik!
                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.DecreaseItemDurability(weaponItemID, durabilityCost);
                    }
                }
            }
            else
            {
                Debug.Log("Ezzel nem tudsz fát vágni! (Balta kell)");
            }
        }
         else if (other.CompareTag("Plant"))
        {
            // Ellenőrizzük, hogy vágóeszköz-e? (Kard vagy Balta)
            // (Vagy ha minden fegyverrel lehet, akkor ez a feltétel nem is kell)
            if (weaponItemID.ToLower().Contains("axe") || weaponItemID.ToLower().Contains("sword"))
            {
                // Ugyanazt a ChoppableTree scriptet használjuk, mert az van rajta!
                ChoppableTree plant = other.GetComponent<ChoppableTree>();
                
                if (plant != null)
                {
                    plant.TakeDamage(100f); // Azonnal kivágjuk (nagy sebzés)
                    Debug.Log("Levágtál egy növényt!");

                    // A fegyver kopik
                    if (InventoryManager.Instance != null)
                    {
                        // Kisebb kopás, mint a fánál (pl. 0.5 vagy 0.2)
                        InventoryManager.Instance.DecreaseItemDurability(weaponItemID, durabilityCost * 0.5f);
                    }
                }
            }
        }

    }
}