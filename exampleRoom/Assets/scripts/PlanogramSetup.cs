using UnityEngine;

public class PlanogramManager : MonoBehaviour
{
    public GameObject prefab; // Assign prefab in the inspector

    void Start()
    {
        if (prefab != null)
        {
            // Crea un'istanza del prefab
            GameObject instance = Instantiate(prefab);

            // Controlla se il prefab ha un MeshFilter e un MeshRenderer
            MeshFilter meshFilter = instance.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = instance.GetComponent<MeshRenderer>();

            // Se non ci sono, aggiungi i componenti
            if (meshFilter == null)
            {
                meshFilter = instance.AddComponent<MeshFilter>();

                // Crea una mesh semplice (un piano)
                Mesh mesh = new Mesh();
                Vector3[] vertices = new Vector3[]
                {
                    new Vector3(-0.5f, 0, -0.5f),
                    new Vector3(0.5f, 0, -0.5f),
                    new Vector3(-0.5f, 0, 0.5f),
                    new Vector3(0.5f, 0, 0.5f),
                };

                int[] triangles = new int[]
                {
                    0, 2, 1, // triangolo 1
                    1, 2, 3  // triangolo 2
                };

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals(); // Calcola le normali
                meshFilter.mesh = mesh; // Assegna la mesh al MeshFilter
            }

            if (meshRenderer == null)
            {
                meshRenderer = instance.AddComponent<MeshRenderer>();
                // Puoi anche assegnare un materiale qui se necessario
                meshRenderer.material = new Material(Shader.Find("Standard")); // Materiale standard
            }

            // Ottieni il Renderer del prefab
            Renderer renderer = instance.GetComponent<Renderer>();

            // Se il renderer è presente
            if (renderer != null)
            {
                // Ottieni il bounding box del Renderer
                Bounds bounds = renderer.bounds;

                // Calcola lunghezza e larghezza
                float lunghezza = bounds.size.x; // Dimensione lungo l'asse X
                float larghezza = bounds.size.z;  // Dimensione lungo l'asse Z

                // Stampa i risultati nella console
                Debug.Log("Lunghezza del prefab: " + lunghezza);
                Debug.Log("Larghezza del prefab: " + larghezza);
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
}
