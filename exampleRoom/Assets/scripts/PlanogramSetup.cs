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
        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>(); // Add BoxCollider if none exists
            Debug.Log("Collider added.");
        }
    }

    private void CalculateDimensions(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            Debug.Log($"Planogram dimensions: Width: {bounds.size.x}, Height: {bounds.size.y}, Depth: {bounds.size.z}");
            Debug.Log($"Radius: {bounds.extents.magnitude}, Center: ({bounds.center.x}, {bounds.center.y}, {bounds.center.z})");
        }
        else
        {
            Debug.LogError("No collider found to calculate dimensions.");
        }
    }
}
