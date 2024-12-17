using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor; // Solo per l'Editor (PrefabUtility)
using System.IO;

public class ProductListLoader : MonoBehaviour
{
    public GameObject productEntryPrefab; // Prefab for the button in the menu
    public Transform contentPanel;        // Content of the Scroll View
    public GameObject productPrefab;      // Prefab to instantiate in the scene
    public GameObject productListManager; // Parent GameObject (ProductListManager)
    public GameObject textTagPrefab;      // Prefab per il tag testuale (TextMeshPro)

    private List<string> availableProducts = new List<string>();
    private List<string> removedProducts = new List<string>();

    private string saveFolder = "Assets/SavedPrefabs"; // Cartella di salvataggio

    void Start()
    {   
        contentPanel = GameObject.Find("Canvas/Scroll View/Viewport/Content").transform;
        LoadProducts();
    }

    void Update()
    {
        HandleDeletion();

        if (Input.GetKeyDown(KeyCode.S))      
        {
            SaveProductListManager();
            SaveProductsToFile();
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

        // Istanzia il prodotto
        GameObject newProduct = Instantiate(productPrefab, spawnPosition, Quaternion.identity);
        newProduct.name = productName;
        newProduct.transform.SetParent(productListManager.transform);

        // Aggiungi il "tag testuale" sopra il prodotto
        if (textTagPrefab != null)
        {
            // Instanzia il prefab dell'etichetta
            GameObject textTag = Instantiate(textTagPrefab);

            // Imposta il "parent" del testo come il prodotto appena creato
            textTag.transform.SetParent(newProduct.transform);

            // Posiziona l'etichetta sopra il prodotto
            textTag.transform.localPosition = new Vector3(0, 2f, 0); // Altezza sopra il prodotto
            textTag.transform.localScale = Vector3.one * 0.5f; // Riduci la scala se necessario

            //Metti la dimensione font pari a 36
            textTag.GetComponent<TextMeshPro>().fontSize = 150;


            // Configura il componente TextMeshPro
            TextMeshPro textMesh = textTag.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                // Pulisce il testo predefinito (se presente)
                textMesh.text = "";  // Azzeriamo il testo se c'è un valore predefinito

                // Imposta il testo con il nome del prodotto
                textMesh.text = productName; // Imposta il testo come il nome del prodotto

                // Impostazioni aggiuntive per il testo (facoltativo)
                textMesh.alignment = TextAlignmentOptions.Center; // Allineamento centrato
                textMesh.fontSize = 150f; // Dimensione leggibile
                textMesh.color = Color.yellow; // Colore per visibilità
            }
        }

        // Aggiorna le liste
        availableProducts.Remove(productName);
        removedProducts.Add(productName);

        UpdateProductListUI();
        Debug.Log($"Spawned product '{productName}' with tag and removed from the UI list.");
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

    void SaveProductListManager()
    {
        // Verifica che ProductListManager sia assegnato
        if (productListManager == null)
        {
            Debug.LogError("ProductListManager is not assigned!");
            return;
        }

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
        string filePath = "Assets/Resources/Products.txt"; // Percorso del file di salvataggio
        StreamWriter writer = new StreamWriter(filePath, false);  // 'false' per sovrascrivere il file

        foreach (string productName in availableProducts)
        {
            writer.WriteLine(productName);  // Scrive ogni prodotto su una nuova riga
        }

        writer.Close();  // Chiude il file
        Debug.Log("Products saved to Products.txt");
    }
}
