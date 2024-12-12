using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductListLoader : MonoBehaviour
{
    public GameObject productEntryPrefab; // Prefab for the button
    public Transform contentPanel;       // Content of the Scroll View

    void Start()
    {
        if (productEntryPrefab == null)
            Debug.LogError("Product Entry Prefab not assigned!");

        if (contentPanel == null)
            Debug.LogError("Content Panel not assigned!");

        LoadProducts();
    }

    public void LoadProducts()
    {
        // Load the text file containing product names
        TextAsset file = Resources.Load<TextAsset>("Products");
        if (file == null)
        {
            Debug.LogError("Products.txt not found in Resources.");
            return;
        }

        // Split the file contents into individual product names
        string[] productNames = file.text.Split('\n');

        foreach (string productName in productNames)
        {
            if (!string.IsNullOrWhiteSpace(productName))
            {
                // Instantiate a new entry
                GameObject newEntry = Instantiate(productEntryPrefab, contentPanel);

                // Set the button text to the product name
                TextMeshProUGUI textComponent = newEntry.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = productName.Trim();
                }

                // Assign the product name to the ProductEntryHandler script
                ProductEntryHandler handler = newEntry.GetComponent<ProductEntryHandler>();
                if (handler != null)
                {
                    handler.productName = productName.Trim();
                }

                Debug.Log("Loaded product: " + productName.Trim());
            }
        }
    }
}
