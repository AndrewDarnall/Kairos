using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    private float zCoordinate;
    private bool isSelected = false; // Stato se l'oggetto è selezionato per il movimento
    private bool isDragging = false;  // Stato se si sta trascinando con il mouse
    private string moveAxis = ""; // Asse selezionato (X, Y, Z)
    public float moveSpeed = 5f; // Velocità di movimento

    // Calcola la posizione del mouse nel mondo
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zCoordinate; // Mantiene la distanza dalla telecamera
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void OnMouseDown()
    {
        // Calcola la distanza Z dalla telecamera
        zCoordinate = Camera.main.WorldToScreenPoint(transform.position).z;

        // Calcola l'offset tra la posizione dell'oggetto e quella del mouse
        offset = transform.position - GetMouseWorldPosition();

        // Se il mouse è stato cliccato sull'oggetto, inizia il drag
        isDragging = true;

        // Se non è già selezionato, abilita il movimento
        if (!isSelected)
        {
            isSelected = true;
        }
    }

    private void OnMouseUp()
    {
        // Quando rilasci il click del mouse, ferma il drag
        isDragging = false;
    }

    private void Update()
    {
        // Se si sta trascinando, sposta l'oggetto con il mouse
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }

        // Movimento con tasti solo se l'oggetto è selezionato
        if (isSelected && !isDragging)
        {
            // Gestione della selezione dell'asse
            if (Input.GetKey(KeyCode.X))
            {
                moveAxis = "X";  // Se è premuto X, si può muovere lungo X
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                moveAxis = "Y";  // Se è premuto Y, si può muovere lungo Y
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                moveAxis = "Z";  // Se è premuto Z, si può muovere lungo Z
            }

            // Movimento lungo l'asse scelto
            if (moveAxis == "X")
            {
                // Freccia su = aumento su X, freccia giù = diminuzione su X
                if (Input.GetKey(KeyCode.UpArrow)) // Freccia su
                {
                    transform.position += Vector3.right * moveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.DownArrow)) // Freccia giù
                {
                    transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
                }
            }
            else if (moveAxis == "Y")
            {
                // Freccia su = aumento su Y, freccia giù = diminuzione su Y
                if (Input.GetKey(KeyCode.UpArrow)) // Freccia su
                {
                    transform.position += Vector3.up * moveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.DownArrow)) // Freccia giù
                {
                    transform.position -= Vector3.up * moveSpeed * Time.deltaTime;
                }
            }
            else if (moveAxis == "Z")
            {
                // Freccia su = aumento su Z, freccia giù = diminuzione su Z
                if (Input.GetKey(KeyCode.UpArrow)) // Freccia su
                {
                    transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.DownArrow)) // Freccia giù
                {
                    transform.position -= Vector3.forward * moveSpeed * Time.deltaTime;
                }
            }
        }

        // Controlla se clicco al di fuori dell'oggetto per disabilitare il movimento
        if (isSelected && !isDragging && Input.GetMouseButtonDown(0))
        {
            // Raycast per determinare se clicco fuori dall'oggetto
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit))
            {
                // Se non colpisce l'oggetto, disabilita il movimento
                isSelected = false;
                moveAxis = ""; // Reset dell'asse selezionato
            }
        }
    }
}
