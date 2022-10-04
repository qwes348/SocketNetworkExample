using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;
    public List<Poolable> poolableList;
    [SerializeField]
    private List<Poolable> currentActivePoolables;

    private Dictionary<string, Stack<Poolable>> poolDictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        poolDictionary = new Dictionary<string, Stack<Poolable>>();

        foreach(var poolObj in poolableList)
        {
            if (poolObj == null)
                continue;
            poolDictionary.Add(poolObj.id, new Stack<Poolable>());

            for (int i = 0; i < poolObj.createCountInAwake; i++)
            {
                Poolable clone = Instantiate(poolObj.gameObject, transform).GetComponent<Poolable>();
                clone.gameObject.SetActive(false);
                poolDictionary[poolObj.id].Push(clone);
            }
        }
    }

    public Poolable Pop(Poolable poolObj)
    {
        if(poolDictionary.ContainsKey(poolObj.id))
        {
            Poolable returnObj = null;
            while (poolDictionary[poolObj.id].Count > 0 && returnObj == null)
            {
                returnObj = poolDictionary[poolObj.id].Pop();
                if (returnObj.isUsing)
                {
                    returnObj = null;
                    continue;
                }

                returnObj.isUsing = true;
                returnObj.onPop?.Invoke();                
            }

            if (returnObj == null)
            {
                returnObj = Instantiate(poolObj.gameObject, transform).GetComponent<Poolable>();
                returnObj.gameObject.SetActive(false);
                returnObj.isUsing = true;
                returnObj.onPop?.Invoke();

                //return returnObj;
            }
            //else
            //    return returnObj;

            currentActivePoolables.Add(returnObj);
            return returnObj;
        }
        else
        {
            poolableList.Add(poolObj);
            poolDictionary.Add(poolObj.id, new Stack<Poolable>());
            Poolable clone = Instantiate(poolObj, transform).GetComponent<Poolable>();
            clone.gameObject.SetActive(false);
            clone.isUsing = true;
            if (clone.isAutoPooling)
                clone.ResetAutoPoolingTimer();
            clone.onPop?.Invoke();

            currentActivePoolables.Add(clone);
            return clone;
        }
    }

    public Poolable Pop(string id)
    {
        if(!poolDictionary.ContainsKey(id))
        {
            Debug.LogError("ID에 해당하는 풀링된 오브젝트 없음");
            return null;
        }

        if (poolDictionary[id].Count > 0)
        {
            Poolable returnObj = null;
            while (poolDictionary[id].Count > 0 && returnObj == null)
            {
                returnObj = poolDictionary[id].Pop();
                if (returnObj.isUsing)
                {
                    returnObj = null;
                    continue;
                }

                returnObj.isUsing = true;
                returnObj.onPop?.Invoke();
            }

            if (returnObj == null)
            {
                returnObj = Instantiate(gameObject, transform).GetComponent<Poolable>();
                returnObj.gameObject.SetActive(false);
                returnObj.isUsing = true;
                returnObj.onPop?.Invoke();
            }

            currentActivePoolables.Add(returnObj);
            return returnObj;
        }
        else
        {
            Poolable prefab = poolableList.Find(p => p.id == id);
            if (prefab == null)
            {
                Debug.LogError("ID에 해당하는 풀링된 오브젝트 없음");
                return null;
            }

            Poolable clone = Instantiate(prefab.gameObject, transform).GetComponent<Poolable>();
            clone.gameObject.SetActive(false);
            clone.isUsing = true;
            if (clone.isAutoPooling)
                clone.ResetAutoPoolingTimer();
            clone.onPop?.Invoke();
            currentActivePoolables.Add(clone);

            return clone;
        }
    }

    public void Push(Poolable poolObj)
    {
        if (!poolDictionary.ContainsKey(poolObj.id))
        {
            poolDictionary.Add(poolObj.id, new Stack<Poolable>());
            poolableList.Add(poolObj);
        }

        poolObj.onPush?.Invoke();
        poolObj.isUsing = false;
        poolDictionary[poolObj.id].Push(poolObj);
        poolObj.gameObject.SetActive(false);

        if (currentActivePoolables.Contains(poolObj))
            currentActivePoolables.Remove(poolObj);
    }

    public void PushAllActivePoolables()
    {
        if (currentActivePoolables.Count <= 0)
            return;

        List<Poolable> copyList = new List<Poolable>();

        foreach (var p in currentActivePoolables)
            copyList.Add(p);

        foreach (var p in copyList)
            Push(p);

        currentActivePoolables.Clear();
    }
}
