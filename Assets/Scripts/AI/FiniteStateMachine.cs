using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    [field: SerializeField] public StateSO LastState { get; private set; }
    [field: SerializeField] public StateSO CurrentState { get; private set; }
    [field: SerializeField] public StateTableSO StateTable { get; private set; }

    private bool TransToState(StateSO nextState)
    {
        if (nextState == null) { return false; }
        LastState = CurrentState;
        CurrentState = nextState;
        return true;
    }

    // public bool Transition(int serialNumberInSequence)
    // {
    //     StateSO _state = serialNumberInSequence < CurrentState.NextStateSequence.Count
    //     ? CurrentState.NextStateSequence[serialNumberInSequence].State : null;
    //     return TransToState(_state);
    // }

    public bool Transition(int serialNumberInTable)
    {
        StateSO _state = serialNumberInTable < StateTable.stateList.Count ? StateTable.stateList[serialNumberInTable] : null;
        return TransToState(_state);
    }

    public bool ForceTransition(int serialNumberInTable)
    {
        StateSO _state = serialNumberInTable < StateTable.stateList.Count ? StateTable.stateList[serialNumberInTable] : null;
        return TransToState(_state);
    }

    public void SetStateTable(StateTableSO table)
    {
        StateTable = table;
    }

    public FiniteStateMachine(StateTableSO stateTableSO, StateSO defaultState)
    {
        if (stateTableSO == null) { return; }
        SetStateTable(stateTableSO);
        Init(defaultState);
    }
    public void Init(StateSO defaultState)
    {
        CurrentState = defaultState;
        LastState = defaultState;
    }

    public StateSO GetState(int stateIndex)
    {
        if (stateIndex >= StateTable.stateList.Count || stateIndex < 0)
        {
            return null;
        }
        return StateTable.stateList[stateIndex];
    }

    public bool IsInState(int stateIndex)
    {
        return IsInState(GetState(stateIndex));
    }

    public bool IsInState(StateSO state)
    {
        if (state == null) { return false; }
        return state == CurrentState;
    }
}
