using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCamera : MonoBehaviour
{
    float rotationX = 0f;
    float rotationY = 0f;
    public float sensitivity = 1.5f;
    public float baseMoveSpeed = 5f; // Velocità iniziale
    public float acceleration = 2f; // Velocità di accelerazione
    public float deceleration = 5f; // Velocità di decelerazione (per fermarsi)
    private float currentMoveSpeed; // Velocità attuale

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Blocca il cursore al centro dello schermo
        currentMoveSpeed = baseMoveSpeed; // Imposta la velocità iniziale
    }

    void Update()
    {
        // Movimento della telecamera con il mouse
        rotationY += Input.GetAxis("Mouse X") * sensitivity;
        rotationX += Input.GetAxis("Mouse Y") * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Limita la rotazione verticale

        transform.localEulerAngles = new Vector3(-rotationX, rotationY, 0);

        // Movimento della telecamera con WASD
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = transform.TransformDirection(move); // Converte il movimento relativo alla direzione della telecamera

        // Gestione dell'accelerazione
        if (move.magnitude > 0) // Se ci stiamo muovendo
        {
            currentMoveSpeed += acceleration * Time.deltaTime; // Aumenta la velocità
        }
        else // Se non ci stiamo muovendo
        {
            currentMoveSpeed = Mathf.Max(baseMoveSpeed, currentMoveSpeed - deceleration * Time.deltaTime); // Decelera fino alla velocità iniziale
        }

        // Muovi la telecamera con la velocità attuale
        transform.position += move * currentMoveSpeed * Time.deltaTime;
    }
}
