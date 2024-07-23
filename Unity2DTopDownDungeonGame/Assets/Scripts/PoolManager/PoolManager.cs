using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    #region Tooltip
    [Tooltip("Populate this array with prefab that you want to add to the pool and" +
        " specify the number of gameobjects to be created for each")]
    #endregion
    [SerializeField] private Pool[] poolArray = null;

    private Transform objectPoolTransform;
    private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();

    [System.Serializable]
    public struct Pool
    {
        public int poolSize;
        public GameObject prefab;
        public string componentType;
    }

    private void Start()
    {
        // this singleton gameobject will be the object pool parent
        objectPoolTransform = this.gameObject.transform;

        // create object pools on start
        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
        }
    }

    /// <summary>
    /// Create the object pool with the specified prefabs and the specified pool size for each
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="poolSize"></param>
    /// <param name="componentType"></param>
    private void CreatePool(GameObject prefab, int poolSize, string componentType)
    {
        int poolKey = prefab.GetInstanceID();

        // get prefab name
        string prefabName = prefab.name;

        // create parent gameobject to parent the child objects to
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");

        parentGameObject.transform.SetParent(objectPoolTransform);

        if(!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            for(int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;

                newObject.SetActive(false);

                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    /// <summary>
    /// Reuse a gameobject component in the pool
    /// </summary>
    /// <param name="prefab"> is the prefab gameobject containing the component</param>
    /// <param name="position"> is the world position for the gameobject where it should
    /// appear when enabled</param>
    /// <param name="rotation"> should be set if the gameobject need to be rotated</param>
    /// <returns></returns>
    public Component ReUseComponent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if(poolDictionary.ContainsKey(poolKey))
        {
            // get object from pool queue
            Component componentToReUse = GetComponentFromPool(poolKey);

            ResetObject(position, rotation, componentToReUse, prefab);

            return componentToReUse;
        }
        else
        {
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }

    /// <summary>
    /// Get a gameobject component from the pool using the poolkey
    /// </summary>
    /// <param name="poolKey"></param>
    /// <returns></returns>
    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReUse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReUse);

        if(componentToReUse.gameObject.activeSelf == true)
        {
            componentToReUse.gameObject.SetActive(false);
        }

        return componentToReUse;
    }

    /// <summary>
    /// Reset the gameobject
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="componentToReUse"></param>
    /// <param name="prefab"></param>
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReUse, GameObject prefab)
    {
        componentToReUse.transform.position = position;
        componentToReUse.transform.rotation = rotation;
        componentToReUse.gameObject.transform.localScale = prefab.transform.localScale;
    }

    #region VALIDATION
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(poolArray), poolArray);
    }
#endif
    #endregion
}
