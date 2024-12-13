using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCamera : MonoBehaviour
{
    float rotationX = 0f;
    float rotationY = 0f;
    public float sensitivity = 1.5f;
    public float moveSpeed = 5f; // Velocità costante

    private bool isActive = false; // Stato del movimento (attivo o disattivo)

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;  
    }

    void Update()
    {
        // Controllo per attivare/disattivare il movimento con il tasto C
        if (Input.GetKeyDown(KeyCode.C))
        {
            isActive = !isActive; // Alterna lo stato
        }

        if (!isActive){
            Cursor.lockState = CursorLockMode.None; 
            return; // Se inattivo, esci dalla funzione Update
        }

        // Movimento della telecamera con il mouse
        Cursor.lockState = CursorLockMode.Locked;
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
}
