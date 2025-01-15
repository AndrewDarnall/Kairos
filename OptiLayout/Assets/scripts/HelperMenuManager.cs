using UnityEngine;

public class HelperMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject helperMenuPanel; // Collega il pannello nell'Inspector

    [Header("Settings")]
    public bool isHelperMenuVisibleAtStart = false; // Imposta la visibilità iniziale del menu

    private void Start()
    {
        // Configura la visibilità iniziale del menu
        if (helperMenuPanel != null)
        {
            helperMenuPanel.SetActive(isHelperMenuVisibleAtStart);
        }
        else
        {
            Debug.LogError("HelperMenuPanel is not assigned in the Inspector!");
        }
    }

    private void Update()
    {
        // Premi H per alternare la visibilità del menu
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHelperMenu();
        }
    }

    private void ToggleHelperMenu()
    {
        if (helperMenuPanel != null)
        {
            // Alterna la visibilità del pannello
            helperMenuPanel.SetActive(!helperMenuPanel.activeSelf);
        }
        else
        {
            Debug.LogError("HelperMenuPanel is not assigned in the Inspector!");
        }
    }
}
