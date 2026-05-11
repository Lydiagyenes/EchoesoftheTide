using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MusicState
{
    Default,    // Séta a világban (Playlist)
    Cave,       // Barlang (Loop)
    Cabin,
    Combat,     // Harc (Loop)
    Event       // Speciális esemény
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Beállítások")]
    public float crossFadeDuration = 2.0f; // Hány másodpercig tartson az átúszás
    public float maxVolume = 0.5f;

    [Header("Zene Listák")]
    public List<AudioClip> defaultPlaylist; // Séta zenék
    public AudioClip caveAmbient;           // Barlang zene
    public AudioClip cabinAmbient;          // Kabin zene
    public AudioClip combatMusic;           // Harc zene

    private AudioSource sourceA;
    private AudioSource sourceB;

    private bool isPlayingSourceA = true;   // Melyik szól éppen?

    private MusicState currentState = MusicState.Default;
    private Coroutine currentPlaylistRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        // Mivel a _GameSystems alatt lesz, nem kell DontDestroy, a szülő intézi
        
        // Létrehozzuk a két zenelejátszót
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        // Beállítások
        sourceA.loop = false;
        sourceB.loop = false;
        sourceA.volume = 0;
        sourceB.volume = 0;
    }

    private void Start()
    {
        // Indításkor elkezdjük a Default listát
        PlayDefaultMusic();
    }

    // --- PUBLIKUS HÍVÁSOK ---

    // Ezt hívja a zóna, ha bemész a barlangba
    public void SetState(MusicState newState)
    {
        if (currentState == newState) return; // Ha már az szól, ne csináljon semmit
        currentState = newState;

        StopPlaylist(); // Leállítjuk a véletlenszerű léptetést

        switch (newState)
        {
            case MusicState.Default:
                PlayDefaultMusic();
                break;
            case MusicState.Cave:
                CrossFadeTo(caveAmbient, true); // True = Loopoljon
                break;
            case MusicState.Combat:
                CrossFadeTo(combatMusic, true);
                break;
            case MusicState.Cabin:
                CrossFadeTo(cabinAmbient, true);
                break;
        }
    }

    // Ezt hívhatod Questeknél vagy Eseményeknél
    public void PlayEventMusic(AudioClip clip)
    {
        currentState = MusicState.Event;
        StopPlaylist();
        CrossFadeTo(clip, false); // Egyszer játssza le
    }

    // --- BELSŐ LOGIKA ---

    private void PlayDefaultMusic()
    {
        currentState = MusicState.Default;
        if (currentPlaylistRoutine == null)
        {
            currentPlaylistRoutine = StartCoroutine(PlaylistRoutine());
        }
    }
     public void StopMusic()
    {
        StopPlaylist(); // Leállítja a coroutine-t
        
        // Azonnal leállítjuk a hangforrásokat
        if (sourceA != null) sourceA.Stop();
        if (sourceB != null) sourceB.Stop();

        // FONTOS: Átállítjuk az állapotot 'Event'-re (vagy bármi másra),
        // hogy ha ezután valaki Defaultot kér, a rendszer érzékelje a váltást!
        currentState = MusicState.Event; 
    }

    private void StopPlaylist()
    {
        if (currentPlaylistRoutine != null)
        {
            StopCoroutine(currentPlaylistRoutine);
            currentPlaylistRoutine = null;
        }
    }

    // Ez felelős azért, hogy egymás után jöjjenek a számok
    private IEnumerator PlaylistRoutine()
    {
        while (currentState == MusicState.Default)
        {
            if (defaultPlaylist.Count > 0)
            {
                // Választunk egy véletlen számot
                AudioClip nextSong = defaultPlaylist[Random.Range(0, defaultPlaylist.Count)];
                
                // Átúszunk rá
                CrossFadeTo(nextSong, false);

                // Megvárjuk, amíg vége a dalnak (mínusz az átúszás ideje, hogy szép legyen)
                yield return new WaitForSeconds(nextSong.length - crossFadeDuration);
            }
            else
            {
                yield return null; // Ha üres a lista, ne fagyjon le
            }
        }
    }

    // A MÁGIA: Az átúszás (Crossfade)
    private void CrossFadeTo(AudioClip newClip, bool loop)
    {
        StartCoroutine(CrossFadeRoutine(newClip, loop));
    }

    private IEnumerator CrossFadeRoutine(AudioClip newClip, bool loop)
    {
        AudioSource activeSource = isPlayingSourceA ? sourceA : sourceB;
        AudioSource newSource = isPlayingSourceA ? sourceB : sourceA;

        // Az új forrás beállítása
        newSource.clip = newClip;
        newSource.loop = loop;
        newSource.Play();

        float timer = 0f;
        while (timer < crossFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossFadeDuration;

            // Az aktív forrást halkítjuk, az újat hangosítjuk
            activeSource.volume = Mathf.Lerp(maxVolume, 0f, t);
            newSource.volume = Mathf.Lerp(0f, maxVolume, t);

            yield return null;
        }

        // Biztos ami biztos: végleges értékek
        activeSource.volume = 0f;
        activeSource.Stop();
        newSource.volume = maxVolume;

        // Cseréljük a jelzőt
        isPlayingSourceA = !isPlayingSourceA;
    }
}