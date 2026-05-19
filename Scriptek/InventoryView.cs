using UnityEngine;
using UnityEngine.UIElements;
using GDS.Core;
using System.Collections.Generic;

public class InventoryView : MonoBehaviour
{
    private VisualElement rootElement;
    private VisualElement inventoryContainer;
    private VisualElement slotsContainer;
    private ListBag mainInventoryBag;

    [Header("Design Beállítások")]
    public int baseCapacity = 15; // Ennyi slot érhető el alapból
    public string backpackSkillID = "Erosebb_Hatizsak"; // Ez a skill nyitja meg a többit

   // Awake helyett OnEnable-t használunk a UI felépítéséhez
    void OnEnable()
    {
        Debug.Log($"InventoryView OnEnable elindult ezen: {gameObject.name}");
        
        var uiDocument = GetComponent<UIDocument>();
        
        // --- 1. BIZTONSÁGI ELLENŐRZÉS ---
        if (uiDocument == null || uiDocument.visualTreeAsset == null)
        {
            Debug.LogError($"BŰNÖS OBJEKTUM: '{gameObject.name}' (Szülő: '{transform.parent?.name}'). Itt hiányzik a Source Asset!");
            return; 
        }
        
        // Itt kérjük el a gyökeret (OnEnable-ben már léteznie kell)
        rootElement = uiDocument.rootVisualElement;

        // --- 2. GYÖKÉR ELLENŐRZÉS ---
        if (rootElement == null)
        {
            // Ha még itt is null, akkor valami nagyon nem stimmel a Unity-vel vagy a prefabbal
            Debug.LogError($"CRITICAL HIBA: '{gameObject.name}' - A Source Asset be van húzva, de a rootVisualElement még NULL!");
            return; 
        }

        // --- 3. UI ELEMEK KERESÉSE ---
        inventoryContainer = rootElement.Q<VisualElement>("InventoryContainer");
        slotsContainer = rootElement.Q<VisualElement>("SlotsContainer");
            
        // --- 4. STÍLUS BEÁLLÍTÁSA (Megtartottuk!) ---
        if (slotsContainer != null)
        {
            slotsContainer.style.flexDirection = FlexDirection.Row; 
            slotsContainer.style.flexWrap = Wrap.Wrap;           
            slotsContainer.style.justifyContent = Justify.Center; 
        }
        else
        {
             Debug.LogError($"InventoryView ({gameObject.name}): Nem találom a 'SlotsContainer'-t a UI-ban!");
        }
        SetVisible(false); 
    }

    // A logikát és a kezdőállapotot a Start-ba tesszük, 
    // hogy az InventoryManager biztosan készen álljon (Awake után)
    void Start()
    {
                   
        Debug.Log("InventoryView Start() elindult.");

        // Skill rendszerre való feliratkozás (ha később vesszük meg a skillt, frissüljön a táska)
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.OnSkillUnlocked += RefreshView;
        }

        if (InventoryManager.Instance != null && mainInventoryBag == null)
        {
            mainInventoryBag = InventoryManager.Instance.MainInventory;
            mainInventoryBag.Data.OnChange += (slots) => { RefreshView(); };
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        
    }

    private void OnDestroy()
    {
        // Fontos leiratkozni, hogy ne legyen hiba kilépéskor
        if (SkillManager.Instance != null) SkillManager.Instance.OnSkillUnlocked -= RefreshView;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

   public void SetVisible(bool visible)
    {
        // JAVÍTÁS: Az inventoryContainer helyett a teljes rootElement-et rejtjük el!
        if (rootElement != null)
        {
            rootElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (visible) RefreshView();
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "Manager_Scene" || scene.name == "Intro_Scene" || scene.name == "MainMenu_Scene" || scene.name == "Loading_Scene")
        {
            SetVisible(false);
        }
    }

    private void RefreshView()
    {
        if (slotsContainer == null || mainInventoryBag == null) return;
        
        slotsContainer.Clear();

        bool hasExtraSpace = false;
        if (SkillManager.Instance != null) 
        {
            hasExtraSpace = SkillManager.Instance.HasSkill(backpackSkillID);
        }

        // --- DEBUG: Ellenőrizzük a képeket ---
        // Csak egyszer futtatjuk le, ne a ciklusban szemeteljen
        Sprite testBg = Resources.Load<Sprite>("UI/Slot_Background");
        Sprite testLock = Resources.Load<Sprite>("UI/Icon_Lock");
        if (testBg == null) Debug.LogError("HIBA: Nem találom a 'Resources/UI/Slot_Background' képet! Ellenőrizd a nevét és hogy Sprite-e!");
        if (testLock == null) Debug.LogError("HIBA: Nem találom a 'Resources/UI/Icon_Lock' képet! Ellenőrizd a nevét és hogy Sprite-e!");
        // -------------------------------------

        for (int i = 0; i < mainInventoryBag.Slots.Count; i++)
        {
            var slot = mainInventoryBag.Slots[i];
            var slotView = new VisualElement();
            slotView.AddToClassList("inventory-slot");

            slotView.style.width = 120;  
            slotView.style.height = 120; 
            slotView.style.marginRight = 8;
            slotView.style.marginLeft = 8;
            slotView.style.marginBottom = 15;
            
          
            // Háttérkép betöltése
            if (testBg != null) slotView.style.backgroundImage = new StyleBackground(testBg);

            // --- TARTÓSSÁG CSÍK ---
                    if (slot.Item.MaxDurability > 0) // Csak ha van tartóssága (tehát szerszám)
                    {
                        // 1. Háttér csík (Fekete/Szürke)
                        var durBarBack = new VisualElement();
                        durBarBack.style.width = Length.Percent(90); // 90%-a a slotnak
                        durBarBack.style.height = 5;
                        durBarBack.style.position = Position.Absolute;
                        durBarBack.style.bottom = 5;
                        durBarBack.style.left = Length.Percent(5);
                        durBarBack.style.backgroundColor = Color.black;
                        
                        // 2. Töltöttség (Zöld -> Piros)
                        var durBarFill = new VisualElement();
                        float percent = slot.Item.CurrentDurability / slot.Item.MaxDurability;
                        
                        durBarFill.style.width = Length.Percent(percent * 100);
                        durBarFill.style.height = 100; // A szülő 100%-a (5px)
                        
                        // Színváltás: Zöld ha sok, Piros ha kevés
                        Color barColor = Color.Lerp(Color.red, Color.green, percent);
                        durBarFill.style.backgroundColor = barColor;

                        durBarBack.Add(durBarFill);
                        slotView.Add(durBarBack);
                    }


            // --- 2. ZÁROLÁS LOGIKA ---
            if (i >= baseCapacity && !hasExtraSpace)
            {
                slotView.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.8f));
                
                var lockIcon = new VisualElement();
                // A lakatot is kicsinyítjük a kisebb dobozhoz
                lockIcon.style.width = 60; 
                lockIcon.style.height = 60;
                lockIcon.style.alignSelf = Align.Center; 
                lockIcon.style.top = 30; // Igazítás középre

                if (testLock != null) lockIcon.style.backgroundImage = new StyleBackground(testLock);
                
                slotView.Add(lockIcon);
            }
            else
            {
                // --- 3. NORMÁL SLOT ---
                
                       if (slot.IsFull())
                {
                    // 1. TÁRGY IKON (ALUL)
                    var iconElement = new VisualElement();
                    iconElement.AddToClassList("slot-icon");
                    // Ikon méret igazítása
                    iconElement.style.width = 100; 
                    iconElement.style.height = 100;
                    iconElement.style.alignSelf = Align.Center;
                    iconElement.style.top = 10;

                    string path = slot.Item.Base.Icon;
                    Sprite loadedSprite = Resources.Load<Sprite>(path);

                    if (loadedSprite != null)
                    {
                        iconElement.style.backgroundImage = new StyleBackground(loadedSprite);
                    }
                    
                    slotView.Add(iconElement); // <--- HOZZÁADJUK AZ ALAP IKONT

                    // 2. TAPASZTALT BOTANIKUS JELZÉSEK (KÖZÉPEN, AZ IKON FÖLÖTT)
                    // (Csak most jön a jelzés, hogy az ikon fölé kerüljön!)
                    if (SkillManager.Instance != null && SkillManager.Instance.HasSkill("Tapasztalt_Botanikus"))
                    {
                        if (BotanyManager.Instance != null)
                        {
                            var plantData = BotanyManager.Instance.GetPlantByID(slot.Item.Base.Id);
                            
                            // Ha ez egy növény (van adatlapja)
                            if (plantData != null)
                            {
                                // Létrehozzuk a jelző ikont
                                var statusIcon = new VisualElement();
                                statusIcon.style.width = 40;
                                statusIcon.style.height = 40;
                                statusIcon.style.position = Position.Absolute;
                                statusIcon.style.top = 4;
                                statusIcon.style.right = 4;

                                Sprite statusSprite = null;

                                if (plantData.isPoisonous)
                                {
                                    // MÉRGEZŐ -> Halálfej
                                    statusSprite = Resources.Load<Sprite>("UI/Icon_Skull");
                                    // Pirosas árnyalat, hogy ijesztő legyen
                                    statusIcon.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 0.6f, 0.6f));
                                }
                                else if (plantData.plantType == PlantType.Berry) // Csak ha ehető ÉS bogyó (hogy a kövön ne legyen csillag)
                                {
                                    // EHETŐ -> Csillag
                                    // Ellenőrizd a fájlnevet! (Star vagy Icon_Star?)
                                    statusSprite = Resources.Load<Sprite>("UI/Icon_Star"); 
                                    // Zöldes árnyalat
                                    statusIcon.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.6f, 1f, 0.6f));
                                }

                                if (statusSprite != null)
                                {
                                    statusIcon.style.backgroundImage = new StyleBackground(statusSprite);
                                    slotView.Add(statusIcon); // <--- HOZZÁADJUK A JELZÉST
                                }
                            }
                        }
                    }

                    // 3. MENNYISÉG (LEGFFELÜL)
                    if (slot.Item.Quant > 1)
                    {
                        var quantityLabel = new Label(slot.Item.Quant.ToString());
                        quantityLabel.AddToClassList("quantity-label");
                        quantityLabel.style.position = Position.Absolute;
                        quantityLabel.style.bottom = 5;
                        quantityLabel.style.right = 10;
                        quantityLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                        quantityLabel.style.fontSize = 28; 
                        quantityLabel.style.color = Color.white;
                        slotView.Add(quantityLabel);
                    }
                    
                    // --- TARTÓSSÁG CSÍK (HA SZERSZÁM) ---
                    // (Ide jöhet a zöld csík kódja, amit korábban írtunk)

                    // 4. ESEMÉNYKEZELÉS
                    ListSlot currentSlot = slot; 
                    slotView.RegisterCallback<PointerDownEvent>(evt => 
                    {
                        if (evt.button == 0) // Bal klikk: Evés
                        {
                            InventoryManager.Instance.ConsumeItem(currentSlot);
                        }
                        
                        if (evt.button == 1) // Jobb klikk: Eldobás
                        {
                             int amountToDrop = 1;
                            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                            {
                                amountToDrop = currentSlot.Item.Quant;
                            }
                            // Debug.Log(...);
                            InventoryManager.Instance.DropItem(currentSlot, amountToDrop);
                        }
                    });
                }
                
            }
            slotsContainer.Add(slotView);
        }
    }
}