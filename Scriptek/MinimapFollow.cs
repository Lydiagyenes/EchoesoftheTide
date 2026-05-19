using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Húzd be a Playert
    public bool rotateWithPlayer = false; // Forogjon a térkép? (Vagy fix Észak?)
    public float height = 50f; // Milyen magasan legyen a kamera

    private void LateUpdate() // A mozgás után frissítünk
    {
        if (player == null) return;

        // 1. Követjük a játékos X és Z pozícióját, de a magasság (Y) fix
        Vector3 newPosition = player.position;
        newPosition.y = height;
        transform.position = newPosition;

        // 2. Forgatás (Opcionális)
        if (rotateWithPlayer)
        {
            // Együtt forog a játékossal
            Vector3 newRotation = transform.eulerAngles;
            newRotation.y = player.eulerAngles.y;
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        else
        {
            // Mindig észak felé néz (ajánlott)
             transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}