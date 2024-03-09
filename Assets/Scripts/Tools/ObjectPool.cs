using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly List<GameObject> free = new();
    private readonly List<GameObject> busy = new();
    private readonly int enlargePoolMinimumAmount;

    public event Action ObjectGet;
    public event Action ObjectCreat;
    public event Action ObjectReturn;

    public ObjectPool(GameObject prefab, int initialPoolSize, int enlargePoolMinimumAmount = 1)
    {
        this.prefab = prefab;
        this.enlargePoolMinimumAmount = enlargePoolMinimumAmount > 0 ? enlargePoolMinimumAmount : 1;
        PopulatePool(initialPoolSize);
    }

    private void PopulatePool(int initialPoolSize)
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateObject();
        }
    }

    private GameObject CreateObject()
    {
        GameObject obj = UnityEngine.Object.Instantiate(prefab);
        obj.SetActive(false);
        free.Add(obj);
        ObjectCreat?.Invoke();
        return obj;
    }

    public GameObject Get()
    {
        if (free.Count <= 0)
        {
            for (int index = 0; index < enlargePoolMinimumAmount; index++)
            {
                CreateObject();
            }
        }

        GameObject obj = free[0];

        free.Remove(obj);
        busy.Add(obj);
        obj.SetActive(true);
        ObjectGet?.Invoke();
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        busy.Remove(obj);
        free.Add(obj);
        ObjectReturn?.Invoke();
    }

    public void ReturnAll()
    {
        foreach (var item in busy)
        {
            free.Add(item);
        }
        busy.Clear();
    }
}
