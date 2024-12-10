using System.IO;
using UnityEngine;

public class ProductLoader : MonoBehaviour
{
    public string[] LoadProductNames()
    {
        TextAsset file = Resources.Load<TextAsset>("Products");
        return file.text.Split('\n');
    }
}
