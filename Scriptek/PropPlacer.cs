using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PropPlacer : MonoBehaviour
{
    [Header("Mit rakjon le?")]
    public GameObject prefab;

    [Header("Mennyit?")]
    public int count = 100;

    [Header("Terület (használd a BoxCollider-t)")]
    public LayerMask groundLayer;

    [Header("Méret randomizálás")]
    public float minScale = 0.8f;
    public float maxScale = 1.4f;

    [Header("Vízszint szűrő")]
    public bool useWaterFilter = true;
    public float waterLevel = 3.5f;

    [Header("Elhelyezett objektumok (ne piszkáld kézzel)")]
    public List<GameObject> placedObjects = new List<GameObject>();

    private BoxCollider zone;

#if UNITY_EDITOR
    public void PlaceProps()
    {
         ClearProps();

    zone = GetComponent<BoxCollider>();
    if (zone == null)
    {
        zone = gameObject.AddComponent<BoxCollider>();
        zone.isTrigger = true;
        Debug.Log("[PropPlacer] BoxCollider automatikusan hozzáadva. Állítsd be a méretét, majd nyomd meg újra a LERAK gombot!");
        return; // Megállunk, hogy be tudd állítani a méretet
    }
        if (prefab == null) { Debug.LogError("[PropPlacer] Nincs prefab!"); return; }

        Bounds bounds = zone.bounds;
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 20;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;
            float rx = Random.Range(bounds.min.x, bounds.max.x);
            float rz = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 rayStart = new Vector3(rx, bounds.max.y + 50f, rz);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 500f, groundLayer))
            {
                if (useWaterFilter && hit.point.y < waterLevel) continue;

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.transform.position = hit.point;
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                obj.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
                obj.transform.parent = this.transform;

                Undo.RegisterCreatedObjectUndo(obj, "PropPlacer: Place");
                placedObjects.Add(obj);
                placed++;
            }
        }

        Debug.Log($"[PropPlacer] Lerakva: {placed} / {count} ({attempts} próbálkozásból).");
    }

    public void ClearProps()
    {
        foreach (var obj in placedObjects)
        {
            if (obj != null) Undo.DestroyObjectImmediate(obj);
        }
        placedObjects.Clear();
    }

    public void ValidatePickupSaveData()
    {
        int missing = 0;
        foreach (var obj in placedObjects)
        {
            if (obj != null && obj.GetComponentInChildren<PickupSaveData>() == null)
            {
                Debug.LogWarning($"[PropPlacer] HIÁNYZÓ PickupSaveData: {obj.name} @ {obj.transform.position}");
                missing++;
            }
        }

        if (missing == 0)
            Debug.Log("<color=green>[PropPlacer] Minden objektumon van PickupSaveData. ✓</color>");
        else
            Debug.LogError($"[PropPlacer] {missing} objektumon HIÁNYZIK a PickupSaveData!");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PropPlacer))]
public class PropPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PropPlacer placer = (PropPlacer)target;

        EditorGUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("▶  LERAK", GUILayout.Height(35)))
            placer.PlaceProps();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("✕  TÖRÖL MINDENT", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Törlés", "Biztosan törlöd az összes lerakott objektumot?", "Igen", "Mégsem"))
                placer.ClearProps();
        }

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🔍  VALIDÁCIÓ (PickupSaveData)", GUILayout.Height(28)))
            placer.ValidatePickupSaveData();

        GUI.backgroundColor = Color.white;
    }
}
#endif