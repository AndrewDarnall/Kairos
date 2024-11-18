using UnityEngine;
using System.IO;

public class hand_track : MonoBehaviour
{
    public GameObject left_hand_parent;
    public GameObject right_hand_parent;

    void Start()
    {
        foreach(Transform child in left_hand_parent.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.GetComponent<Renderer>() == null) // Exclude the sphere
            {
                GameObject redSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                redSphere.transform.position = child.position;
                redSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                redSphere.GetComponent<Renderer>().material.color = Color.red;
                redSphere.transform.parent = child;
            }
        }
    }

    void Update()
    {
        string path = "hand_positions.txt";
        using (StreamWriter writer = new StreamWriter(path, true)) // Append to the file instead of overwriting
        {
            foreach (Transform child in left_hand_parent.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.GetComponent<Renderer>() == null) // Exclude the sphere
                {
                    writer.WriteLine($"{child.name}: {child.position}");
                }
            }

            foreach (Transform child in right_hand_parent.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.GetComponent<Renderer>() == null) // Exclude the sphere
                {
                    writer.WriteLine($"{child.name}: {child.position}");
                }
            }
        }
    }

    private void IsHandClose()
    {
        Transform finger = null;
        foreach(Transform child in left_hand_parent.GetComponentsInChildren<Transform>())
        {
            if (child.name == "l_index_finger_tip_marker") 
            {
                finger = child;
                break;
            }
        }

        if (finger != null)
        {
            Vector3 relativePosition = left_hand_parent.transform.InverseTransformPoint(finger.transform.position);
            // Debug.Log(relativePosition);
            if (relativePosition.x > -0.08f && relativePosition.x < -0.06f &&
                relativePosition.y > 0.02f && relativePosition.y < 0.04f &&
                relativePosition.z > -0.03f && relativePosition.z < -0.01f)
            {
                Debug.Log("Hand closed detected!");
            }
        }
    }

}
