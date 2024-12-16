using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductListLoader : MonoBehaviour
{
    public GameObject productEntryPrefab; // Prefab for the button in the menu
    public Transform contentPanel;        // Content of the Scroll View
    public GameObject productPrefab;      // Prefab to instantiate in the scene
    public GameObject productListManager; // Parent GameObject (ProductListManager)

    void Start()
    {
        LoadProducts();
    }

    public void LoadProducts()
    {
        // Carica il file dei prodotti
        TextAsset file = Resources.Load<TextAsset>("Products");
        if (file == null)
        {
            Debug.LogError("Products.txt not found in Resources.");
            return;
        }

        // Suddivide i prodotti e li processa
        string[] productNames = file.text.Split('\n');

        foreach (string productName in productNames)
        {
            if (!string.IsNullOrWhiteSpace(productName))
            {
                string trimmedName = productName.Trim();

                // Controlla se esiste già un'entry nella lista
                Transform existingEntry = contentPanel.Find(trimmedName);
                if (existingEntry != null)
                {
                    Debug.LogWarning($"Menu entry for '{trimmedName}' already exists!");
                    continue;
                }

                // Istanzia un nuovo menu entry
                GameObject newEntry = Instantiate(productEntryPrefab, contentPanel);

                // Configura il testo del pulsante
                TextMeshProUGUI textComponent = newEntry.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = trimmedName;
                }

                // Configura il pulsante
                Button button = newEntry.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners(); // Rimuove listener esistenti
                    button.onClick.AddListener(() => SpawnProduct(trimmedName));
                }

                // Rinomina il nuovo oggetto per gestione più semplice
                newEntry.name = trimmedName;

                Debug.Log($"Loaded menu entry: {trimmedName}");
            }
        }
    }

    private void SpawnProduct(string productName)
    {
        // Trimma il nome per sicurezza
        productName = productName.Trim();

        // Calcola la posizione di spawn
        Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1f;

        // Istanzia il prodotto
        GameObject newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity);

        // Imposta il parent come ProductListManager
        newProduct.transform.SetParent(productListManager.transform);

        // Rinomina l'oggetto instanziato
        newProduct.name = productName;

        // Trova e rimuove l'entry del prodotto dalla lista UI
        foreach (Transform child in contentPanel)
        {
            TextMeshProUGUI textComponent = child.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null && textComponent.text == productName)
            {
                Destroy(child.gameObject); // Elimina il pulsante dalla lista
                break;
            }
        }

        Debug.Log($"Spawned product '{productName}' and removed from the UI list.");
    }
}
