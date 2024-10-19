using UnityEngine;

public class PlanogramManager : MonoBehaviour
{
    public GameObject prefab; // Assign your prefab in the inspector

    void Start()
    {
        if (prefab != null)
        {
            // Instantiate the prefab at the position of this GameObject
            GameObject instantiatedObject = Instantiate(prefab, transform.position, transform.rotation);

            // Calculate the dimensions
            CalculateDimensions(instantiatedObject);
        }
        else
        {
            Debug.LogError("Prefab is not assigned in the inspector!");
        }
    }

    private void CalculateDimensions(GameObject obj)
    {
        // Get the collider or mesh filter to calculate the bounds
        Collider collider = obj.GetComponent<Collider>();
        if (collider == null)
        {
            // If there's no collider, try to get the mesh filter
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                collider = meshFilter.GetComponent<Collider>();
            }
        }

        if (collider != null)
        {
            // Calculate the bounds of the collider
            Bounds bounds = collider.bounds;

            // Get dimensions
            float width = bounds.size.x;
            float height = bounds.size.y;
            float depth = bounds.size.z;

            // Print dimensions in the console
            Debug.Log($"Width: {width}, Height: {height}, Depth: {depth}");
        }
        else
        {
            Debug.LogError("No collider or mesh found to calculate dimensions.");
        }
    }
}
