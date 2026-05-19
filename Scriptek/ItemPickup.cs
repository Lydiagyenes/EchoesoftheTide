using UnityEngine;
using GDS.Core; 

public class ItemPickup : MonoBehaviour
{
    [Header("1. MÓD: Fix Tárgy (pl. Kő)")]
    public string itemID;
    public int quantity = 1;

    [Header("2. MÓD: Botanika (Sorsolás)")]
    public bool isBotanyPlant = false;
    public PlantSourceColor plantColor;
    public PlantType plantType;

    [Header("Skill Visuals")]
    // Ide húzd be a BotanistFX gyerekobjektumot (a Particle Systemet)!
    public GameObject botanistFX; 

    private bool isPickingUp = false;

    // Induláskor ellenőrizzük a skillt
    private void Start()
    {
        CheckBotanistSkill();
    }

    private void OnEnable()
    {
        isPickingUp = false;
        
        // Ha bokor, és újra engedélyezzük, visszaállítunk mindent
        if (isBotanyPlant)
        {
            GetComponent<Collider>().enabled = true;
            
            // Minden gyereket visszakapcsolunk (bogyókat is)
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }

            // DE: Az effektet újraellenőrizzük, hogy kell-e látszódnia
            CheckBotanistSkill();
        }
    }

    // --- ÚJ FÜGGVÉNY: Skill ellenőrzés és effekt kapcsolás ---
    private void CheckBotanistSkill()
    {
        // Csak növényeknél és ha van beállítva effekt
        if (isBotanyPlant && botanistFX != null)
        {
            // Alapból kikapcsoljuk (biztonság kedvéért)
            botanistFX.SetActive(false);

            // Ha megvan a skill, bekapcsoljuk
            // FONTOS: Ellenőrizd, hogy a Skill fájlodban pontosan mi az ID! 
            // (Pl. "survivor_botany_1" vagy "Tapasztalt_Botanikus")
            if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Tapasztalt_Botanikus")) 
            {
                botanistFX.SetActive(true);
            }
        }
    }
    // ---------------------------------------------------------

    public void Interact()
    {
        if (isPickingUp) return;
        isPickingUp = true;

        string cleanID = itemID != null ? itemID.Trim() : "";
        
        Debug.Log($"Interacting with: {gameObject.name}");

        try 
        {
            ItemBase itemBaseToPickup = null;
            int finalQuantity = quantity;

            // --- SKILL: A TERMÉSZET KAMRÁJA ---
            // (Itt az ID-t a te kódod alapján hagytam: "A_Termeszet_Kamraja")
            if (isBotanyPlant && SkillManager.Instance != null && SkillManager.Instance.HasSkill("A_Termeszet_Kamraja"))
            {
                if (Random.value <= 0.25f)
                {
                    finalQuantity *= 2;
                    Debug.Log("<color=green>TERMÉSZET KAMRÁJA! Dupla mennyiséget találtál!</color>");
                }
            }
            // ----------------------------------

            if (isBotanyPlant)
            {
                if (BotanyManager.Instance != null)
                {
                    BotanyItem randomPlant = BotanyManager.Instance.GetRandomPlant(plantColor, plantType);
                    if (randomPlant != null) itemBaseToPickup = randomPlant.CreateRuntimeItem();
                }
            }
            else
            {
                itemBaseToPickup = GetItemBaseFromDatabase(cleanID);
                if (itemBaseToPickup == null) 
                    Debug.LogError($"HIBA: A '{cleanID}' ID-jű tárgy nincs az adatbázisban!");
            }

            if (itemBaseToPickup != null)
            {
                GDS.Core.Events.EventBus.Global.Publish(new ItemPickedUp(itemBaseToPickup, finalQuantity));
                Debug.Log($"Sikeres felvétel: {itemBaseToPickup.Name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Váratlan hiba: " + e.Message);
        }
        finally
        {
             var saveComp = GetComponent<PickupSaveData>();
            if (saveComp != null)
            {
                saveComp.CompletePickup();
            }
            else
            {
                if (!isBotanyPlant) Debug.LogWarning($"A {gameObject.name} tárgyon nincs PickupSaveData script, ezért visszajön betöltéskor!");
            }
            
            if (isBotanyPlant)
            {
                HandleHarvesting();
            }
            else
            {
                Debug.Log($"TÁRGY TÖRLÉSE: {gameObject.name}");
                Destroy(gameObject); 
            }
        }
    }

    private void HandleHarvesting()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

       // var highlight = GetComponent<GDS.Common.Scripts.IHighlight>();
       // if (highlight != null) highlight.Unhighlight();

        // --- ÚJ: Ha leszüreteltük, kapcsoljuk ki a csillogást is! ---
        /*if (botanistFX != null) 
        {
            botanistFX.SetActive(false);
        }*/
        // ------------------------------------------------------------

        var hlScript = GetComponent<HighlightObject>();
        if (hlScript != null) hlScript.Unhighlight();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            
            // Ha ez a gyerek véletlenül a Highlight Effekt (amit a másik script hozott létre),
            // akkor békén hagyjuk, vagy azt a HighlightObject script kezeli.
            if (child.GetComponent<ParticleSystem>() != null) continue;

            string childName = child.name.ToLower();

            if (childName.Contains("berry") || childName.Contains("sphere") || childName.Contains("mesh"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private GDS.Core.ItemBase GetItemBaseFromDatabase(string id)
    {
        // --- Eszközök és Fegyverek ---
        if (id == MyItemDatabase.Axe.Id) return MyItemDatabase.Axe;
        if (id == MyItemDatabase.ReinforcedAxe.Id) return MyItemDatabase.ReinforcedAxe;
        if (id == MyItemDatabase.Sword.Id) return MyItemDatabase.Sword;
        if (id == MyItemDatabase.ReinforcedSword.Id) return MyItemDatabase.ReinforcedSword;
        if (id == MyItemDatabase.Hammer.Id) return MyItemDatabase.Hammer;
        if (id == MyItemDatabase.Lamp.Id) return MyItemDatabase.Lamp;

        // --- Nyersanyagok ---
        if (id == MyItemDatabase.WoodLog.Id) return MyItemDatabase.WoodLog;
        if (id == MyItemDatabase.WoodBranch.Id) return MyItemDatabase.WoodBranch;
        if (id == MyItemDatabase.WoodPlank.Id) return MyItemDatabase.WoodPlank;
        if (id == MyItemDatabase.GiantLog.Id) return MyItemDatabase.GiantLog;
        if (id == MyItemDatabase.Campfire.Id) return MyItemDatabase.Campfire;
        if (id == MyItemDatabase.Stone.Id) return MyItemDatabase.Stone;
        if (id == MyItemDatabase.Flint.Id) return MyItemDatabase.Flint;
        if (id == MyItemDatabase.SharpStone.Id) return MyItemDatabase.SharpStone;
        if (id == MyItemDatabase.PlantFiber.Id) return MyItemDatabase.PlantFiber;
        if (id == MyItemDatabase.SilkGrass.Id) return MyItemDatabase.SilkGrass;
        if (id == MyItemDatabase.StrongCanvas.Id) return MyItemDatabase.StrongCanvas;
        if (id == MyItemDatabase.Resin.Id) return MyItemDatabase.Resin;
        if (id == MyItemDatabase.PlantTar.Id) return MyItemDatabase.PlantTar;
        if (id == MyItemDatabase.MetalScrap.Id) return MyItemDatabase.MetalScrap;
        if (id == MyItemDatabase.LeatherStrap.Id) return MyItemDatabase.LeatherStrap;

        // --- Állati Eredetű (Loot) ---
        if (id == MyItemDatabase.RawMeat.Id) return MyItemDatabase.RawMeat;
        if (id == MyItemDatabase.CookedMeat.Id) return MyItemDatabase.CookedMeat;
        if (id == MyItemDatabase.Bone.Id) return MyItemDatabase.Bone;
        if (id == MyItemDatabase.WolfSkin.Id) return MyItemDatabase.WolfSkin;

        // --- Fogyasztható ---
        if (id == MyItemDatabase.EmptyFlask.Id) return MyItemDatabase.EmptyFlask;
        if (id == MyItemDatabase.WaterFlask.Id) return MyItemDatabase.WaterFlask;
        if (id == MyItemDatabase.DarkWaterFlask.Id) return MyItemDatabase.DarkWaterFlask;
        if (id == MyItemDatabase.Bread.Id) return MyItemDatabase.Bread;
        if (id == MyItemDatabase.Antidote.Id) return MyItemDatabase.Antidote;
        if (id == MyItemDatabase.HealingPotion.Id) return MyItemDatabase.HealingPotion;
        if (id == MyItemDatabase.EndurancePotion.Id) return MyItemDatabase.EndurancePotion;

        // --- Történet és Quest Tárgyak ---
        
        if (id == MyItemDatabase.EliasCompass.Id) return MyItemDatabase.EliasCompass;
        if (id == MyItemDatabase.EleanorsLocket.Id) return MyItemDatabase.EleanorsLocket;
        if (id == MyItemDatabase.BrokenDagger.Id) return MyItemDatabase.BrokenDagger;
        if (id == MyItemDatabase.EchoShard.Id) return MyItemDatabase.EchoShard;
        if (id == MyItemDatabase.ThornsTalisman.Id) return MyItemDatabase.ThornsTalisman;
        if (id == MyItemDatabase.quest_2.Id) return MyItemDatabase.quest_2;

        return null;
    }
}