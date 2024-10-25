using UnityEngine;

public class PlanogramManager : MonoBehaviour
{
    private GameObject planogramPrefab;

    void Start()
    {
        // Ensure planogramPrefab is assigned before proceeding
        if (planogramPrefab == null)
        {
            Debug.LogError("Planogram prefab not assigned!");
            return;
        }

        // Instantiate the planogram and configure it
        GameObject planogram = Instantiate(planogramPrefab, transform.position, transform.rotation);
        EnsureMeshCollider(planogram);
        
        GameObject plane = CreatePlane(planogram);
        HidePlanogramMeshRenderers(planogram, plane); 
        CalculateBaseDimensions(plane);
    }

    private void EnsureMeshCollider(GameObject planogram)
    {
        // Add a MeshCollider if one does not exist
        if (planogram.GetComponent<MeshCollider>() == null)
        {
            MeshCollider collider = planogram.AddComponent<MeshCollider>();
            collider.convex = true;
        }
        Physics.SyncTransforms(); // Sync physics transforms
    }

    private void HidePlanogramMeshRenderers(GameObject planogram, GameObject plane)
    {
        foreach (var renderer in planogram.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer.gameObject != plane) // Check if the renderer belongs to the plane
            {
                renderer.enabled = false;
            }
        }
    }

    private GameObject CreatePlane(GameObject obj)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        MeshCollider collider = obj.GetComponent<MeshCollider>();

        // Calculate and set plane properties based on the planogram's collider
        Vector3 bottomPosition = new Vector3(collider.bounds.center.x, collider.bounds.min.y, collider.bounds.center.z);
        plane.transform.SetPositionAndRotation(bottomPosition, obj.transform.rotation);
        plane.transform.SetParent(obj.transform, true); // Set the plane as a child of the planogram

        // Configure the plane's renderer
        MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
        if (planeRenderer != null)
        {
            planeRenderer.material = new Material(Shader.Find("Standard")) { color = Color.white }; // Assign a visible material
            planeRenderer.enabled = true; // Make the plane visible
        }

        return plane;
    }

    private void CalculateBaseDimensions(GameObject plane)
    {
        Debug.Log($"Plane Dimensions:\n" +
                    $"Position [X: {plane.transform.position.x:F2}, Y: {plane.transform.position.y:F2}, Z: {plane.transform.position.z:F2}]\n" +
                    $"Rotation [X: {plane.transform.rotation.eulerAngles.x:F2}, Y: {plane.transform.rotation.eulerAngles.y:F2}, Z: {plane.transform.rotation.eulerAngles.z:F2}]\n" +
                    $"Scale [X: {plane.transform.localScale.x:F2}, Y: {plane.transform.localScale.y:F2}, Z: {plane.transform.localScale.z:F2}]");
    }
}
