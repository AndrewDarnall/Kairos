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

    public TMP_Text statusText; // Riferimento al componente TMP_Text per l'avviso

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

        if (!isActive){
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

    // Metodo per aggiornare il testo dello stato
    void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isActive ? "Camera Movement: ON" : "Camera Movement: OFF";
        }
    }
}
