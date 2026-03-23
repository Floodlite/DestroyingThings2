using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class Pooler : MonoBehaviour
{
    [SerializeField] private bool doNotDestroyOnLoad = false;
    private GameObject emptyPool;
    private static GameObject bullets;
    private static GameObject objects;
    private static Dictionary<GameObject, ObjectPool<GameObject>> poolDict;
    private static Dictionary<GameObject, GameObject> cloneDict;

    public enum PoolType
    {
        bullets,
        objects
    }

    public static PoolType PoolingType;

    private void Awake()
    {
        poolDict = new Dictionary<GameObject, ObjectPool<GameObject>>();
        cloneDict = new Dictionary<GameObject, GameObject>();
        SetObjects();
    }

    private void SetObjects()
    {
        emptyPool = new GameObject("Pools");
        bullets = new GameObject("Bullets");
        bullets.transform.SetParent(emptyPool.transform);
        objects = new GameObject("Objects");
        objects.transform.SetParent(emptyPool.transform);
        
        if(doNotDestroyOnLoad) { DontDestroyOnLoad(emptyPool.transform.root); }
    }

    private static void FillPool(GameObject prefab, Vector3 position, Quaternion rotation, PoolType poolType = PoolType.objects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, position, rotation, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDrain
        );
        poolDict.Add(prefab, pool);
    }

    private static void FillPool(GameObject prefab, Transform parent, Quaternion rotation, PoolType poolType = PoolType.objects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, parent, rotation, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDrain
        );
        poolDict.Add(prefab, pool);
    }

    private static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotation, PoolType poolType = PoolType.objects)
    {
        prefab.SetActive(false);
        GameObject objectum = Instantiate(prefab, position, rotation);
        prefab.SetActive(true);
        GameObject parentObject = SetParentObject(poolType);
        objectum.transform.SetParent(parentObject.transform);
        return objectum;
    }

    private static GameObject CreateObject(GameObject prefab, Transform parent, Quaternion rotation, PoolType poolType = PoolType.objects)
    {
        prefab.SetActive(false);
        GameObject objectum = Instantiate(prefab, parent);
        objectum.transform.localPosition = Vector3.zero;
        objectum.transform.localRotation = rotation;
        objectum.transform.localScale = Vector3.one;
        prefab.SetActive(true);
        return objectum;
    }

    private static void OnGetObject(GameObject prefab)
    {
        //palceholder
    }

    private static void OnRelease(GameObject prefab)
    {
        prefab.SetActive(false);
    }

    private static void OnDrain(GameObject prefab)
    {
        if(cloneDict.ContainsKey(prefab)) {
            cloneDict.Remove(prefab);
        }
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        switch(poolType)
        {
            case PoolType.bullets:
                return bullets;
            case PoolType.objects:
                return objects;
            default:
                return null;
        }
    }

    private static T SpawnObject<T>(GameObject spawned, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.objects) where T : Object {
        //T: generic placeholder value
        //where T: object: T constrained to be an object
        if(!poolDict.ContainsKey(spawned))
        {
            FillPool(spawned, spawnPosition, spawnRotation, poolType);
        }

        GameObject objectum = poolDict[spawned].Get();

        if(objectum != null)
        {
            if(!cloneDict.ContainsKey(objectum))
            {
                cloneDict.Add(objectum, spawned);
            }
            objectum.transform.position = spawnPosition;
            objectum.transform.rotation = spawnRotation;
            objectum.SetActive(true);

            //cases to handle
            if(typeof(T) == typeof(GameObject))
            {
                return objectum as T;
            }
            T component = objectum.GetComponent<T>();
            if(component == null)
            {
                Debug.LogWarning($"Objectum {spawned.name} lacks a component");
                return null;
            }
            return component;
        }
        return null;
    }

    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.objects) where T : Component {
        //component overload
        return SpawnObject<T>(typePrefab.gameObject, spawnPosition, spawnRotation, poolType);
    }

    public static T SpawnObject<T>(T typePrefab, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.objects) where T : Component {
        return SpawnObject<T>(typePrefab.gameObject, parent, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject spawned, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.objects) {
        //GameObject overload
        return SpawnObject<GameObject>(spawned, spawnPosition, spawnRotation, poolType);
    }
    public static GameObject SpawnObject(GameObject spawned,Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.objects) {
        return SpawnObject<GameObject>(spawned, parent, spawnRotation, poolType);
    }

    private static T SpawnObject<T>(GameObject spawned, Transform parent, Quaternion spawnRotation, PoolType poolType = PoolType.objects) where T : Object {
        if(!poolDict.ContainsKey(spawned))
        {
            FillPool(spawned, parent, spawnRotation, poolType);
        }

        GameObject objectum = poolDict[spawned].Get();

        if(objectum != null)
        {
            if(!cloneDict.ContainsKey(objectum))
            {
                cloneDict.Add(objectum, spawned);
            }
            objectum.transform.SetParent(parent);
            objectum.transform.localPosition = Vector3.zero;
            objectum.transform.localRotation = spawnRotation;
            objectum.SetActive(true);

  
            if(typeof(T) == typeof(GameObject))
            {
                return objectum as T;
            }
            T component = objectum.GetComponent<T>();
            if(component == null)
            {
                Debug.LogWarning($"Objectum {spawned.name} lacks a component");
                return null;
            }
            return component;
        }
        return null;
    }



    public static void ReleaseObjectToPool(GameObject objectum, PoolType poolType = PoolType.bullets)
    {
        if(cloneDict.TryGetValue(objectum, out GameObject prefab)) {
            GameObject parentObject = SetParentObject(poolType);
            if(objectum.transform.parent != parentObject.transform)
            {
                objectum.transform.SetParent(parentObject.transform);
            }
            
            if(poolDict.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(objectum);
            }
        }
        else {
            if(objectum.name != "Sphere") {
                Debug.LogWarning($"Object {objectum.name} is not in the pool");
            }
        }
    }
}
