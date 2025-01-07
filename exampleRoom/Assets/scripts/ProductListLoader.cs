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

    [Header("Prefabs")]
    public GameObject productPrefab;
    public GameObject textTagPrefab;
    public GameObject pointerPrefab;

    [Header("References")]
    public GameObject productListManager;

    private List<string> availableProducts = new();
    private GameObject pointerInstance;
    private const string FilePath = "Assets/Resources/Products.txt";
    private const string SaveFolder = "Assets/SavedPrefabs";

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

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                if (Input.GetKeyDown(KeyCode.S)) SaveAll();
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

        availableProducts
            .Where(product => product.ToLower().Contains(filter.ToLower()))
            .ToList()
            .ForEach(AddProductToUI);
    }

    private void AddProductToUI(string productName)
    {
        var newEntry = Instantiate(productEntryPrefab, contentPanel);
        var textComponent = newEntry.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
            textComponent.text = productName;

        var button = newEntry.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => SpawnProduct(productName));

        newEntry.name = productName;
    }

    private void SpawnProduct(string productName)
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
                textMesh.fontSize = 150f;
                textMesh.color = Color.yellow;
            }
        }

        availableProducts.Remove(productName);
        UpdateProductListUI();
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

                // Controlla se l'oggetto esiste e non è già stato distrutto
                if (clickedObject != null)
                {
                    RemoveProduct(clickedObject);
                }
                else
                {
                    Debug.LogWarning("The object you tried to remove does not exist or has already been destroyed.");
                }
            }
            else
            {
                Debug.LogWarning("No object was detected under the mouse cursor.");
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

        Destroy(product);
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
