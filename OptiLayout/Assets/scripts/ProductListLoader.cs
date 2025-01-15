using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.EventSystems;

public class ProductListLoader : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject productEntryPrefab;
    public Transform contentPanel;
    public TMP_InputField searchInputField;
    public Button activeProductFilterButton;

    [Header("Prefabs")]
    public GameObject productPrefab;
    public GameObject pointerPrefab;

    [Header("References")]
    public GameObject productListManager;

    private List<string> availableProducts = new();
    private Dictionary<string, GameObject> spawnedProducts = new();
    private GameObject pointerInstance;

    private const string ResourcesFolder = "Assets/Resources/";
    private const string SaveFolder = "Assets/SavedPrefabs/";
    private const string ProductsFileName = "Products.txt";
    private const string PlacedProductsFileName = "PlacedProducts.txt";

    private bool showOnlySpawned = false;

    private void Start()
    {
        InitializePointer();
        InitializeUI();
        LoadProducts();
        LoadPlacedProducts();
        UpdateProductListUI();
    }

    private void Update()
    {
        if (!IsInputFieldFocused())
        {
            UpdatePointerPosition();
            HandleProductDeletion();

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.S))
                    SaveAll();
            }
        }
    }

    private void InitializePointer()
    {
        if (pointerPrefab)
        {
            pointerInstance = Instantiate(pointerPrefab);
            pointerInstance.SetActive(false);
        }
    }

    private void InitializeUI()
    {
        searchInputField.onValueChanged.AddListener(UpdateProductListUI);
        contentPanel = GameObject.Find("Canvas/Scroll View/Viewport/Content").transform;

        if (activeProductFilterButton != null)
        {
            activeProductFilterButton.onClick.AddListener(ToggleFilter);
        }
    }

    private void ToggleFilter()
    {
        showOnlySpawned = !showOnlySpawned;
        UpdateProductListUI(searchInputField.text);
    }

    private void LoadProducts()
    {
        var filePath = Path.Combine(ResourcesFolder, ProductsFileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {ProductsFileName} not found in {ResourcesFolder}.");
            return;
        }

        availableProducts = File.ReadAllLines(filePath)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        UpdateProductListUI();
    }

    private void UpdateProductListUI(string filter = "")
    {
        foreach (Transform child in contentPanel)
        {
            if (child != null && child.gameObject != null)
                Destroy(child.gameObject);
        }

        var filteredProducts = availableProducts
            .Where(product => product.ToLower().Contains(filter.ToLower()))
            .ToList();

        if (showOnlySpawned)
        {
            filteredProducts = filteredProducts
                .Where(product => spawnedProducts.ContainsKey(product))
                .ToList();
        }

        filteredProducts.ForEach(AddProductToUI);
    }

    private void AddProductToUI(string productName)
    {
        var newEntry = Instantiate(productEntryPrefab, contentPanel);
        var textComponent = newEntry.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
            textComponent.text = productName;

        var button = newEntry.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => HandleProductSelection(productName, newEntry));

        newEntry.name = productName;

        var entryImage = newEntry.GetComponent<Image>();
        if (entryImage != null)
        {
            entryImage.color = spawnedProducts.ContainsKey(productName) ? Color.green : Color.yellow;
        }
    }

    private void HandleProductSelection(string productName, GameObject entry)
    {
        if (spawnedProducts.ContainsKey(productName))
        {
            FocusOnProduct(spawnedProducts[productName]);
        }
        else
        {
            SpawnProduct(productName, entry);
        }
    }

    private void SpawnProduct(string productName, GameObject entry)
    {
        if (pointerInstance == null || !pointerInstance)
        {
            Debug.LogWarning("Pointer instance is missing or destroyed.");
            return;
        }

        if (spawnedProducts.ContainsKey(productName) && spawnedProducts[productName] != null)
        {
            Debug.LogWarning($"Product {productName} is already spawned.");
            return;
        }

        // Spawn position for the product
        var spawnPosition = pointerInstance.transform.position;

        // Spawn the product
        var newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity, productListManager.transform);
        newProduct.name = productName;

        // Calculate the size of the product
        var productBounds = newProduct.GetComponent<Renderer>()?.bounds.size ?? Vector3.one;

        // Add a text label slightly above the product
        GameObject textObject = new GameObject("ProductNameText");
        textObject.transform.SetParent(newProduct.transform); // Attach the text to the product

        // Position the label slightly above the top edge
        float textOffset = productBounds.y + 2f; // Offset slightly above the top edge
        textObject.transform.localPosition = Vector3.up * textOffset;

        // Add the TextMeshPro component
        var textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = productName; // Set the product name as text
        textMesh.fontSize = 5f;      // Set the text size
        textMesh.alignment = TextAlignmentOptions.Center; // Center the text
        textMesh.color = Color.black; // Set the text color

        // Keep the text oriented towards the camera
        if (Camera.main != null)
        {
            textObject.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        }

        // Save the product in the list of spawned objects
        spawnedProducts[productName] = newProduct;

        // Change the color of the UI element
        var entryImage = entry.GetComponent<Image>();
        if (entryImage != null)
            entryImage.color = Color.green;
    }


private void FocusOnProduct(GameObject product)
{
    if (product != null && Camera.main != null)
    {
        Vector3 offset = Camera.main.transform.forward * 10f;
        Camera.main.transform.position = product.transform.position - offset;

        // Puntare la camera verso l'oggetto
        Camera.main.transform.LookAt(product.transform);

        Debug.Log($"Camera focused on {product.name}.");
    }
    else
    {
        Debug.LogWarning("Product or camera is null, focus operation aborted.");
    }
}


    private void UpdatePointerPosition()
    {
        if (pointerInstance)
        {
            pointerInstance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 40f;
            pointerInstance.SetActive(true);
        }
    }

    private void HandleProductDeletion()
    {
        if (Input.GetKey(KeyCode.D) && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                if (clickedObject != null && clickedObject.CompareTag("Product"))
                {
                    RemoveProduct(clickedObject);
                }
                else
                {
                    Debug.Log("Object is not a product and cannot be deleted.");
                }
            }
        }
    }

    private void RemoveProduct(GameObject product)
    {
        if (product == null || !product)
        {
            Debug.LogWarning("The product you are trying to remove no longer exists.");
            return;
        }

        string productName = product.name;

        if (!string.IsNullOrEmpty(productName) && !availableProducts.Contains(productName))
        {
            availableProducts.Add(productName);
            Debug.Log($"Product '{productName}' added back to the available list.");
        }

        if (spawnedProducts.ContainsKey(productName))
            spawnedProducts.Remove(productName);

        Destroy(product);

        var entry = contentPanel.Find(productName)?.GetComponent<Image>();
        if (entry != null)
            entry.color = Color.yellow;

        UpdateProductListUI();
        Debug.Log($"Product '{productName}' has been removed.");
    }

    private void SaveAll()
    {
        SaveProductListManager();
        SaveProductsToFile();
        SavePlacedProducts();
    }

    private void SaveProductListManager()
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);

        var prefabPath = Path.Combine(SaveFolder, $"{productListManager.name}.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(productListManager, prefabPath);
        Debug.Log(prefab ? $"Prefab saved at {prefabPath}" : "Failed to save prefab!");
    }

    private void SaveProductsToFile()
    {
        var filePath = Path.Combine(ResourcesFolder, ProductsFileName);
        File.WriteAllLines(filePath, availableProducts);
        Debug.Log($"Products saved to {filePath}");
    }

private void SavePlacedProducts()
{
    var placedFilePath = Path.Combine(ResourcesFolder, PlacedProductsFileName);

    var placedProducts = spawnedProducts
        .Where(kvp => kvp.Value != null) // Assicurati che l'oggetto non sia nullo
        .Select(kvp =>
        {
            var position = kvp.Value.transform.position;
            return $"{kvp.Key}|{position.x},{position.y},{position.z}";
        })
        .ToList();

    File.WriteAllLines(placedFilePath, placedProducts);
    Debug.Log($"Placed products with positions saved to {placedFilePath}");
}

private void LoadPlacedProducts()
{
    var placedFilePath = Path.Combine(ResourcesFolder, PlacedProductsFileName);

    if (File.Exists(placedFilePath))
    {
        var placedProducts = File.ReadAllLines(placedFilePath)
            .Select(line =>
            {
                var parts = line.Split('|');
                if (parts.Length != 2) return default; // Usa il valore predefinito

                var productName = parts[0].Trim();
                var positionParts = parts[1].Split(',');
                if (positionParts.Length != 3) return default; // Usa il valore predefinito

                if (float.TryParse(positionParts[0], out var x) &&
                    float.TryParse(positionParts[1], out var y) &&
                    float.TryParse(positionParts[2], out var z))
                {
                    return (productName, new Vector3(x, y, z));
                }

                return default; // Usa il valore predefinito
            })
            .Where(entry => !string.IsNullOrEmpty(entry.productName)) // Filtra i valori non validi
            .ToList();

        foreach (var (productName, position) in placedProducts)
        {
            if (!spawnedProducts.ContainsKey(productName) && availableProducts.Contains(productName))
            {
                // Instanzia l'oggetto nella posizione salvata
                var newProduct = Instantiate(productPrefab, position, Quaternion.identity, productListManager.transform);
                newProduct.name = productName;

                // Associa il prodotto al dizionario
                spawnedProducts[productName] = newProduct;

                // Configura un'etichetta visibile sopra l'oggetto
                SetupProductLabel(newProduct, productName);

                Debug.Log($"Product '{productName}' instantiated at {position}");
            }
        }

        Debug.Log("Placed products with positions loaded and instantiated successfully.");
    }
    else
    {
        Debug.LogWarning($"File {PlacedProductsFileName} not found in {ResourcesFolder}.");
    }
}


// Metodo di supporto per creare l'etichetta sopra l'oggetto
private void SetupProductLabel(GameObject product, string productName)
{
    var productBounds = product.GetComponent<Renderer>()?.bounds.size ?? Vector3.one;

    // Aggiunge l'etichetta sopra il prodotto
    GameObject textObject = new GameObject("ProductNameText");
    textObject.transform.SetParent(product.transform);

    float textOffset = productBounds.y + 2f;
    textObject.transform.localPosition = Vector3.up * textOffset;

    var textMesh = textObject.AddComponent<TextMeshPro>();
    textMesh.text = productName;
    textMesh.fontSize = 5f;
    textMesh.alignment = TextAlignmentOptions.Center;
    textMesh.color = Color.black;

    if (Camera.main != null)
    {
        textObject.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
    }
}


    private bool IsInputFieldFocused()
    {
        var selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null || !selectedObject)
            return false;

        var inputField = selectedObject.GetComponent<TMP_InputField>();
        return inputField != null;
    }
}