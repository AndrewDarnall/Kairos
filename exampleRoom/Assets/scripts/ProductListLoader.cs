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
    public GameObject textTagPrefab;
    public GameObject pointerPrefab;

    [Header("References")]
    public GameObject productListManager;

    private List<string> availableProducts = new();
    private Dictionary<string, GameObject> spawnedProducts = new();
    private GameObject pointerInstance;
    private const string FilePath = "Assets/Resources/Products.txt";
    private const string SaveFolder = "Assets/SavedPrefabs";

    private bool showOnlySpawned = false; // Stato del filtro

    private void Start()
    {
        InitializePointer();
        InitializeUI();
        LoadProducts();
    }

    private void Update()
    {
        if (!IsInputFieldFocused())
        {
            UpdatePointerPosition();
            HandleProductDeletion();

            // Salva tutti i dati quando si preme Shift + S
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

        // Collega l'evento al pulsante di filtro
        if (activeProductFilterButton != null)
        {
            activeProductFilterButton.onClick.AddListener(ToggleFilter);
        }
    }

    private void ToggleFilter()
    {
        showOnlySpawned = !showOnlySpawned; // Cambia stato del filtro
        UpdateProductListUI(searchInputField.text); // Aggiorna la lista
    }

    private void LoadProducts()
    {
        var file = Resources.Load<TextAsset>("Products");
        if (file == null)
        {
            Debug.LogError("Products.txt not found in Resources.");
            return;
        }

        availableProducts = file.text
            .Split('\n')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        UpdateProductListUI();
    }

    private void UpdateProductListUI(string filter = "")
    {
        // Rimuove tutti i figli esistenti in modo sicuro
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
            // Filtra solo i prodotti già spawnati
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

        // Cambia il colore dell'entry a seconda che il prodotto sia spawnato
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
            // Il prodotto è già spawnato, quindi sposta la telecamera e termina
            FocusOnProduct(spawnedProducts[productName]);
        }
        else
        {
            // Spawna un nuovo prodotto
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

        var spawnPosition = pointerInstance.transform.position;
        var newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity, productListManager.transform);
        newProduct.name = productName;

        if (textTagPrefab)
        {
            var textTag = Instantiate(textTagPrefab, newProduct.transform);
            textTag.transform.localPosition = Vector3.up * 2f;
            var textMesh = textTag.GetComponent<TextMeshPro>();

            if (textMesh)
            {
                textMesh.text = productName;
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.fontSize = 20f;
                textMesh.color = Color.yellow;
            }
        }

        spawnedProducts[productName] = newProduct;

        // Cambia il colore dell'entry su verde
        var entryImage = entry.GetComponent<Image>();
        if (entryImage != null)
            entryImage.color = Color.green;
    }

    private void FocusOnProduct(GameObject product)
    {
        if (product != null && Camera.main != null)
        {
            Camera.main.transform.position = product.transform.position - Camera.main.transform.forward * 10f;
            Debug.Log($"Camera focused on {product.name}.");
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

                // Controlla se l'oggetto ha il tag "Product" prima di eliminarlo
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

        // Controlla se il nome del prodotto è valido e non è già nella lista
        if (!string.IsNullOrEmpty(productName) && !availableProducts.Contains(productName))
        {
            availableProducts.Add(productName);
            Debug.Log($"Product '{productName}' added back to the available list.");
        }

        // Rimuove il prodotto dalla lista dei prodotti spawnati
        if (spawnedProducts.ContainsKey(productName))
            spawnedProducts.Remove(productName);

        Destroy(product);

        // Cambia il colore dell'entry su giallo
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
        File.WriteAllLines(FilePath, availableProducts);
        Debug.Log("Products saved to Products.txt");
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