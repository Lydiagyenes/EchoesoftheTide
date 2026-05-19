using UnityEngine;
using GDS.Core; 
using GDS.Core.Events;
using System.Collections.Generic; // Kell a Listához


public class InventoryManager : MonoBehaviour, ISaveable
{
    public static InventoryManager Instance { get; private set; }
    public ListBag MainInventory { get; private set; }
    
    private void Awake() {
        // --- JAVÍTÁS: BIZTONSÁGOS DUPLIKÁCIÓ KEZELÉS ---
        
        if (Instance != null && Instance != this)
        {
            // FONTOS: Ha ez egy gyerekobjektum, NEM szabad a gameObject-et törölni,
            // mert akkor eltűnik a UI! Csak a scriptet kapcsoljuk ki, 
            // és hagyjuk, hogy a szülő (_GameSystems) intézze a törlést, ha ő is duplikált.
            
            Debug.LogWarning("InventoryManager duplikátumot találtam! Kikapcsolom ezt a komponenst.");
            Destroy(this); // Csak a scriptet töröljük, nem az objektumot!
            return;
            
        }
        
        Instance = this;
        
        // DontDestroyOnLoad NEM KELL, mert a szülő (_GameSystems) viszi magával!
        // -----------------------------------------------

        MainInventory = ListBagFactory.Create<ListBag>("MainInventory", 20);
    }

    private void Start()
    {
        // JAVÍTÁS: A segédfüggvényt adjuk át
        EventBus.Global.On<ItemPickedUp>(OnItemPickedUpWrapper);
    }

    private void OnDestroy()
    {
        // JAVÍTÁS: 'Off' a helyes parancs 'Unregister' helyett!
        if (Instance == this) 
        {
            EventBus.Global.Off<ItemPickedUp>(OnItemPickedUpWrapper);
        }
    }

    // --- JAVÍTÁS: Ez a "csomagoló" függvény kell a típuskonverzió miatt ---
    // Az EventBus 'CustomEvent'-et küld, ezt alakítjuk át 'ItemPickedUp'-ra
    private void OnItemPickedUpWrapper(GDS.Core.Events.CustomEvent e)
    {
        OnItemPickedUp((ItemPickedUp)e);
    }
    // ---------------------------------------------------------------------
    
    private void OnItemPickedUp(ItemPickedUp e) {
        AddItemToInventory(e.ItemBase, e.Quantity);
        
        Debug.Log($"Felvettél: {e.Quantity} db {e.ItemBase.Id}.");
    }
    public void SaveData(ref GameData data)
    {
        // 1. Töröljük az előző mentett inventoryt
        data.inventoryContents.Clear();

        // 2. Végigmegyünk a slotokon
        foreach (var slot in MainInventory.Slots)
        {
            if (!slot.IsEmpty())
            {
                // Létrehozzuk az adatcsomagot
                GameData.SavedItemData itemData = new GameData.SavedItemData
                {
                    itemID = slot.Item.Base.Id,
                    amount = slot.Item.Quant,
                    slotIndex = slot.Index,
                    currentDurability = slot.Item.CurrentDurability 
                };
                
                // Hozzáadjuk a listához
                data.inventoryContents.Add(itemData);
            }
        }
        Debug.Log("[InventoryManager] Inventory mentve. Tárgyak száma: " + data.inventoryContents.Count);
    }

   public void LoadData(GameData data)
    {
        MainInventory.Clear();
        foreach (var savedItem in data.inventoryContents)
        {
            // 1. Alap Item létrehozása
            ItemBase itemBase = FindItemBaseByID(savedItem.itemID);

            if (itemBase != null)
            {
                // 2. JAVÍTÁS: Tartósság kiderítése ID alapján (nem típuskényszerítéssel)
                float loadedMaxDurability = 0;
                
                if (CraftingManager.Instance != null)
                {
                    // ID alapján keressük a ToolItem adatfájlt
                    ToolItem toolData = CraftingManager.Instance.GetToolByID(savedItem.itemID);
                    if (toolData != null)
                    {
                        loadedMaxDurability = toolData.maxDurability;
                    }
                }

                // 3. Item összerakása
                Item item = new Item() 
                { 
                    Base = itemBase, 
                    Quant = savedItem.amount,
                    CurrentDurability = savedItem.currentDurability,
                    MaxDurability = loadedMaxDurability
                };

                // Biztonsági feltöltés (ha régi mentés volt 0 max értékkel)
                if (item.MaxDurability > 0 && item.CurrentDurability <= 0.1f) 
                {
                    item.CurrentDurability = item.MaxDurability;
                }

                // 4. Helyére rakás
                if (savedItem.slotIndex < MainInventory.Slots.Count)
                {
                    MainInventory.Slots[savedItem.slotIndex] = MainInventory.Slots[savedItem.slotIndex] with { Item = item };
                }
            }
        }
        MainInventory.Data.Notify();
    }
    // --- SEGÉDFÜGGVÉNY: ID -> Tárgy keresés ---
    public ItemBase FindItemBaseByID(string id)
    {
        // 1. Megnézzük a Botanika adatbázisban (ha van Manager)
        if (BotanyManager.Instance != null)
        {
            // A te BotanyManageredben ez a függvény a GetPlantByID
            // De az BotanyItem-et ad vissza, nekünk ItemBase kell.
            // A BotanyItem-nek van CreateRuntimeItem() függvénye!
            var plant = BotanyManager.Instance.GetPlantByID(id);
            if (plant != null)
            {
                return plant.CreateRuntimeItem();
            }
        }

        // 2. Megnézzük a fix tárgyak között (Kő, Kard, stb.)
        // Itt sorold fel az összes fix tárgyadat!
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
        if(id == MyItemDatabase.quest_2.Id) return MyItemDatabase.quest_2;
        if (id == MyItemDatabase.JournalItem.Id) return MyItemDatabase.JournalItem;

        if (CraftingManager.Instance != null)
        {
            var tool = CraftingManager.Instance.GetToolByID(id);
            if (tool != null)
            {
                return tool.CreateRuntimeItem();
            }
        }

        return null; // Nem találtuk
    }
    
    // 1. Megszámolja, mennyi van egy adott tárgyból összesen
    public int GetItemAmount(string itemID)
    {
        int total = 0;
        foreach (var slot in MainInventory.Slots)
        {
            if (!slot.IsEmpty() && slot.Item.Base.Id == itemID)
            {
                total += slot.Item.Quant;
            }
        }
        return total;
    }

    // 2. Kivesz a táskából X darabot (több slotból is, ha kell)
    public bool RemoveItems(string itemID, int amountToRemove)
    {
        // 1. Van elég összesen?
        if (GetItemAmount(itemID) < amountToRemove) return false;

        int remainingToRemove = amountToRemove;

        // 2. Kigyűjtjük azokat a slot indexeket, ahol ez a tárgy van
        List<int> candidateSlots = new List<int>();
        for (int i = 0; i < MainInventory.Slots.Count; i++)
        {
            if (!MainInventory.Slots[i].IsEmpty() && MainInventory.Slots[i].Item.Base.Id == itemID)
            {
                candidateSlots.Add(i);
            }
        }

        // 3. SORBARENDEZÉS: A legkisebb CurrentDurability legyen elöl!
        // Így a "Javítás" recept mindig a töröttet veszi el, nem az újat.
        candidateSlots.Sort((a, b) => 
        {
            float durA = MainInventory.Slots[a].Item.CurrentDurability;
            float durB = MainInventory.Slots[b].Item.CurrentDurability;
            return durA.CompareTo(durB); // Növekvő sorrend (kicsi -> nagy)
        });

        // 4. Törlés a rendezett lista alapján
        foreach (int slotIndex in candidateSlots)
        {
            var slot = MainInventory.Slots[slotIndex];
            int inSlot = slot.Item.Quant;

            if (inSlot > remainingToRemove)
            {
                // Részleges levonás
                GDS.Core.Item newItem = slot.Item;
                newItem.Quant -= remainingToRemove;
                MainInventory.Slots[slotIndex] = MainInventory.Slots[slotIndex] with { Item = newItem };
                remainingToRemove = 0;
            }
            else
            {
                // Teljes slot törlés
                remainingToRemove -= inSlot;
                MainInventory.RemoveItem(slot);
            }

            if (remainingToRemove <= 0) break;
        }
        
        MainInventory.Data.Notify();
        Debug.Log($"Sikeresen elvettünk {amountToRemove} db {itemID}-t (a legkopottabbakat priorizálva).");
        return true;
    }
    // --- TARTÓSSÁG CSÖKKENTÉSE ---
    public void DecreaseItemDurability(string itemID, float amount)
    {
        // Végigmegyünk a slotokon, hogy megtaláljuk az eszközt
        for (int i = 0; i < MainInventory.Slots.Count; i++)
        {
            var slot = MainInventory.Slots[i];

            if (!slot.IsEmpty() && slot.Item.Base.Id == itemID)
            {
                // Megvan az eszköz! (Feltételezzük, hogy az első találat az, amit használsz)
                GDS.Core.Item item = slot.Item;

                // Csak akkor vonunk le, ha van tartóssága (tehát szerszám)
                if (item.MaxDurability > 0)
                {
                    item.CurrentDurability -= amount;

                    // Ha elfogyott a tartósság -> Eltörik
                    if (item.CurrentDurability <= 0)
                    {
                        Debug.Log($"[InventoryManager] A(z) {item.Base.Name} eltört!");
                        MainInventory.RemoveItem(slot);
                        
                        // Opcionális: Lejátszhatsz itt egy törés hangot is
                    }
                    else
                    {
                        // Ha még bírja -> Frissítjük az állapotot
                        MainInventory.Slots[i] = MainInventory.Slots[i] with { Item = item };
                        // Debug.Log($"Tartósság csökkent: {item.CurrentDurability}/{item.MaxDurability}");
                    }

                    // Értesítjük a UI-t, hogy frissítse a zöld csíkot
                    MainInventory.Data.Notify();
                    return; // Csak egy eszközt koptatunk egyszerre, kilépünk.
                }
            }
        }
    }

// --- ÚJ FÜGGVÉNY: Tárgy elfogyasztása ---
    public void ConsumeItem(ListSlot slot)
    {
        if (slot.IsEmpty()) return;

        string itemID = slot.Item.Base.Id;
        bool itemConsumed = false; // Ezzel figyeljük, sikerült-e a fogyasztás

        // --- 1. ESET: NÖVÉNYEK (BOTANY) ---
        if (BotanyManager.Instance != null)
        {
            BotanyItem plantData = BotanyManager.Instance.GetPlantByID(itemID);

            if (plantData != null)
            {
                if (PlayerStats.Instance != null)
                {
                    // --- SKILL: GYÓGYFÜVES (Herbalist) ---
                    // Ez CSAK a növényekre vonatkozik, ahogy eddig is!
                    float multiplier = 1.0f;
                    if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Gyogyfuves"))
                    {
                        multiplier = 1.5f; 
                        Debug.Log("Gyógyfüves bónusz aktiválva! (+50% hatás)");
                    }
                    
                    // Élet hatás (Szorzóval)
                    if (plantData.healthEffect > 0)
                        PlayerStats.Instance.Heal(plantData.healthEffect * multiplier);
                    else
                        PlayerStats.Instance.TakeDamage(Mathf.Abs(plantData.healthEffect));

                    // Stamina hatás (Szorzóval az időtartamra)
                    if (plantData.staminaBuffDuration > 0)
                    {
                        PlayerStats.Instance.ApplyStaminaBuff(
                            plantData.staminaBuffDuration * multiplier, 
                            plantData.staminaRegenMultiplier
                        );
                    }
                }
                Debug.Log($"Megetted a növényt: {plantData.itemName}");
                itemConsumed = true; // Jelezzük, hogy sikeres volt
            }
        }

        // --- 2. ESET: ÉTELEK (FOOD) --- 
        // Csak akkor nézzük meg, ha még nem ettük meg növényként!
        if (!itemConsumed && FoodManager.Instance != null)
        {
            FoodItem foodData = FoodManager.Instance.GetFoodByID(itemID);

            if (foodData != null)
            {
                if (PlayerStats.Instance != null)
                {
                    // Itt NINCS Gyógyfüves szorzó, mert ez hús/étel!
                    
                    // Élet hatás
                    if (foodData.healthEffect > 0) 
                        PlayerStats.Instance.Heal(foodData.healthEffect);
                    else 
                        PlayerStats.Instance.TakeDamage(Mathf.Abs(foodData.healthEffect));

                    // Stamina hatás
                    if (foodData.staminaBuffDuration > 0)
                    {
                        PlayerStats.Instance.ApplyStaminaBuff(foodData.staminaBuffDuration, foodData.staminaRegenMultiplier);
                    }
                }
                Debug.Log($"Megetted az ételt: {foodData.itemName}");
                itemConsumed = true; // Jelezzük, hogy sikeres volt
            }
        }
        if (!itemConsumed)
        {
            if (itemID == MyItemDatabase.Antidote.Id)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.CurePoison();
                    // Opcionális: Adhat egy kis HP-t is
                    PlayerStats.Instance.Heal(5); 
                }
                Debug.Log("Megittad az ellenmérget.");
                itemConsumed = true;
            }
        }

        // --- KÖZÖS TÖRLÉSI LOGIKA ---
        // Ha bármelyik fenti blokkban sikerült a fogyasztás, itt vonjuk le.
        if (itemConsumed)
        {
            GDS.Core.Item newItem = slot.Item;
            newItem.Quant -= 1;
            
            if (newItem.Quant <= 0)
            {
                MainInventory.RemoveItem(slot);
            }
            else
            {
                MainInventory.Slots[slot.Index] = MainInventory.Slots[slot.Index] with { Item = newItem };
            }
            
            MainInventory.Data.Notify();
        }
        else
        {
            // Ha se növény, se étel nem volt
            // Debug.Log("Ez a tárgy nem ehető."); 
        }
    }
   
    public void AddItemToInventory(ItemBase itemBase, int quantityToAdd)
    {
        // 1. Kapacitás ellenőrzés
        int effectiveCapacity = 15;
        if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Erosebb_Hatizsak"))
        {
            effectiveCapacity = 20;
        }

        int stackLimit = 1;
        if (itemBase.Stack != null && itemBase.Stack.Max > 0) 
            stackLimit = (int)itemBase.Stack.Max;

        float startDurability = 0;
        float maxDurability = 0;

        if (CraftingManager.Instance != null)
        {
            ToolItem toolData = CraftingManager.Instance.GetToolByID(itemBase.Id);
            if (toolData != null)
            {
                maxDurability = toolData.maxDurability;
                startDurability = maxDurability;

                // SKILL: MESTERI KOVÁCS (Master Blacksmith)
                if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Mesteri_Kovacs")) // Ellenőrizd az ID-t!
                {
                    // +25% tartósság
                    float bonus = startDurability * 0.25f;
                    startDurability += bonus;
                    maxDurability += bonus; // A maximumot is növeljük, hogy a csík teli legyen
                    Debug.Log("Mesteri Kovács bónusz: Erősebb eszközt kaptál!");
                }
            }
        }

        // 2. Stackelés (Meglévő helyekre) - JAVÍTOTT CIKLUS (For loop)
        for (int i = 0; i < MainInventory.Slots.Count; i++)
        {
            var slot = MainInventory.Slots[i]; // Így kérjük le az elemet

            if (slot.Index >= effectiveCapacity) continue;
            if (quantityToAdd <= 0) break;

            // ID alapú összehasonlítás
            if (!slot.IsEmpty() && slot.Item.Base.Id == itemBase.Id)
            {
                int spaceInSlot = stackLimit - slot.Item.Quant;
                if (spaceInSlot > 0)
                {
                    int amountToStack = Mathf.Min(spaceInSlot, quantityToAdd);
                    
                    // GDS frissítés
                    GDS.Core.Item newItem = slot.Item;
                    newItem.Quant += amountToStack;
                    MainInventory.Slots[slot.Index] = MainInventory.Slots[slot.Index] with { Item = newItem };
                    
                    quantityToAdd -= amountToStack;
                }
            }
        }
        
        MainInventory.Data.Notify();

        // 3. Új slotba helyezés
         if (quantityToAdd > 0)
        {
            while (quantityToAdd > 0)
            {
                int amountForNewSlot = Mathf.Min(stackLimit, quantityToAdd);
                
                // ITT A LÉNYEG:
                 GDS.Core.Item newItem = new GDS.Core.Item() 
                { 
                    Base = itemBase, 
                    Quant = amountForNewSlot,
                    CurrentDurability = startDurability, 
                    MaxDurability = maxDurability        
                };
                
                MainInventory.AddItems(newItem);
                quantityToAdd -= amountForNewSlot;
            }
        }
        
        // 4. Túlcsordulás ellenőrzése - JAVÍTOTT CIKLUS
        for (int i = effectiveCapacity; i < MainInventory.Slots.Count; i++)
        {
            var slot = MainInventory.Slots[i];
            if (!slot.IsEmpty())
            {
                Debug.LogWarning($"Tárgy került a lezárt slotra ({i}). Eldobjuk: {slot.Item.Base.Name}");
                DropItem(slot, slot.Item.Quant); 
            }
        }

         if (QuestLog.Instance != null)
        {
            QuestLog.Instance.CheckAutoCompleteQuests();
        }
    }

    // --- MÓDOSÍTOTT FÜGGVÉNY: Törlés a világból (Eldobás helyett) ---
  // Új paraméter: amountToDrop (alapból 1)
    public void DropItem(ListSlot slot, int amountToDrop = 1)
    {
        if (slot.IsEmpty()) return;
        
        if (amountToDrop > slot.Item.Quant) amountToDrop = slot.Item.Quant;

        Item item = slot.Item;
        GameObject prefabToSpawn = null;
        
        if (ItemPrefabDatabase.Instance != null)
        {
            prefabToSpawn = ItemPrefabDatabase.Instance.GetPrefab(item.Base.Id);
        }

        if (prefabToSpawn != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPos = transform.position; 
            Quaternion spawnRot = Quaternion.identity;

            if (playerObj != null)
            {
                // Eldöntjük, hogy építmény-e (Tábortűz)
                bool isBuilding = item.Base.Id == MyItemDatabase.Campfire.Id || prefabToSpawn.GetComponent<CampfireController>() != null;
                
                // A játékos elé tesszük (építményt 2 méterre, sima tárgyat 1 méterre)
                float dropDistance = isBuilding ? 2.0f : 1.0f;
                spawnPos = playerObj.transform.position + (playerObj.transform.forward * dropDistance);
                spawnRot = playerObj.transform.rotation;

                if (isBuilding)
                {
                    // --- ÉPÍTMÉNY LERAKÁSA (JAVÍTOTT RAYCAST) ---
                    
                    // 1. Sokkal magasabbról indítjuk a sugarat (A játékos feje felett 10 méterrel)
                    Vector3 rayStart = new Vector3(spawnPos.x, playerObj.transform.position.y + 10f, spawnPos.z); 

                    // 2. Csak a talajt akarjuk eltalálni! (Kizárjuk a Water és Player rétegeket)
                    // Feltételezem, hogy a talajod a "Default", "Ground" vagy "Terrain" rétegen van.
                    int groundMask = LayerMask.GetMask("Default", "Ground", "Terrain");

                    // Lefelé lőjük a sugarat, figyelmen kívül hagyva a Triggereket
                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 30f, groundMask, QueryTriggerInteraction.Ignore))
                    {
                        // 3. A hit.point a fű pontos felszíne. 
                        // Mivel a tábortűz pivotja valószínűleg a modell közepén van, emeljük meg 1 méterrel!
                        // (Ha a levegőben lebegne ezután, ezt az 1.0f-et írd át pl. 0.5f-re)
                        spawnPos = hit.point + (Vector3.up * 1.0f);
                        
                        Debug.Log($"[DropItem] Talaj megtalálva: {hit.collider.gameObject.name} | Új magasság: {spawnPos.y}");
                    }
                    else
                    {
                        Debug.LogWarning("[DropItem] A Raycast nem talált talajt! Lehet, hogy a talaj nincs a Default/Ground rétegen?");
                        // Ha nem talál talajt (pl. leestél a pályáról), legalább a játékos magasságába teszi
                        spawnPos.y = playerObj.transform.position.y;
                    }
                }
                else
                {
                    // --- SIMA TÁRGY ELDOBÁSA ---
                    spawnPos.y += 1.5f; 
                }
            }

            // 2. LÉTREHOZÁS A VILÁGBAN
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, spawnRot);

            // 3. FIZIKA BEÁLLÍTÁSA
            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
            bool isCampfire = spawnedObject.GetComponent<CampfireController>() != null;

            if (isCampfire)
            {
                // --- TÁBORTŰZ ESETÉN: LEBETONOZZUK! ---
                if (rb != null) 
                {
                    rb.isKinematic = true;  // NEM hat rá a fizika (nincs pattogás!)
                    rb.useGravity = false;  // NEM esik le
                }
                
                ItemPickup pickup = spawnedObject.GetComponent<ItemPickup>();
                if (pickup != null) Destroy(pickup);
            }
            else
            {
                // --- SIMA TÁRGY ESETÉN: LEEJTJÜK! ---
                if (rb != null) 
                {
                    rb.isKinematic = false; // Mozoghat
                    rb.useGravity = true;   // Leeshet
                    
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                ItemPickup pickupComp = spawnedObject.GetComponent<ItemPickup>();
                if (pickupComp != null)
                {
                    pickupComp.itemID = item.Base.Id;
                    pickupComp.quantity = amountToDrop; 
                    pickupComp.isBotanyPlant = false; 
                }
            }
            
            spawnedObject.SetActive(true);
            Debug.Log($"[DropItem] Létrehozva: {item.Base.Id}");
        }

        // 4. LEVONÁS AZ INVENTORY-BÓL
        GDS.Core.Item newItem = slot.Item;
        newItem.Quant -= amountToDrop;

        if (newItem.Quant <= 0) MainInventory.RemoveItem(slot);
        else
        {
            MainInventory.Slots[slot.Index] = MainInventory.Slots[slot.Index] with { Item = newItem };
            MainInventory.Data.Notify(); 
        }
    }
    // --- KATEGÓRIA ALAPÚ KERESÉS (CRAFTINGHOZ) ---

    // Megszámolja, hány darab van egy bizonyos típusból (pl. ehető bogyó)
    public int GetBotanyAmountByType(bool mustBePoisonous)
    {
        int total = 0;
        Debug.Log($"[InventoryManager] Keresés indítása. Keresett típus: {(mustBePoisonous ? "MÉRGEZŐ" : "EHETŐ")} BOGYÓK");

        foreach (var slot in MainInventory.Slots)
        {
            // Ha üres a slot, azonnal lépjünk a következőre
            if (slot.IsEmpty()) continue;

            // ITT HOZZUK LÉTRE A VÁLTOZÓT (Így az egész cikluson belül élni fog)
            string currentID = slot.Item.Base.Id;
            
            if (BotanyManager.Instance != null)
            {
                var plantData = BotanyManager.Instance.GetPlantByID(currentID);
                
                if (plantData != null)
                {
                    Debug.Log($"-> Tárgy vizsgálata: '{plantData.itemName}' (ID: {currentID}). Típus: {plantData.plantType}, Mérgező: {plantData.isPoisonous}");

                    bool typeMatch = plantData.plantType == PlantType.Berry;
                    bool poisonMatch = plantData.isPoisonous == mustBePoisonous;
                    bool isNotPotion = plantData.sourceColor != PlantSourceColor.Potion;

                    if (typeMatch && poisonMatch && isNotPotion)
                    {
                        Debug.Log($"   MATCH! Ez jó! Mennyiség: {slot.Item.Quant}");
                        total += slot.Item.Quant;
                    }
                    else
                    {
                        Debug.Log($"   NEM JÓ. Típus egyezik? {typeMatch}. Méreg egyezik? {poisonMatch}");
                    }
                }
                else
                {
                    // Most már itt is használhatjuk a currentID-t, mert fentebb deklaráltuk
                    // Debug.Log($"-> Tárgy kihagyva (Nem növény vagy hibás ID): {currentID}");
                }
            }
        }
        
        Debug.Log($"[InventoryManager] Találat összesen: {total}");
        return total;
    }

    // Kivesz X darabot a kért típusból (vegyesen is, pl. 2 áfonya + 1 málna)
    public void RemoveBotanyItemsByType(bool mustBePoisonous, int amountToRemove)
    {
        int remaining = amountToRemove;
        Debug.Log($"[InventoryManager] Törlés indítása. Cél: {amountToRemove} db {(mustBePoisonous ? "MÉRGEZŐ" : "EHETŐ")}.");

        // Végigmegyünk a slotokon
        for (int i = 0; i < MainInventory.Slots.Count; i++)
        {
            // Ha már mindent levontunk, azonnal kilépünk
            if (remaining <= 0) 
            {
                Debug.Log("[InventoryManager] Törlés kész (remaining <= 0). Kilépés.");
                break;
            }

            var slot = MainInventory.Slots[i];
            
            // Üres slotot átugorjuk
            if (slot.IsEmpty()) continue;

            string currentID = slot.Item.Base.Id;
            
            if (BotanyManager.Instance != null)
            {
                var plantData = BotanyManager.Instance.GetPlantByID(currentID);
                
                // Ha ez a tárgy megfelel a feltételeknek
                if (plantData != null && plantData.plantType == PlantType.Berry && plantData.isPoisonous == mustBePoisonous &&
                    plantData.sourceColor != PlantSourceColor.Potion)
                {
                    int inSlot = slot.Item.Quant;
                    Debug.Log($"   -> Találat a {i}. sloton ({plantData.itemName}). Van benne: {inSlot}. Még kell: {remaining}");

                    if (inSlot > remaining)
                    {
                        // A slotban több van, mint amennyi kell -> Csökkentjük
                        GDS.Core.Item newItem = slot.Item;
                        newItem.Quant -= remaining;
                        MainInventory.Slots[i] = MainInventory.Slots[i] with { Item = newItem };
                        
                        Debug.Log($"      Részleges levonás. Maradt a slotban: {newItem.Quant}");
                        remaining = 0;
                    }
                    else
                    {
                        // A slotban kevesebb vagy pont elég van -> Töröljük a slotot
                        remaining -= inSlot;
                        MainInventory.RemoveItem(slot);
                        
                        Debug.Log($"      Teljes slot törlése. Még kell: {remaining}");
                    }
                }
            }
        }

        // Ellenőrzés a végén
        if (remaining > 0)
        {
            Debug.LogError($"[InventoryManager] HIBA! A ciklus véget ért, de még {remaining} db-ot nem tudtam törölni! (Lehet, hogy a CanCraft rosszul számolt?)");
        }
        else
        {
            Debug.Log("[InventoryManager] SIKER! Minden szükséges anyag törölve.");
        }

        MainInventory.Data.Notify();
    }


}