using UnityEngine;
using UnityEngine.UI;

public class ProductEntryHandler : MonoBehaviour
{
    public string productName; // Name of the product associated with this entry
    private const string prefabPath = "ProductPlaceholder"; // Path to the product prefab

    public void OnClick()
    {
        // Load the prefab from the specified path
        GameObject productPrefab = Resources.Load<GameObject>(prefabPath);
        if (productPrefab == null)
        {
            Debug.LogError("Prefab not found at path: " + prefabPath);
            return;
        }

        // Find the Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found in the scene.");
            return;
        }

        // Calculate the spawn position (1 meter in front of the Main Camera)
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.0f;

        // Instantiate the prefab
        GameObject newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
        newProduct.name = productName; // Optionally set the name of the instantiated object

        Debug.Log("Instantiated prefab: " + productName + " at position " + spawnPosition);
    }
}

