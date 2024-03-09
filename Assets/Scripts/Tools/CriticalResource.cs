using System;
using UnityEngine;

[Serializable]
public class CriticalResource
{
    [SerializeField] public int Resources { get; private set; }
    private readonly int maxResources;

    public CriticalResource(int Amount)
    {
        maxResources = Amount - 1;
        Resources = Amount - 1;
    }

    public bool Take()
    {
        if (Resources >= 0)
        {
            Resources--;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Return()
    {
        Resources = Mathf.Min(Resources + 1, maxResources);
    }
}
