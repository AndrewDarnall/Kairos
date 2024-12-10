using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public Transform productList;

    public void SaveState()
    {
        foreach (Transform child in productList)
        {
            Debug.Log($"Saved {child.name} at {child.position}");
            // Puoi salvare queste informazioni in un file o prefab
        }
    }
}
