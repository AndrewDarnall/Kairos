using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 offset;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - GetMouseWorldPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = GetMouseWorldPosition() + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Rilascia l'oggetto in posizione
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
}
