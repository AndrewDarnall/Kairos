using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MoveCamera : MonoBehaviour
{
    float rotationX = 0f;
    float rotationY = 0f;
    public float sensitivity = 1.5f;
    public float moveSpeed = 5f; // Speed for WASD movement
    public float heightAdjustSpeed = 10f; // Speed for height adjustment

    private bool isActive = false; // Free movement state
    private bool isTopView = false; // Top view state

    public TMP_Text statusText; // UI feedback for camera state

    private Vector3 previousPosition; // Position before switching to top view
    private Quaternion previousRotation; // Rotation before switching to top view

    public float topViewHeight = 50f; // Default height for top view

    void Start()
    {
        UpdateStatusText();
    }

    void Update()
    {
        if (IsInputFieldFocused()) return; // Block movement if UI is active

        HandleCameraToggle();
        HandleTopViewToggle();

        if (isTopView)
        {
            HandleTopViewMovement();
            HandleHeightAdjustment();
        }
        else if (isActive)
        {
            HandleFreeCameraMovement();
        }
    }

    // Toggles free movement mode
    void HandleCameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isActive = !isActive;
            Cursor.lockState = isActive ? CursorLockMode.Locked : CursorLockMode.None;
            UpdateStatusText();
        }
    }

    // Toggles top view mode
    void HandleTopViewToggle()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (!isTopView)
            {
                previousPosition = transform.position;
                previousRotation = transform.rotation;

                transform.position = new Vector3(transform.position.x, topViewHeight, transform.position.z);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Look straight down
            }
            else
            {
                transform.position = previousPosition;
                transform.rotation = previousRotation;
            }

            isTopView = !isTopView;
        }
    }

    // Free camera movement with mouse and WASD
    void HandleFreeCameraMovement()
    {
        rotationY += Input.GetAxis("Mouse X") * sensitivity;
        rotationX += Input.GetAxis("Mouse Y") * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.localEulerAngles = new Vector3(-rotationX, rotationY, 0);

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = transform.TransformDirection(move);

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // Top view movement using WASD
    void HandleTopViewMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move.z += 1; // Forward
        if (Input.GetKey(KeyCode.S)) move.z -= 1; // Backward
        if (Input.GetKey(KeyCode.A)) move.x -= 1; // Left
        if (Input.GetKey(KeyCode.D)) move.x += 1; // Right

        move.Normalize(); // Prevent faster diagonal movement

        transform.position += new Vector3(move.x, 0, move.z) * moveSpeed * Time.deltaTime;
    }

    // Adjust height in top view
    void HandleHeightAdjustment()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            transform.position += Vector3.up * heightAdjustSpeed * Time.deltaTime;
        else if (Input.GetKey(KeyCode.DownArrow))
            transform.position += Vector3.down * heightAdjustSpeed * Time.deltaTime;
    }

    // Update status text for camera state
    void UpdateStatusText()
    {
        if (statusText != null)
        {
            if (isTopView)
                statusText.text = "Top View: ACTIVE";
            else if (isActive)
                statusText.text = "Free Movement: ON";
            else
                statusText.text = "Free Movement: OFF";
        }
    }

    // Check if an InputField is focused
    private bool IsInputFieldFocused()
    {
        return EventSystem.current.currentSelectedGameObject != null &&
               EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
    }
}
