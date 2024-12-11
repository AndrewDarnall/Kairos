using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductListLoader : MonoBehaviour
{
    public GameObject productEntryPrefab; // Prefab del pulsante
    public Transform contentPanel;       // Content del Scroll View

    void Start()
    {
        if (productEntryPrefab == null)
            Debug.LogError("Product Entry Prefab non assegnato!");

        if (contentPanel == null)
            Debug.LogError("Content Panel non assegnato!");

        LoadProducts();
    }


    void LoadProducts()
    {
        // Carica il file Products.txt
        TextAsset file = Resources.Load<TextAsset>("Products");
        if (file == null)
        {
            Debug.LogError("Products.txt non trovato in Resources.");
            return;
        }

        string[] productNames = file.text.Split('\n');

        foreach (string productName in productNames)
        {
            if (!string.IsNullOrWhiteSpace(productName))
            {
                string trimmedName = productName.Trim();
                Debug.Log("Caricato prodotto: " + trimmedName);

                GameObject newEntry = Instantiate(productEntryPrefab, contentPanel);

                TextMeshProUGUI textComponent = newEntry.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                    textComponent.text = trimmedName;
                else
                    Debug.LogError("Componente Text non trovato nel prefab Product Entry!");
            }
        }
    }


}
