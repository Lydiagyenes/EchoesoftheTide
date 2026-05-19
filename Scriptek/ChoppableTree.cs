using UnityEngine;

public class ChoppableTree : MonoBehaviour
{
    [Header("Fa Beállítások")]
    public float treeHealth = 3f; // Hány ütést bír ki
    public GameObject logPrefab;  // Mit dobjon, ha kivágták (ItemPickup prefab)
    public int logsAmount = 3;    // Hány darabot dobjon

    [Header("Effektek")]
    // Ide rakhatsz hangot vagy faforgács effektet később
    public GameObject hitEffect; 

    public void TakeDamage(float damage)
    {
        treeHealth -= damage;
        
        // Opcionális: Megrázkódik a fa ütéskor
        // transform.DOShake... (ha lenne DoTween), vagy egyszerű animáció

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);
        }

        if (treeHealth <= 0)
        {
            TreeFall();
        }
    }

    private void TreeFall()
    {
        Debug.Log("A fa kidőlt!");
        
        // 1. Létrehozzuk a faanyagot (Loot)
        if (logPrefab != null)
        {
            for (int i = 0; i < logsAmount; i++)
            {
                // Kicsit szórjuk szét őket
                Vector3 spawnPos = transform.position + Vector3.up + Random.insideUnitSphere * 1.5f;
                spawnPos.y = transform.position.y + 0.5f; // Ne a föld alatt legyen
                
                Instantiate(logPrefab, spawnPos, Quaternion.identity);
            }
        }

        // 2. Eltüntetjük a fát
        // (Később itt lehet kidőlés animációt csinálni, most egyszerűen megsemmisítjük)
        Destroy(gameObject);
    }
}