using System.Collections.Generic;
using UnityEngine;

public class Pool : GOSingleton<Pool> 
{
    [SerializeField]
    public Dictionary<string, GameObject> pooledPrefabs = new Dictionary<string, GameObject>();
    
    private Dictionary<string, Queue<GameObject>> poolQueueDic = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        AddPool(Resources.Load<GameObject>("Prefabs/Rain"), 1000);
    }

    public void AddPool(GameObject obj, int Count)
    {
        string objName = obj.name;
        pooledPrefabs.Add(objName, obj);    
        poolQueueDic.Add(objName, new Queue<GameObject>());
        for (int j = 0; j < Count; j++)
        {
            poolQueueDic[objName].Enqueue(CreateObject(objName));
        }
    }

    private GameObject CreateObject(string objName)
    {
        GameObject newObj = Instantiate(pooledPrefabs[objName]);
        newObj.name = objName;  
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public GameObject GetObject(string objName)
    {
        Queue<GameObject> poolQueue;
        if(poolQueueDic.TryGetValue(objName, out poolQueue))
        {
            GameObject obj;
            if (poolQueue.Count > 0)
            {
                obj = poolQueue.Dequeue();
            }
            else
            {
                obj = CreateObject(objName);
            }
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        return null;
    }

    public void DisableObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolQueueDic[obj.name].Enqueue(obj);
    }
}
