using UnityEngine;

public class CameraTargetSetter : MonoBehaviour
{
    private Transform currentPlayer; // Eltároljuk a játékost
    private Transform headTarget;
    private Transform bodyTarget;
    
    public void SetTarget(Transform rootTarget)
    {
        currentPlayer = rootTarget;
        
        // Megkeressük a célpontokat
        headTarget = rootTarget.Find("CameraTarget");
        bodyTarget = rootTarget.Find("BodyTarget"); // Ezt most hoztuk létre

        // Alapból a fejet követjük (vagy a gyökeret, ha nincs fej)
        UpdateCameraTarget(headTarget != null ? headTarget : rootTarget);
    }

    // Ezt hívja a Zóna
    public void SetZoneMode(bool useBody)
    {
        if (currentPlayer == null) return;

        if (useBody && bodyTarget != null)
        {
            // Ha zónában vagyunk -> Testet követjük
            UpdateCameraTarget(bodyTarget);
        }
        else
        {
            // Ha kimentünk -> Vissza a fejre
            UpdateCameraTarget(headTarget != null ? headTarget : currentPlayer);
        }
    }

    private void UpdateCameraTarget(Transform target)
    {
        var vcamComponent = GetComponent("CinemachineVirtualCameraBase"); // Vagy CinemachineFreeLook

        if (vcamComponent != null)
        {
            vcamComponent.GetType().GetProperty("Follow").SetValue(vcamComponent, target);
            vcamComponent.GetType().GetProperty("LookAt").SetValue(vcamComponent, target);
            Debug.Log("Kamera célpont átállítva: " + target.name);
        }
    }
}