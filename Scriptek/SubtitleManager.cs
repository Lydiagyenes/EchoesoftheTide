using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }
     private AudioClip lastPlayedClip;
    private float lastPlayTime;

    [Header("UI Elemek")]
    public GameObject subtitlePanel;     
    public TextMeshProUGUI subtitleText;[Header("Hang")]
    public AudioSource voiceSource;      

    [Header("Irányítás")][Tooltip("Ezzel a gombbal lehet átugrani a szinkront (pl. Space)")]
    public KeyCode skipKey = KeyCode.Space;

    private Coroutine currentRoutine;    
    private System.Action currentCallback; // Itt tároljuk a "Mi történjen utána?" kódot

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    private void Update()
    {
        // SKIPPELÉS LOGIKÁJA: Ha látszik a panel ÉS megnyomják a Space-t
        if (subtitlePanel != null && subtitlePanel.activeSelf)
        {
            if (Input.GetKeyDown(skipKey) || Input.GetKeyDown(KeyCode.Return) )
            {
                Debug.Log("<color=yellow>[SubtitleManager] Szinkron átugorva!</color>");
                SkipDialogue();
            }
        }
    }

    public void PlayDialogue(DialogueLine line, System.Action onComplete = null)
    {
        // 1. TÖKÉLETES VÉDELEM: Ha az elmúlt 0.1 másodpercben MÁR elindítottuk ezt a hangot,
        // akkor ez egy "klón" parancs a dupla collider miatt. Azonnal blokkoljuk!
        if (line.audio != null && line.audio == lastPlayedClip && (Time.time - lastPlayTime) < 0.1f)
        {
            Debug.Log("<color=grey>[SubtitleManager] Dupla trigger blokkolva!</color>");
            return; 
        }

        // Feljegyezzük, mit és mikor indítunk el
        if (line.audio != null)
        {
            lastPlayedClip = line.audio;
            lastPlayTime = Time.time;
        }

        // Ha egy másik szöveg megy, azt leállítjuk
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        
        // Eltároljuk a futtatandó eseményt (hogy skipnél is le tudjon futni)
        currentCallback = onComplete;
        
        currentRoutine = StartCoroutine(PlayRoutine(line));
    }

    private IEnumerator PlayRoutine(DialogueLine line)
    {
        // 1. UI Bekapcsolása
        if (subtitlePanel != null) subtitlePanel.SetActive(true);
        if (subtitleText != null) subtitleText.text = line.text;

        // 2. Hang lejátszása
        float waitTime = line.duration; 

        if (line.audio != null && voiceSource != null)
        {
            voiceSource.Stop(); 
            voiceSource.clip = line.audio;
            voiceSource.Play();
            waitTime = line.audio.length; 
        }

        // 3. Várakozás a hang végéig (+0.5 mp szünet)
        yield return new WaitForSeconds(waitTime + 0.5f);

        // 4. Befejezés (Ha végigment rendesen)
        FinishDialogue();
    }

    private void SkipDialogue()
    {
        // Leállítjuk a várakozást a Coroutine-ban
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        // Rögtön befejezzük
        FinishDialogue();
    }

    private void FinishDialogue()
    {
        // 1. Hang elnémítása
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }

        // 2. UI Eltüntetése
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";

        // 3. Visszajelzés (Callback) futtatása!
        // Ezt át kell másolnunk egy lokális változóba, majd nullázni a globálist, 
        // nehogy végtelen ciklusba vagy duplázódásba kergessük a rendszert.
        System.Action callbackToRun = currentCallback;
        currentCallback = null; 
        currentRoutine = null;

        // Futtatjuk a Quest-indító vagy dialógus-léptető kódot
        callbackToRun?.Invoke();
    }
}