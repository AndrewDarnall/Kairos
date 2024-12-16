using UnityEngine;
using UnityEditor; // Solo per l'Editor (PrefabUtility)
using System.IO;

public class SaverManager : MonoBehaviour
{
    public GameObject productListManager; // Oggetto contenente la gerarchia da salvare
    private string saveFolder = "Assets/SavedPrefabs"; // Cartella di salvataggio

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))      
        {
            SaveProductListManager();
        }
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
}
