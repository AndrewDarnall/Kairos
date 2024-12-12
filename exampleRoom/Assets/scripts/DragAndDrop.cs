using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    private float zCoordinate;

    // Calculate the mouse position in world space
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zCoordinate; // Maintain the distance from the camera
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void OnMouseDown()
    {
        // Calculate the Z distance from the camera
        zCoordinate = Camera.main.WorldToScreenPoint(transform.position).z;

        // Calculate the offset between the object's position and the mouse position
        offset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        // Move the object following the mouse, maintaining the offset
        transform.position = GetMouseWorldPosition() + offset;
    }
}
