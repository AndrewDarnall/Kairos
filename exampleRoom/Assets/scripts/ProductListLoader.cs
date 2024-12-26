using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor; // Solo per l'Editor (PrefabUtility)
using System.IO;
using System.Linq; // Per utilizzare LINQ
using UnityEngine.EventSystems; // Necessario per usare EventSystem


public class ProductListLoader : MonoBehaviour
{
    public GameObject productEntryPrefab; // Prefab for the button in the menu
    public Transform contentPanel;        // Content of the Scroll View
    public GameObject productPrefab;      // Prefab to instantiate in the scene
    public GameObject productListManager; // Parent GameObject (ProductListManager)
    public GameObject textTagPrefab;      // Prefab per il tag testuale (TextMeshPro)
    private string filePath = "Assets/Resources/Products.txt";

    public TMP_InputField searchInputField; // Riferimento al campo di input per la ricerca

    private List<string> availableProducts = new List<string>(); // Lista dei prodotti disponibili
    private string saveFolder = "Assets/SavedPrefabs"; // Cartella di salvataggio

    void Start()
    {
        // Imposta il riferimento al campo di input (modifica il percorso in base alla tua gerarchia)
        searchInputField = GameObject.Find("Canvas/searchText").GetComponent<TMP_InputField>();
        searchInputField.onValueChanged.AddListener(OnSearchValueChanged);

        contentPanel = GameObject.Find("Canvas/Scroll View/Viewport/Content").transform;
        LoadProducts();
    }

    void Update()
    {
        // Controlla se un campo di input è attualmente attivo
        if (IsInputFieldFocused())
            return;

        HandleDeletion();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) // Controlla se Shift è premuto        
        {
            if (Input.GetKeyDown(KeyCode.S)) // Controlla se S è premuto
            {
                SaveProductListManager();
                SaveProductsToFile();
            }
        }
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
        // Pulisce la UI
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Aggiunge i prodotti disponibili nella UI
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

        // Istanzia il prodotto
        GameObject newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
        newProduct.name = productName;
        newProduct.transform.SetParent(productListManager.transform);

        // Aggiunge il "tag testuale" sopra il prodotto
        if (textTagPrefab != null)
        {
            GameObject textTag = Instantiate(textTagPrefab);
            textTag.transform.SetParent(newProduct.transform);
            textTag.transform.localPosition = new Vector3(0, 2f, 0);
            textTag.transform.localScale = Vector3.one * 0.5f;

            TextMeshPro textMesh = textTag.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = productName;
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.fontSize = 150f;
                textMesh.color = Color.yellow;
            }
        }

        // Rimuove il prodotto dalla lista disponibile
        availableProducts.Remove(productName);

        // Aggiorna l'interfaccia utente
        UpdateProductListUI();
        Debug.Log($"Spawned product '{productName}' with tag and removed from the UI list.");
    }

    private void HandleDeletion()
    {
        if (Input.GetKey(KeyCode.D)) // Tiene premuto D
        {
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

        // Riaggiunge il prodotto nella lista disponibile
        if (!availableProducts.Contains(productName))
        {
            availableProducts.Add(productName);
        }

        UpdateProductListUI();
        Debug.Log($"Removed product '{productName}' and added it back to the UI list.");
    }

    void SaveProductListManager()
    {
        // Crea la cartella di salvataggio se non esiste
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            Debug.Log($"Created save folder: {saveFolder}");
        }

        // Genera il percorso del prefab
        string prefabPath = Path.Combine(saveFolder, productListManager.name + ".prefab");

        // Salva il prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(productListManager, prefabPath);
        if (prefab != null)
        {
            Debug.Log($"Prefab saved successfully at: {prefabPath}");
        }
        else
        {
            Debug.LogError("Failed to save prefab!");
        }
    }

    public void SaveProductsToFile()
    {
        StreamWriter writer = new StreamWriter(filePath, false); // Sovrascrive il file

        foreach (string productName in availableProducts)
        {
            writer.WriteLine(productName);
        }

        writer.Close();
        Debug.Log("Products saved to Products.txt");
    }

    // Metodo chiamato quando il testo del campo di ricerca cambia
    private void OnSearchValueChanged(string searchText)
    {
        UpdateProductListUI(searchText);
    }

    // Aggiorna la lista dei prodotti in base al testo di ricerca
    private void UpdateProductListUI(string filter = "")
    {
        // Pulisce la UI
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Filtra i prodotti disponibili in base al testo inserito
        IEnumerable<string> filteredProducts = availableProducts;

        if (!string.IsNullOrEmpty(filter))
        {
            filteredProducts = availableProducts
                .Where(product => product.ToLower().Contains(filter.ToLower()));
        }

        // Aggiunge i prodotti filtrati nella UI
        foreach (string productName in filteredProducts)
        {
            AddProductToUI(productName);
        }
    }

    // Funzione per verificare se un InputField è attivo
    private bool IsInputFieldFocused()
    {
        // Controlla se un oggetto UI è selezionato e se è un InputField
        return EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
    }
}
