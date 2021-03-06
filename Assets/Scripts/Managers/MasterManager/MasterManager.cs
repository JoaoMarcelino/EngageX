using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Photon.Pun;

[CreateAssetMenu(menuName = "Singletons/MasterManager")]

public class MasterManager: SingletonScriptableObject<MasterManager>
{
    [SerializeField] private GameSettings _gameSettings;
    public static GameSettings GameSettings { get { return Instance._gameSettings; } }
    [SerializeField] private List<NetworkedPrefab> _networkedPrefabs = new List<NetworkedPrefab>();

    public static GameObject NetworkInstantiate(GameObject obj, Vector3 position, Quaternion rotation, bool roomObject = false, object[] data = null)
    {
        foreach (NetworkedPrefab networkedPrefab in Instance._networkedPrefabs)
        {
            if(networkedPrefab.Prefab == obj)
            {
                if(networkedPrefab.Path != string.Empty)
                {   
                    GameObject result;

                    if(roomObject && data != null)
                        result = PhotonNetwork.InstantiateRoomObject(networkedPrefab.Path, position, rotation, 0, data);
                    else if(roomObject)
                        result = PhotonNetwork.InstantiateRoomObject(networkedPrefab.Path, position, rotation);
                    else if(data != null)
                        result = PhotonNetwork.Instantiate(networkedPrefab.Path, position, rotation, 0, data);
                    else
                        result = PhotonNetwork.Instantiate(networkedPrefab.Path, position, rotation);

                    return result;
                }
                else
                {
                    Debug.LogError("Path is empty for gameObject name " + networkedPrefab.Prefab);
                    return null;
                }
            }
        }
        return null;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void PopulateNetworkedPrefabs()
    {
        #if UNITY_EDITOR

        Instance._networkedPrefabs.Clear();
        
        GameObject[] results = Resources.LoadAll<GameObject>("");

        for (int i = 0; i < results.Length; i++)
        {
            if(results[i].GetComponent<PhotonView>() != null)
            {
                string path = AssetDatabase.GetAssetPath(results[i]);
                Debug.Log(path);
                Instance._networkedPrefabs.Add(new NetworkedPrefab(results[i], path));
            }
        }
        #endif
    }

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }

    public static long GetCurrentTimestamp()
    {
        return GetTimestamp(DateTime.UtcNow);
    }

    public static byte[] ToByteArray<T>(T obj)
    {
        if(obj == null)
            return null;
        
        BinaryFormatter bf = new BinaryFormatter();
        
        using(MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static T FromByteArray<T>(byte[] data)
    {
        if(data == null)
            return default(T);
        
        BinaryFormatter bf = new BinaryFormatter();
        
        using(MemoryStream ms = new MemoryStream(data))
        {
            object obj = bf.Deserialize(ms);
            return (T)obj;
        }
    }
}