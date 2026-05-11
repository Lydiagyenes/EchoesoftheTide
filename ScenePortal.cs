using UnityEngine;

public class ScenePortal : MonoBehaviour
{[Header("Portal Settings (Hová vezessen?)")]
    public string sceneToLoad;[Header("Érkezési Koordináták (Transform helyett!)")][Tooltip("Ide írd be a pontos X, Y, Z értékeket, ahová a játékos érkezni fog a KÖVETKEZŐ pályán!")]
    public Vector3 targetPosition;[Tooltip("Írd be, merre nézzen a karakter (Y forgás, pl. 0, 90, 180).")]
    public float targetRotationY;

    [Header("Vizuális")]
    public Sprite loadingScreenImage;

    // Egyetlen, megmásíthatatlan kapcsoló. Ha egyszer elsült, kész!
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Ha a játékos lép be, és a portál még nem sült el...
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; // AZONNAL lezárjuk, így soha többé nem akad be!

            // 1. Lefagyasztjuk a játékost, hogy ne futhasson tovább és ne pattanjon vissza!
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null) pm.canMove = false;

            // Biztonsági ellenőrzés
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("[ScenePortal] HIBA: Nincs megadva a betöltendő Scene neve!");
                return;
            }

            Debug.Log($"[ScenePortal] Portál aktiválva! Irány: {sceneToLoad}, Érkezés: {targetPosition}");

            // 2. Indítjuk a váltást a PONTOS kézzel beírt koordinátákkal (Transform helyett)
            SceneTransitionManager.Instance.LoadScene(
                sceneToLoad, 
                targetPosition, 
                Quaternion.Euler(0f, targetRotationY, 0f), 
                this.loadingScreenImage
            );
        }
    }
    
    // AZ OnTriggerExit FÜGGVÉNYT TELJESEN KITÖRÖLTÜK! 
    // Ha egyszer belementünk az ajtóba, nincs visszaút!
}