using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character), typeof(FiniteStateMachine))]
public class AI : MonoBehaviour
{
    protected FiniteStateMachine fsm;
    [field: SerializeField] public StateTableSO StateTable { get; private set; }
    [field: SerializeField] public Vector3 TransferPosition { get; private set; }
    protected Character character;
    protected ActionControl actionControl;
    [field: SerializeField] public List<ActionConfig> ActionConfigs { get; private set; }
    [field: SerializeField] public List<AttackAction> AttackActions { get; private set; }
    public List<ActionConfig> ActionTestTable;

    [Serializable]
    public struct AttackAction
    {
        public CombatActionConfig combatActionConfig;
        public int Priority;
    }

    virtual protected void Awake()
    {
        character = GetComponent<Character>();
        fsm = GetComponent<FiniteStateMachine>();
        actionControl = GetComponent<ActionControl>();
        ActionTestTable = new();
    }

    virtual protected void Update()
    {
        UpdateActionTestTable();
    }

    virtual protected void Start()
    {

    }

    public void SetTransferPosition(Vector3 targetPosition)
    {
        TransferPosition = targetPosition;
    }

    public void UpdateActionTestTable()
    {
        if (ActionTestTable.Count <= 0) { return; }
        ActionConfig _newAC = ActionTestTable[0];
        if (_newAC == null) { return; }
        actionControl.PlayAction(_newAC);
        ActionTestTable.RemoveAt(0);
    }

    protected int GetTotalPriority()
    {
        int _prioritys = 0;
        if (AttackActions == null || AttackActions.Count <= 0)
        {
            throw new Exception("Attack Actions is empty.");
        }
        foreach (var item in AttackActions)
        {
            _prioritys += item.Priority;
        }
        return _prioritys;
    }

    protected CombatActionConfig GetAttackActionByPriority()
    {
        int _p = GetTotalPriority();
        if (AttackActions.Count <= 0) { return null; }
        int _r = UnityEngine.Random.Range(0, _p);
        int _tmp = 0;
        foreach (var item in AttackActions)
        {
            if (_r < _tmp + item.Priority)
            {
                return item.combatActionConfig;
            }
            _tmp += item.Priority;
        }
        return null;
    }

    protected float GetMinTriggerDistance(CombatActionConfig combatActionConfig)
    {
        if (combatActionConfig.actionMoves.Count <= 0) { return 0; }
        Vector3 _disVec = Vector3.zero;
        foreach (var item in combatActionConfig.actionMoves)
        {
            if (_disVec.magnitude <= (_disVec + item.MoveVector).magnitude) { continue; }
            _disVec += item.MoveVector;
        }

        return _disVec.magnitude;
    }

    protected float GetMaxTriggerDistance(CombatActionConfig combatActionConfig)
    {
        if (combatActionConfig.actionMoves.Count <= 0) { return 0; }
        Vector3 _disVec = Vector3.zero;
        foreach (var item in combatActionConfig.actionMoves)
        {
            if (_disVec.magnitude >= (_disVec + item.MoveVector).magnitude) { continue; }
            _disVec += item.MoveVector;
        }

        return _disVec.magnitude;
    }
}
