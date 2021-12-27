using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NetworkedPrefab : MonoBehaviour
{
    public GameObject Prefab;
    public string Path;

    public NetworkedPrefab(GameObject obj, string path)
    {
        Prefab = obj;
        Path = FilterPath(path);
    }

    private string FilterPath(string path)
    {
        int extentionLenght = System.IO.Path.GetExtension(path).Length;
        int extraLenght = 10;
        int startIndex = path.ToLower().IndexOf("resources");

        if(startIndex == -1)
            return string.Empty;
        else
            return path.Substring(startIndex + extraLenght, path.Length - (startIndex + extraLenght + extentionLenght));
    }
}
