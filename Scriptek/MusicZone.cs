using UnityEngine;

public class MusicZone : MonoBehaviour
{
    [Tooltip("Milyen zenére váltson?")]
    public MusicState targetMusic = MusicState.Cave;

    [Header("Működési Mód")]
    [Tooltip("Ha PIPA: Csak átvált, amikor átsétálsz rajta (Ajtókeret mód). Ha ÜRES: Addig szól, amíg benne állsz (Szoba mód).")]
    public bool switchOnly = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null)
            {
                // Mindkét módban: ha belépsz, átváltjuk a zenét a célra
                AudioManager.Instance.SetState(targetMusic);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // CSAK AKKOR váltunk vissza alapra, ha NEM "Switch Only" módban vagyunk.
            // Switch Only módban a kilépés nem csinál semmit (így nem baj, ha átmész rajta).
            if (!switchOnly)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetState(MusicState.Default);
                }
            }
        }
    }
}