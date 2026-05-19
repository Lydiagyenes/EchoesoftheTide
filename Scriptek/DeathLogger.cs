using UnityEngine;

public class DeathLogger : MonoBehaviour
{
    void OnDestroy()
    {
        // Ez kiírja a konzolra pirossal, ha az objektum megsemmisül!
        // A "StackTrace" megmondja majd, melyik script hívta a Destroy-t.
        Debug.LogError($"!!! A '{gameObject.name}' MEGSEMMISÜLT !!!");
    }
}