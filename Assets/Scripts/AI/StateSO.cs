using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/StateMachine/State")]
public class StateSO : ScriptableObject
{
    public string StateName;
    public List<Next> NextStateSequence;
    [Serializable]
    public struct Next
    {
        public bool isBreakable;
        public StateSO State;
        public float Priority;
    }

    public StateSO GetNextState()
    {
        StateSO _s = null;

        if (NextStateSequence == null) { return _s; }

        float _targetNum = UnityEngine.Random.Range(0, GetTotalPriority());
        float _nowNum = 0;

        foreach (Next item in NextStateSequence)
        {
            float _nextNum = _nowNum + item.Priority;
            if (_targetNum < _nextNum && _targetNum >= _nowNum)
            {
                _s = item.State;
                break;
            }
            _nowNum = _nextNum;
        }

        return _s;
    }

    public float GetTotalPriority()
    {
        float _t = 0;
        if (NextStateSequence == null) { return _t; }
        foreach (var item in NextStateSequence)
        {
            _t += item.Priority;
        }
        return _t;
    }
}
