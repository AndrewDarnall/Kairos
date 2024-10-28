using UnityEngine;

public class PlanogramManager : MonoBehaviour
{
    public GameObject prefab; // Assign prefab in the inspector

    void Start()
    {
        if (prefab != null)
        {
            // Create an instance of the prefab
            GameObject instance = Instantiate(prefab);

            // Print the scale of the prefab
            Vector3 scalaPrefab = instance.transform.localScale;
            Debug.Log("Scala del prefab: " + scalaPrefab);

            // Search for a Renderer in the prefab or its children
            Renderer renderer = instance.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                // Get bounding box dimensions
                Bounds bounds = renderer.bounds;
                float lunghezza = bounds.size.x;
                float altezza = bounds.size.y;
                float larghezza = bounds.size.z;

                // Print the dimensions of the "piano"
                Debug.Log("Lunghezza del piano: " + lunghezza);
                Debug.Log("Altezza del piano: " + altezza);
                Debug.Log("Larghezza del piano: " + larghezza);

                // Calculate scaled dimensions
                lunghezza /= (10);
                larghezza /= (10);

                // Create a new GameObject for the "piano" using a primitive plane
                CreatePianoGameObject(lunghezza, altezza, larghezza, instance.transform);
            }
            else
            {
                Debug.LogError("Nessun Renderer trovato sul prefab o nei suoi figli.");
            }
        }
        else
        {
            Debug.LogError("Nessun prefab assegnato nell'inspector.");
        }
    }

    void CreatePianoGameObject(float lunghezza, float altezza, float larghezza, Transform parentTransform)
    {
        // Create a primitive plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

        // Set the position of the plane
        Vector3 bottomPosition = new Vector3(0, 0, 200); // Initial position (can be adjusted later)
        plane.transform.position = bottomPosition;

        // Set the plane as a child of the prefab instance
        //plane.transform.SetParent(parentTransform, true);

        // Scale the plane to fit the dimensions
        plane.transform.localScale = new Vector3(lunghezza, 1, larghezza);

        // Configure the plane's renderer
        MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
        if (planeRenderer != null)
        {
            planeRenderer.material = new Material(Shader.Find("Standard")) { color = Color.white }; // Assign a visible material
            planeRenderer.enabled = true; // Make the plane visible
        }

        // Optionally, add a MeshCollider if needed
        MeshCollider collider = plane.AddComponent<MeshCollider>();
        collider.sharedMesh = plane.GetComponent<MeshFilter>().mesh; // Assign the plane's mesh to the collider
    }
}