using UnityEngine;
using UnityEngine.UI;

public class ProductMenuLoader : MonoBehaviour
{
    public GameObject buttonPrefab;       // Prefab del pulsante per ogni prodotto
    public Transform contentPanel;       // Il contenitore dei pulsanti (Content del Scroll View)
    public string[] productNames = {"Prod1", "Prod2"};        // Lista dei nomi dei prodotti

    void Start()
    {
        LoadProductMenu();
    }

    void LoadProductMenu()
    {
        foreach (string productName in productNames)
        {
            // Crea un nuovo pulsante nel pannello
            GameObject newButton = Instantiate(buttonPrefab, contentPanel);

            // Imposta il testo del pulsante con il nome del prodotto
            newButton.GetComponentInChildren<Text>().text = productName;

            // Puoi aggiungere altre funzionalità al pulsante (es. click per spawnare un prodotto)
            newButton.GetComponent<Button>().onClick.AddListener(() => OnProductButtonClick(productName));
        }
    }

    void OnProductButtonClick(string productName)
    {
        Debug.Log("Selezionato prodotto: " + productName);
        // Qui puoi aggiungere logica per il drag-and-drop o lo spawn del prodotto
    }
}
