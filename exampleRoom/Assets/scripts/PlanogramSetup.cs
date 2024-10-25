using UnityEngine;

/* 
   PlanogramManager:
   Instantiates a prefab, ensures it has a collider, and calculates its dimensions. 
*/

public class PlanogramManager : MonoBehaviour
{
    public GameObject prefab; // Assign prefab in the inspector

    void Start()
    {
        if (prefab != null)
        {
            GameObject instantiatedObject = Instantiate(prefab, transform.position, transform.rotation);
            EnsureCollider(instantiatedObject);
            Physics.SyncTransforms(); // Sync physics before calculating dimensions
            CalculateDimensions(instantiatedObject);
        }
        else
        {
            Debug.LogError("Prefab not assigned!");
        }
    }

    private void EnsureCollider(GameObject obj)
    {
        if (obj.GetComponent<MeshCollider>() == null)
        {
            obj.AddComponent<MeshCollider>(); // Add MeshCollider if none exists
            Debug.Log("Collider added.");
        }
    }

    private void CalculateDimensions(GameObject obj)
    {
        MeshCollider collider = obj.GetComponent<MeshCollider>();
        if (collider != null)
        {
            // Ottieni i bounds dal MeshCollider
            Bounds bounds = collider.bounds;
            Debug.Log($"Planogram dimensions: Width: {bounds.size.x}, Height: {bounds.size.y}, Depth: {bounds.size.z}");
            Debug.Log($"Radius: {bounds.extents.magnitude}, Center: ({bounds.center.x}, {bounds.center.y}, {bounds.center.z})");

            // Calcola la dimensione della base
            float baseWidth = bounds.size.x;
            float baseLength = bounds.size.z;

            // Stampa le dimensioni della base nella console
            Debug.Log($"Dimensione della base della mesh: Larghezza = {baseWidth}, Lunghezza = {baseLength}");
        }
        else
        {
            Debug.LogError("No collider found to calculate dimensions.");
        }
    }
}
