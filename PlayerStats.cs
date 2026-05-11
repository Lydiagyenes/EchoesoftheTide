using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, ISaveable
{
    public static PlayerStats Instance { get; private set; }

    [Header("Alap Statisztikák")]
    public float baseMaxHealth = 100f;
    public float baseMaxStamina = 100f;
    private float staminaMultiplier = 1f;
    public float healthRegenRate = 0f; 
    public float staminaRegenRate = 5f;

    [Header("Regeneráció Beállítások")]
    public float regenDelay = 1.5f;
    private float currentRegenTimer = 0f;

    public float MaxHealth { get; private set; }
    public float MaxStamina { get; private set; }

    public float currentHealth;
    public float currentStamina;

    public event System.Action OnStatsChanged;

    [Header("Halál beállítások")]
    public GameObject gameOverPanel;
    private Animator playerAnimator; 
    private PlayerMovement playerMovement;

    private bool isDead = false;

     private bool isInCampfireZone = false;
    private float campfireMultiplier = 1f;
     private Coroutine poisonCoroutine;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Debug.Log($"[PlayerStats] Start lefutott. HP beállítása...");
        RecalculateStats();
        
        // Ha nincs mentés betöltés (pl. editorból indítva), akkor feltöltjük
        if (currentHealth <= 0) 
        {
            Debug.Log("[PlayerStats] Kezdő HP 0 volt, feltöltöm Max-ra.");
            currentHealth = MaxHealth;
            currentStamina = MaxStamina;
        }

        FindPlayerReferences();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Game Over képernyő eltüntetése
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 2. Játékállapot visszaállítása (hogy ne legyünk halottak az új pályán)
        isDead = false; 
        Time.timeScale = 1f; // Idő újraindítása

        // 3. Egér elrejtése (vissza a játékba)
        // Kivétel: Ha a Főmenübe léptünk vissza, ott kell az egér!
        if (scene.name != "MainMenu_Scene")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 4. Referenciák megkeresése az új karakterhez
        FindPlayerReferences();
        
        // 5. Biztonsági gyógyítás (hogy ne 0 HP-val éledjünk újra és haljunk meg rögtön)
        // Ha a mentésből töltünk, a LoadData úgyis felülírja ezt, de ez a védőháló.
        if (currentHealth <= 0)
        {
            currentHealth = MaxHealth;
            currentStamina = MaxStamina;
            OnStatsChanged?.Invoke();
        }
    }


    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        // BIZTONSÁGI ELLENŐRZÉS: Ha nincs játékos, ne csináljunk semmit
        if (player == null)
        {
            // Opcionális: Debug.LogWarning("[PlayerStats] Még nem találom a játékost. (Ez normális betöltéskor)");
            return; 
        }

        // Ha van játékos, mehet a lekérés
        playerAnimator = player.GetComponent<Animator>();
        playerMovement = player.GetComponent<PlayerMovement>();
        
        if (playerMovement != null) playerMovement.canMove = true;
        
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    private void Update()
    {
        if (currentRegenTimer > 0)
        {
            currentRegenTimer -= Time.deltaTime;
        }
        else if (currentStamina < MaxStamina)
        {
             float totalMultiplier = staminaMultiplier; // Ez a bogyó buff
           if (isInCampfireZone) totalMultiplier *= campfireMultiplier; // Ez a tűz buff

             currentStamina += (staminaRegenRate * totalMultiplier) * Time.deltaTime;
           
           if (currentStamina > MaxStamina) currentStamina = MaxStamina;
           OnStatsChanged?.Invoke();
        }

        if (healthRegenRate > 0 && currentHealth < MaxHealth && currentHealth > 0)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            if (currentHealth > MaxHealth) currentHealth = MaxHealth;
            OnStatsChanged?.Invoke();
        }
        
        // DEBUG: Ha a HP 0 alá megy, de még nem haltunk meg, szóljon
        if (currentHealth <= 0 && !isDead)
        {
            Debug.LogError("[PlayerStats] UPDATE: A HP 0, de a Die() még nem futott le! Hívom most.");
            Die();
        }
    }

    public bool ConsumeStamina(float amount)
    {
        if (currentStamina > 0)
        {
            currentStamina -= amount;
            if (currentStamina < 0) currentStamina = 0;
            currentRegenTimer = regenDelay; 
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }

    // --- HALÁL LOGIKA DEBUGGAL ---
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("A Játékos meghalt!");

        // 1. Mozgás letiltása
        if (playerMovement != null)
        {
            // Ez önmagában elég, mert a PlayerMovement script
            // nem dolgozza fel a gombnyomásokat, ha ez hamis.
            playerMovement.canMove = false; 
        }

        // 2. Animáció
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Die");
        }

        StartCoroutine(ShowGameOverSequence());
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"[PlayerStats] Sebződés: {amount}. HP előtte: {currentHealth}");
        currentHealth -= amount;
        Debug.Log($"[PlayerStats] HP utána: {currentHealth}");

        if (currentHealth <= 0) 
        { 
            currentHealth = 0; 
            Debug.Log("[PlayerStats] HP elfogyott, Die() hívása a TakeDamage-ből.");
            Die(); 
        }
        OnStatsChanged?.Invoke();
    }

     public void ApplyPoison(float damagePerTick, float duration, float tickRate = 1.0f)
    {
        // Ha már mérgezve vagyunk, újraindítjuk (vagy halmozhatnánk is, de most reseteljük)
        if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);
        
        poisonCoroutine = StartCoroutine(PoisonRoutine(damagePerTick, duration, tickRate));
        Debug.Log($"[PlayerStats] MÉRGEZÉS! {duration} másodpercig.");
    }
    public void CurePoison()
    {
        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
            poisonCoroutine = null;
            Debug.Log("<color=green>[PlayerStats] Az ellenméreg hatott! Mérgezés megszüntetve.</color>");
            
            // Opcionális: Itt játszhatsz le egy megkönnyebbülés hangot vagy effektet
        }
        else
        {
            Debug.Log("[PlayerStats] Nem voltál mérgezve, de az ellenméreg finom volt.");
        }
    }

    private IEnumerator PoisonRoutine(float damagePerTick, float duration, float tickRate)
    {
        float timer = 0f;
        while (timer < duration)
        {
            yield return new WaitForSeconds(tickRate);
            
            // Sebzés (használjuk a meglévő TakeDamage-et, hogy a HUD is frissüljön)
            TakeDamage(damagePerTick);
            Debug.Log($"[Mérgezés] -{damagePerTick} HP");

            // Opcionális: Zöld villanás a képernyőn vagy hang
            
            timer += tickRate;
        }
        
        Debug.Log("[PlayerStats] A mérgezés elmúlt.");
        poisonCoroutine = null;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > MaxHealth) currentHealth = MaxHealth;
        OnStatsChanged?.Invoke();
    }

    public void RecalculateStats()
    {
        MaxHealth = baseMaxHealth;
        MaxStamina = baseMaxStamina;
        healthRegenRate = 0f;

        if (SkillManager.Instance != null)
        {
            // Skill bónuszok...
        }

        if (currentHealth > MaxHealth) currentHealth = MaxHealth;
        if (currentStamina > MaxStamina) currentStamina = MaxStamina;
        
        Debug.Log($"[PlayerStats] Statok újrakalkulálva. MaxHP: {MaxHealth}, CurrHP: {currentHealth}");
        OnStatsChanged?.Invoke();
    }

    public void SaveData(ref GameData data)
    {
        data.currentHealth = this.currentHealth;
        data.currentStamina = this.currentStamina;
    }

    public void LoadData(GameData data)
    {
        Debug.Log($"[PlayerStats] Adatok betöltése... Mentett HP: {data.currentHealth}");
        RecalculateStats();
        this.currentHealth = data.currentHealth;
        this.currentStamina = data.currentStamina;
        
        // Ha valamiért 0 lenne (pl. hiba miatt), töltsük fel
        if (this.currentHealth <= 0.1f) 
        {
             Debug.LogWarning("[PlayerStats] Betöltött HP túl alacsony, biztonsági feltöltés!");
             this.currentHealth = 1;
             this.currentStamina = MaxStamina;
        }
        OnStatsChanged?.Invoke();
    }

    private IEnumerator ShowGameOverSequence()
    {
        yield return new WaitForSeconds(3f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; 
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
   

    public void ApplyStaminaBuff(float duration, float multiplier)
    {
        StopCoroutine("StaminaBuffRoutine"); // Ha volt előző, azt leállítjuk
        StartCoroutine(StaminaBuffRoutine(duration, multiplier));
    }

    private IEnumerator StaminaBuffRoutine(float duration, float multiplier)
    {
        staminaMultiplier = multiplier;
        Debug.Log($"[PlayerStats] Stamina Boost Aktív! ({multiplier}x sebesség)");
        
        yield return new WaitForSeconds(duration);
        
        staminaMultiplier = 1f; // Visszaállunk normálra
        Debug.Log("[PlayerStats] Stamina Boost Lejárt.");
    }

    public void SetCampfireRegen(bool active, float multiplier)
    {
        isInCampfireZone = active;
        campfireMultiplier = multiplier;
        // Debug.Log($"Campfire Status: {active}");
    }
}