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
    public float moveSpeed = 5f; // Constant speed

    private bool isActive = false; // Movement state (active or inactive)
    private bool isTopView = false; // Top view state

    public TMP_Text statusText; // Reference to the TMP_Text component for the status message

    private Vector3 previousPosition; // Previous camera position
    private Quaternion previousRotation; // Previous camera rotation

    // Specific height for the top view
    public float topViewHeight = 50f;
    public float heightAdjustSpeed = 10f; // Height adjustment speed

    void Start()
    {
        UpdateStatusText();
    }

    void Update()
    {
        // Check if an InputField is active to block camera movement
        if (IsInputFieldFocused())
            return;

        // Check to activate/deactivate movement with the C key
        if (Input.GetKeyDown(KeyCode.C))
        {
            isActive = !isActive; // Toggle the state
            UpdateStatusText();  // Update the text when the state changes
        }

        // Check to activate/deactivate the top view with the U key
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
            Cursor.lockState = CursorLockMode.None; // Release the cursor
            return; // If inactive, exit the Update function
        }

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor

        // Camera movement with the mouse
        rotationY += Input.GetAxis("Mouse X") * sensitivity;
        rotationX += Input.GetAxis("Mouse Y") * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Limit vertical rotation

        transform.localEulerAngles = new Vector3(-rotationX, rotationY, 0);

        // Camera movement with WASD
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = transform.TransformDirection(move); // Convert movement relative to the camera direction

        // Move the camera with a fixed speed
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    // Method to activate/deactivate the top view
    void ToggleTopView()
    {
        if (!isTopView)
        {
            // Save the current position and rotation
            previousPosition = transform.position;
            previousRotation = transform.rotation;

            // Set the top view: higher position and centered
            transform.position = new Vector3(transform.position.x, topViewHeight, transform.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Look downwards
        }
        else
        {
            // Restore the previous position and rotation
            transform.position = previousPosition;
            transform.rotation = previousRotation;
        }

        isTopView = !isTopView; // Change state
    }

    // Movement during the top view
    void HandleTopViewMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) // Move forward (Z axis)
        {
            move.z = 1;
        }
        else if (Input.GetKey(KeyCode.S)) // Move backward
        {
            move.z = -1;
        }
        else if (Input.GetKey(KeyCode.A)) // Move left (X axis)
        {
            move.x = -1;
        }
        else if (Input.GetKey(KeyCode.D)) // Move right
        {
            move.x = 1;
        }

        // Normalize the movement to avoid diagonals
        move = move.normalized;

        // Move the camera only on the X and Z axes
        transform.position += new Vector3(move.x, 0, move.z) * moveSpeed * Time.deltaTime;
    }

    // Height adjustment with the up/down arrows
    void HandleHeightAdjustment()
    {
        if (Input.GetKey(KeyCode.UpArrow)) // Up arrow: increase height
        {
            transform.position += Vector3.up * heightAdjustSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow)) // Down arrow: decrease height
        {
            transform.position += Vector3.down * heightAdjustSpeed * Time.deltaTime;
        }
    }

    // Method to update the status text
    void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isActive ? "Camera Movement: ON" : "Camera Movement: OFF";
        }
    }

    // Function to check if an InputField is active
    private bool IsInputFieldFocused()
    {
        // Check if a UI object is selected and if it is an InputField
        return EventSystem.current.currentSelectedGameObject != null &&
               EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
    }
}
