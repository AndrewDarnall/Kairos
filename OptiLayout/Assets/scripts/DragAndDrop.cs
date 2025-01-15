using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // For TextMeshPro InputField

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    private float zCoordinate;
    private bool isSelected = false; // State if the object is selected for movement
    private bool isDragging = false; // State if dragging with the mouse
    private string moveAxis = "";    // Selected axis (X, Y, Z)
    public float moveSpeed = 5f;     // Movement speed

    // Calculate the mouse position in the world
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zCoordinate; // Maintain the distance from the camera
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void OnMouseDown()
    {
        // Check if the focus is on an InputField
        if (IsInputFieldFocused())
            return;

        // Calculate the Z distance from the camera
        zCoordinate = Camera.main.WorldToScreenPoint(transform.position).z;

        // Calculate the offset between the object's position and the mouse position
        offset = transform.position - GetMouseWorldPosition();

        // If the mouse is clicked on the object, start dragging
        isDragging = true;

        // If not already selected, enable movement
        if (!isSelected)
        {
            isSelected = true;
        }
    }

    private void OnMouseUp()
    {
        // When the mouse click is released, stop dragging
        isDragging = false;
    }

    private void Update()
    {
        // Check if the focus is on an InputField
        if (IsInputFieldFocused())
            return;

        // If dragging, move the object with the mouse
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }

        // Movement with keys only if the object is selected
        if (isSelected && !isDragging)
        {
            // Axis selection management
            if (Input.GetKey(KeyCode.X))
            {
                moveAxis = "X"; // If X is pressed, move along X
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                moveAxis = "Y"; // If Y is pressed, move along Y
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                moveAxis = "Z"; // If Z is pressed, move along Z
            }

            // Movement along the selected axis
            if (moveAxis == "X")
            {
                if (Input.GetKey(KeyCode.UpArrow)) // Up arrow
                {
                    transform.position += Vector3.right * moveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.DownArrow)) // Down arrow
                {
                    transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
                }
            }
            else if (moveAxis == "Y")
            {
                if (Input.GetKey(KeyCode.UpArrow)) // Up arrow
                {
                    transform.position += Vector3.up * moveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.DownArrow)) // Down arrow
                {
                    transform.position -= Vector3.up * moveSpeed * Time.deltaTime;
                }
            }
            else if (moveAxis == "Z")
            {
                if (Input.GetKey(KeyCode.UpArrow)) // Up arrow
                {
                    transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.DownArrow)) // Down arrow
                {
                    transform.position -= Vector3.forward * moveSpeed * Time.deltaTime;
                }
            }
        }

        // Check if clicking outside the object to disable movement
        if (isSelected && !isDragging && Input.GetMouseButtonDown(0))
        {
            // Raycast to determine if clicking outside the object
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit))
            {
                // If it doesn't hit the object, disable movement
                isSelected = false;
                moveAxis = ""; // Reset the selected axis
            }
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
