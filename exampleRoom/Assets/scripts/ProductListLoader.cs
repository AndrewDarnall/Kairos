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
    private List<string> availableProducts = new List<string>();
    private List<string> removedProducts = new List<string>();

    void Start()
    {
        LoadProducts();
    }

    void Update()
    {
        HandleDeletion();
    }

    public void LoadProducts()
    {
        TextAsset file = Resources.Load<TextAsset>("Products");
        if (file == null)
        {
            Debug.LogError("Products.txt not found in Resources.");
            return;
        }

        string[] productNames = file.text.Split('\n');

        foreach (string productName in productNames)
        {
            string trimmedName = productName.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedName) && !availableProducts.Contains(trimmedName))
            {
                availableProducts.Add(trimmedName);
            }
        }

        UpdateProductListUI();
    }

    private void UpdateProductListUI()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (string productName in availableProducts)
        {
            AddProductToUI(productName);
        }
    }

    private void AddProductToUI(string productName)
    {
        GameObject newEntry = Instantiate(productEntryPrefab, contentPanel);

        TextMeshProUGUI textComponent = newEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = productName;
        }

        Button button = newEntry.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SpawnProduct(productName));
        }

        newEntry.name = productName;
    }

    private void SpawnProduct(string productName)
    {
        Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1f;

        GameObject newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
        newProduct.name = productName;
        newProduct.transform.SetParent(productListManager.transform);

        availableProducts.Remove(productName);
        removedProducts.Add(productName);

        UpdateProductListUI();
        Debug.Log($"Spawned product '{productName}' and removed from the UI list.");
    }

    private void HandleDeletion()
    {
        if (Input.GetKey(KeyCode.D)) // Tiene premuto D
        {
            // Controlla il click del mouse
            if (Input.GetMouseButtonDown(0)) // Click sinistro
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedObject = hit.collider.gameObject;
                    RemoveProduct(clickedObject);
                }
            }
        }
    }

    private void RemoveProduct(GameObject product)
    {
        string productName = product.name;

        Destroy(product);

        if (removedProducts.Contains(productName))
        {
            removedProducts.Remove(productName);
            availableProducts.Add(productName);
        }

        UpdateProductListUI();
        Debug.Log($"Removed product '{productName}' and added it back to the UI list.");
    }

}
