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
    [Header("Offers Menu Elements")]
    public GameObject offersScrollView; // Scroll View separata per le offerte
    public Button toggleOffersButton; // Pulsante per alternare tra i menu
    public TMP_InputField offersInputField; // Campo di input per nuove offerte
    public Transform offersContentPanel; // Contenuto dello Scroll View delle offerte


    private List<string> activeOffers = new(); // Lista delle offerte attive
    private bool isOffersMenuVisible = false; // Stato del menu delle offerte

    [Header("UI Elements")]
    public GameObject productEntryPrefab;
    public Transform contentPanel;
    public TMP_InputField searchInputField;
    public Button activeProductFilterButton;
    public GameObject productsScrollView; // Riferimento al menu dei prodotti

    [Header("Prefabs")]
    public GameObject productPrefab;
    public GameObject pointerPrefab;
        public GameObject offerPlaceholderPrefab; // Prefab per l'offerta nella scena

    private Dictionary<string, GameObject> spawnedOffers = new(); // Dizionario per le offerte piazzate


    [Header("References")]
    public GameObject productListManager;

    private List<string> availableProducts = new();
    private Dictionary<string, GameObject> spawnedProducts = new();
    private GameObject pointerInstance;

    private const string ResourcesFolder = "Assets/Resources/";
    private const string SaveFolder = "Assets/SavedPrefabs/";
    private const string ProductsFileName = "Products.txt";
    private const string PlacedProductsFileName = "PlacedProducts.txt";
    private const string PlacedOffersFileName = "PlacedOffers.txt";


    private bool showOnlySpawned = false;

    private void Start()
    {
        InitializePointer();
    InitializeUI();
    LoadProducts();
    LoadPlacedProducts(); // Carica i prodotti piazzati
    LoadPlacedOffers();   // Carica le offerte piazzate
    UpdateProductListUI();
    InitializeOffersMenu();
    LoadOffers(); // Carica le offerte dal file
    UpdateOffersUI(); // Aggiorna la UI delle offerte

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
    // Assegna direttamente il riferimento al menu dei prodotti
    productsScrollView = GameObject.Find("Canvas/Scroll View");
    if (productsScrollView == null)
    {
        Debug.LogError("ProductsScrollView not found!");
    }

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

    if (isOffersMenuVisible)
    {
        // Mostra solo le offerte piazzate
        UpdateOffersUI();
    }
    else
    {
        // Mostra solo i prodotti piazzati
        UpdateProductListUI(searchInputField.text);
    }
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


private const string OffersFileName = "Offers.txt";

private void HandleOfferSelection(string offerText, GameObject entry)
{
    if (spawnedOffers.ContainsKey(offerText))
    {
        // Se l'offerta è già piazzata, focalizza la camera su di essa
        FocusOnOffer(spawnedOffers[offerText]);
    }
    else
    {
        // Se l'offerta non è ancora piazzata, spawnala
        SpawnOffer(offerText, entry);
    }
}



private void SpawnOffer(string offerText, GameObject entry)
{
    if (pointerInstance == null)
    {
        Debug.LogWarning("Pointer instance is missing or destroyed.");
        return;
    }

    if (spawnedOffers.ContainsKey(offerText))
    {
        Debug.LogWarning($"Offer '{offerText}' is already spawned.");
        return;
    }

    var globalSpawnPosition = pointerInstance.transform.position;
    var parentTransform = productListManager.transform;
    var localSpawnPosition = parentTransform.InverseTransformPoint(globalSpawnPosition);

    var newOffer = Instantiate(offerPlaceholderPrefab, parentTransform);
    newOffer.transform.localPosition = localSpawnPosition;
    newOffer.name = offerText;

    SetupOfferLabel(newOffer, offerText);

    spawnedOffers[offerText] = newOffer;

    // Aggiorna la voce UI per indicare che è attiva
    var entryImage = entry.GetComponent<Image>();
    if (entryImage != null)
        entryImage.color = Color.green;

    Debug.Log($"Offer '{offerText}' spawned at local position {localSpawnPosition}.");
}


private void FocusOnOffer(GameObject offer)
{
    if (offer != null && Camera.main != null)
    {
        Vector3 offset = Camera.main.transform.forward * 10f;
        Camera.main.transform.position = offer.transform.position - offset;
        Camera.main.transform.LookAt(offer.transform);

        Debug.Log($"Camera focused on {offer.name}.");
    }
    else
    {
        Debug.LogWarning("Offer or camera is null, focus operation aborted.");
    }
}


    private void SetupOfferLabel(GameObject offer, string offerText)
    {
        var offerBounds = offer.GetComponent<Renderer>()?.bounds.size ?? Vector3.one;

        // Aggiungi l'etichetta sopra l'offerta
        GameObject textObject = new GameObject("OfferNameText");
        textObject.transform.SetParent(offer.transform);

        float textOffset = offerBounds.y + 2f;
        textObject.transform.localPosition = Vector3.up * textOffset;

        var textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = offerText;
        textMesh.fontSize = 5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;

        if (Camera.main != null)
        {
            textObject.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        }
    }

    private void RemoveOffer(GameObject offer)
    {
        if (offer == null)
        {
            Debug.LogWarning("The offer you are trying to remove does not exist.");
            return;
        }

        string offerName = offer.name;

        if (spawnedOffers.ContainsKey(offerName))
            spawnedOffers.Remove(offerName);

        Destroy(offer);

        Debug.Log($"Offer '{offerName}' removed from the scene.");
        UpdateOffersUI();
    }

private void SaveOfferToFile(string offerText)
{
    var filePath = Path.Combine(ResourcesFolder, OffersFileName);
    if (!File.Exists(filePath))
    {
        File.Create(filePath).Dispose();
        Debug.Log($"Offers file created at {filePath}");
    }

    var existingOffers = File.ReadAllLines(filePath).ToList();
    if (!existingOffers.Contains(offerText))
    {
        existingOffers.Add(offerText);
        File.WriteAllLines(filePath, existingOffers);
        Debug.Log($"Offer '{offerText}' added to {filePath}");
    }
    else
    {
        Debug.Log($"Offer '{offerText}' already exists in {filePath}");
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
    SavePlacedOffers();
    SaveAllOffersToFile();
}

private void SaveAllOffersToFile()
{
    var filePath = Path.Combine(ResourcesFolder, OffersFileName);
    if (!Directory.Exists(ResourcesFolder))
        Directory.CreateDirectory(ResourcesFolder);

    File.WriteAllLines(filePath, activeOffers);
    Debug.Log($"All offers saved to {filePath}");
}


private void SavePlacedOffers()
{
    var filePath = Path.Combine(ResourcesFolder, PlacedOffersFileName);

    if (!Directory.Exists(ResourcesFolder))
        Directory.CreateDirectory(ResourcesFolder);

    var lines = spawnedOffers.Select(o =>
    {
        var position = o.Value.transform.localPosition;
        return $"{o.Key}|{position.x},{position.y},{position.z}";
    }).ToList();

    File.WriteAllLines(filePath, lines);

    Debug.Log($"Placed offers saved to {filePath}");
}




private void LoadPlacedOffers()
{
    var filePath = Path.Combine(ResourcesFolder, PlacedOffersFileName);
    if (!File.Exists(filePath))
    {
        Debug.LogWarning($"PlacedOffers file {PlacedOffersFileName} not found.");
        return;
    }

    var lines = File.ReadAllLines(filePath);
    foreach (var line in lines)
    {
        var parts = line.Split('|');
        if (parts.Length != 2) continue;

        var offerName = parts[0];
        var positionParts = parts[1].Split(',');
        if (positionParts.Length != 3) continue;

        if (!float.TryParse(positionParts[0], out var x) ||
            !float.TryParse(positionParts[1], out var y) ||
            !float.TryParse(positionParts[2], out var z)) continue;

        var position = new Vector3(x, y, z);

        if (!spawnedOffers.ContainsKey(offerName))
        {
            var newOffer = Instantiate(offerPlaceholderPrefab, productListManager.transform);
            newOffer.transform.localPosition = position;
            newOffer.name = offerName;

            SetupOfferLabel(newOffer, offerName);
            spawnedOffers[offerName] = newOffer;

            Debug.Log($"Offer '{offerName}' loaded at {position}");
        }
    }
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

private void LoadOffers()
{
    var filePath = Path.Combine(ResourcesFolder, OffersFileName);
    if (!File.Exists(filePath))
    {
        Debug.LogWarning($"Offers file {OffersFileName} not found. Creating a new one.");
        File.Create(filePath).Dispose();
        return;
    }

    activeOffers = File.ReadAllLines(filePath)
        .Select(o => o.Trim())
        .Where(o => !string.IsNullOrWhiteSpace(o))
        .Distinct()
        .ToList();

    UpdateOffersUI();
    Debug.Log($"Loaded {activeOffers.Count} offers from {filePath}");
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

    private void InitializeOffersMenu()
{
    // Collega gli elementi del menu Offers
    offersScrollView = GameObject.Find("Canvas/OffersScrollView");
    if (offersScrollView == null)
        Debug.LogError("OffersScrollView not found!");

    toggleOffersButton = GameObject.Find("Canvas/ToggleOffersButton")?.GetComponent<Button>();
    if (toggleOffersButton == null)
        Debug.LogError("ToggleOffersButton not found!");

    offersInputField = GameObject.Find("Canvas/OffersInputField")?.GetComponent<TMP_InputField>();
    if (offersInputField == null)
        Debug.LogError("OffersInputField not found!");

    offersContentPanel = GameObject.Find("Canvas/OffersScrollView/Viewport/Content")?.transform;
    if (offersContentPanel == null)
        Debug.LogError("OffersContentPanel not found!");

    // Configura gli eventi
    if (toggleOffersButton != null)
        toggleOffersButton.onClick.AddListener(ToggleOffersMenu);

    if (offersInputField != null)
        offersInputField.onSubmit.AddListener(AddOffer);

    // Assicura che il menu offerte sia inizialmente nascosto
    if (offersScrollView != null)
        offersScrollView.SetActive(false);

    // Nascondi il campo di input delle offerte all'inizio
    if (offersInputField != null)
        offersInputField.gameObject.SetActive(false);

    // Assicura che la barra di ricerca dei prodotti sia visibile all'inizio
    if (searchInputField != null)
        searchInputField.gameObject.SetActive(true);
}



private void ToggleOffersMenu()
{
    isOffersMenuVisible = !isOffersMenuVisible;

    // Usa i riferimenti diretti per alternare la visibilità dei menu
    if (productsScrollView != null)
    {
        productsScrollView.SetActive(!isOffersMenuVisible);
    }

    if (offersScrollView != null)
    {
        offersScrollView.SetActive(isOffersMenuVisible);
    }

    // Alterna la visibilità tra la barra di ricerca dei prodotti e il campo di input delle offerte
    if (searchInputField != null)
        searchInputField.gameObject.SetActive(!isOffersMenuVisible);

    if (offersInputField != null)
        offersInputField.gameObject.SetActive(isOffersMenuVisible);

    // Cambia il testo del pulsante
    var buttonText = toggleOffersButton.GetComponentInChildren<TextMeshProUGUI>();
    if (buttonText != null)
    {
        buttonText.text = isOffersMenuVisible ? "Products" : "Offers";
    }

    // Aggiorna la UI del menu visibile
    if (isOffersMenuVisible)
    {
        UpdateOffersUI();
    }
    else
    {
        UpdateProductListUI(searchInputField != null ? searchInputField.text : "");
    }
}


private void AddOffer(string offerText)
{
    if (string.IsNullOrWhiteSpace(offerText)) return;

    activeOffers.Add(offerText);

    var newOfferEntry = Instantiate(productEntryPrefab, offersContentPanel);
    var textComponent = newOfferEntry.GetComponentInChildren<TextMeshProUGUI>();
    if (textComponent != null)
        textComponent.text = offerText;

    newOfferEntry.name = offerText;

    var button = newOfferEntry.GetComponent<Button>();
    if (button != null)
        button.onClick.AddListener(() => HandleOfferSelection(offerText, newOfferEntry));

    offersInputField.text = string.Empty;
}


private void UpdateOffersUI()
{
    // Cancella tutte le voci esistenti nel pannello delle offerte
    foreach (Transform child in offersContentPanel)
    {
        Destroy(child.gameObject);
    }

    // Filtra le offerte attive o tutte le offerte
    var filteredOffers = activeOffers;

    if (showOnlySpawned)
    {
        // Mostra solo le offerte piazzate
        filteredOffers = filteredOffers
            .Where(offer => spawnedOffers.ContainsKey(offer))
            .ToList();
    }

    foreach (var offer in filteredOffers)
    {
        var newOfferEntry = Instantiate(productEntryPrefab, offersContentPanel);
        var textComponent = newOfferEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
            textComponent.text = offer;

        newOfferEntry.name = offer;

        var button = newOfferEntry.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(() => HandleOfferSelection(offer, newOfferEntry));

        var entryImage = newOfferEntry.GetComponent<Image>();
        if (entryImage != null)
        {
            entryImage.color = spawnedOffers.ContainsKey(offer) ? Color.green : Color.yellow;
        }
    }
}


}