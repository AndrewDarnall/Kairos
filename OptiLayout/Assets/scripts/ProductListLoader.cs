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

                if (Input.GetKeyDown(KeyCode.Q))
                    QuitApplication(); // Aggiungi questo metodo
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
    if (pointerInstance == null)
    {
        Debug.LogWarning("Pointer instance is missing or destroyed.");
        return;
    }

    // Controlla se il prodotto esiste già come figlio di ProductListManager
    if (spawnedProducts.ContainsKey(productName))
    {
        Debug.LogWarning($"Product {productName} is already spawned.");
        return;
    }

    var globalSpawnPosition = pointerInstance.transform.position;
    var parentTransform = productListManager.transform;
    var localSpawnPosition = parentTransform.InverseTransformPoint(globalSpawnPosition);

    // Crea il nuovo prodotto come figlio del ProductListManager
    var newProduct = Instantiate(productPrefab, parentTransform);
    newProduct.transform.localPosition = localSpawnPosition;
    newProduct.name = productName;

    // Configura il prodotto (aggiunge l'etichetta sopra di esso)
    SetupProductLabel(newProduct, productName);

    // Aggiungi il prodotto alla lista degli oggetti generati
    spawnedProducts[productName] = newProduct;

    // Aggiorna il colore dell'entry nell'UI
    var entryImage = entry.GetComponent<Image>();
    if (entryImage != null)
        entryImage.color = Color.green;

    Debug.Log($"Product '{productName}' spawned at local position {localSpawnPosition}.");
}

    private void FocusOnProduct(GameObject product)
    {
        if (product != null && Camera.main != null)
        {
            Vector3 offset = Camera.main.transform.forward * 10f;
            Camera.main.transform.position = product.transform.position - offset;

            // Point the camera towards the object
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

    private void QuitApplication()
    {
        Debug.Log("Exiting application...");

    #if UNITY_EDITOR
        // Per uscire dall'editor durante il Play Mode
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        // Per chiudere l'applicazione compilata
        Application.Quit();
    #endif
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
    if (product == null)
    {
        Debug.LogWarning("The product you are trying to remove does not exist.");
        return;
    }

    string productName = product.name;

    if (spawnedProducts.ContainsKey(productName))
        spawnedProducts.Remove(productName);

    Destroy(product);

    Debug.Log($"Product '{productName}' removed from the scene.");
    UpdateProductListUI();
}



    private void SaveAll()
    {
        SaveProductListManager();
        SaveProductsToFile();
    }

private void SaveProductListManager()
{
    if (!Directory.Exists(SaveFolder))
        Directory.CreateDirectory(SaveFolder);

    var prefabPath = Path.Combine(SaveFolder, $"{productListManager.name}.prefab");
    var prefab = PrefabUtility.SaveAsPrefabAsset(productListManager, prefabPath);

    Debug.Log(prefab ? $"ProductListManager saved as prefab at {prefabPath}" : "Failed to save prefab!");
}


    private void SaveProductsToFile()
    {
        var filePath = Path.Combine(ResourcesFolder, ProductsFileName);
        File.WriteAllLines(filePath, availableProducts);
        Debug.Log($"Products saved to {filePath}");
    }

private void LoadPlacedProducts()
{
    foreach (Transform child in productListManager.transform)
    {
        var productName = child.name;

        // Verifica che il prodotto non sia già registrato
        if (!spawnedProducts.ContainsKey(productName))
        {
            spawnedProducts[productName] = child.gameObject;
            Debug.Log($"Product '{productName}' loaded from scene hierarchy.");
        }
    }
}


    // Support method to create the label above the object
    private void SetupProductLabel(GameObject product, string productName)
    {
        var productBounds = product.GetComponent<Renderer>()?.bounds.size ?? Vector3.one;

        // Add the label above the product
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
