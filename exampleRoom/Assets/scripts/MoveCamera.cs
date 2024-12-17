using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Namespace necessario per TextMeshPro

public class moveCamera : MonoBehaviour
{
    float rotationX = 0f;
    float rotationY = 0f;
    public float sensitivity = 1.5f;
    public float moveSpeed = 5f; // Velocità costante

    private bool isActive = false; // Stato del movimento (attivo o disattivo)
    private bool isTopView = false; // Stato della vista dall'alto

    public TMP_Text statusText; // Riferimento al componente TMP_Text per l'avviso

    private Vector3 previousPosition; // Posizione precedente della telecamera
    private Quaternion previousRotation; // Rotazione precedente della telecamera

    // Altezza specifica per la vista dall'alto
    public float topViewHeight = 50f;
    public float heightAdjustSpeed = 10f; // Velocità di aggiustamento dell'altezza

    void Start()
    {
        UpdateStatusText();
    }

    void Update()
    {
        // Controllo per attivare/disattivare il movimento con il tasto C
        if (Input.GetKeyDown(KeyCode.C))
        {
            isActive = !isActive; // Alterna lo stato
            UpdateStatusText();  // Aggiorna il testo quando lo stato cambia
        }

        // Controllo per attivare/disattivare la vista dall'alto con il tasto U
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleTopView();
        }

        if (isTopView)
        {
            HandleTopViewMovement();
            HandleHeightAdjustment();
            return;
        }

        if (!isActive)
        {
            Cursor.lockState = CursorLockMode.None; // Rilascia il cursore
            return; // Se inattivo, esci dalla funzione Update
        }

        Cursor.lockState = CursorLockMode.Locked; // Blocca il cursore

        // Movimento della telecamera con il mouse
        rotationY += Input.GetAxis("Mouse X") * sensitivity;
        rotationX += Input.GetAxis("Mouse Y") * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Limita la rotazione verticale

        transform.localEulerAngles = new Vector3(-rotationX, rotationY, 0);

        // Movimento della telecamera con WASD
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = transform.TransformDirection(move); // Converte il movimento relativo alla direzione della telecamera

        // Muovi la telecamera con la velocità fissa
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // Metodo per attivare/disattivare la vista dall'alto
    void ToggleTopView()
    {
        if (!isTopView)
        {
            // Salva la posizione e rotazione attuali
            previousPosition = transform.position;
            previousRotation = transform.rotation;

            // Imposta la vista dall'alto: posizione più alta e centrata
            transform.position = new Vector3(transform.position.x, topViewHeight, transform.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Guarda verso il basso
        }
        else
        {
            // Ripristina posizione e rotazione precedenti
            transform.position = previousPosition;
            transform.rotation = previousRotation;
        }

        isTopView = !isTopView; // Cambia stato
    }

    // Movimento durante la vista dall'alto
    void HandleTopViewMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) // Movimento in avanti (asse Z)
        {
            move.z = 1;
        }
        else if (Input.GetKey(KeyCode.S)) // Movimento indietro
        {
            move.z = -1;
        }
        else if (Input.GetKey(KeyCode.A)) // Movimento a sinistra (asse X)
        {
            move.x = -1;
        }
        else if (Input.GetKey(KeyCode.D)) // Movimento a destra
        {
            move.x = 1;
        }

        // Normalizza il movimento per evitare diagonali
        move = move.normalized;

        // Muovi la telecamera solo negli assi X e Z
        transform.position += new Vector3(move.x, 0, move.z) * moveSpeed * Time.deltaTime;
    }

    // Aggiustamento dell'altezza con le frecce su/giù
    void HandleHeightAdjustment()
    {
        if (Input.GetKey(KeyCode.UpArrow)) // Freccia su: aumenta l'altezza
        {
            transform.position += Vector3.up * heightAdjustSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow)) // Freccia giù: diminuisce l'altezza
        {
            transform.position += Vector3.down * heightAdjustSpeed * Time.deltaTime;
        }
    }

    // Metodo per aggiornare il testo dello stato
    void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isActive ? "Camera Movement: ON" : "Camera Movement: OFF";
        }
    }
}
