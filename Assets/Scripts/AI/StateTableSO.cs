using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/StateMachine/StateTable")]
public class StateTableSO : ScriptableObject
{
    public string TableName;
    public List<StateSO> stateList;
}
