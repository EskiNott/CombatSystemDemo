using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossAI : AI
{
    public float DistanceValue;

    protected override void Awake()
    {
        base.Awake();
        Roam_Timer = new();
        Resist_Timer = new();
    }

    override protected void Start()
    {
        base.Start();
        StartRoam();
        StartGeneral();
    }

    override protected void Update()
    {
        base.Update();
        UpdateSituation();
        UpdateTimer();
        if (character.GetLockTarget() != null)
        {
            DistanceValue = Vector3.Distance(character.GetTransform().position, character.GetLockTarget().GetTransform().position);
        }
    }

    private void UpdateSituation()
    {
        UpdateFirstMet();
        UpdateRoam();
        UpdateAttack();
        UpdateGeneral();
    }

    #region General

    [Header("General")]
    [SerializeField] Timer Resist_Timer;
    [SerializeField][Range(0, 1f)] float Resist_Rate;
    [SerializeField] float Resist_TriggerDistance;
    [SerializeField][Range(0, 50)] int Resist_BlockScale;
    [SerializeField][Range(0, 50)] int Resist_DodgeScale;
    [SerializeField] ActionConfig StandUpAC;

    private void StartGeneral()
    {
        CombatManager.Instance.PlayerAttackEvent += Resist;
    }

    private void UpdateGeneral()
    {
        UpdateTimer();
        UpdateReserveTough();
    }

    private void UpdateTimer()
    {
        Resist_Timer.Update();
        Roam_Timer.Update();
    }

    private void Resist()
    {
        if (!character.IsLocked()) { return; }

        if (Vector3.Distance(character.GetTransform().position, character.GetLockTarget().GetTransform().position) > Resist_TriggerDistance) { return; }

        if (!EskiNottToolKit.MathLibrary.ProbabilityCalculate(Resist_Rate)) { return; }

        Debug.Log("Resist");

        float _waitfortime = Random.Range(0, character.GetLockTarget().GetActionControl().GetCurrentActionConfig().AnimationFixedTime);
        int _DefendOrDodge = UnityEngine.Random.Range(0, Resist_BlockScale + Resist_DodgeScale);
        if (_DefendOrDodge > Resist_DodgeScale)
        {
            //Do Resist
            Resist_Timer.TimerEnded += Resist_DoBlock;
        }
        else
        {
            //Do Defend
            Resist_Timer.TimerEnded += Resist_DoDodge;
        }

        Resist_Timer.Begin(_waitfortime);
    }

    private void Resist_DoBlock()
    {
        CombatManager.Instance.SendCommand(character, CombatManager.ActionCommand.Block);
        Resist_Timer.TimerEnded -= Resist_DoBlock;
    }

    private void Resist_DoDodge()
    {
        CombatManager.Instance.SendCommand(character, CombatManager.ActionCommand.Dodge);
        Resist_Timer.TimerEnded -= Resist_DoDodge;
    }

    private void UpdateReserveTough()
    {
        if (character.GetActionControl().GetCurrentActionConfig() == StandUpAC)
        {
            character.AddTough(character.MaxTough);
        }
    }

    #endregion

    #region FirstMet
    private void UpdateFirstMet()
    {
        if (!fsm.IsInState(0)) { return; }
    }

    #endregion

    #region Roam

    [Header("Roam")]
    [SerializeField] Timer Roam_Timer;
    float roamTime;
    Vector3 roamVector;
    [SerializeField] float Roam_Time_Min = 0f;
    [SerializeField] float Roam_Time_Max = 3f;
    [SerializeField] Vector3 Roam_DisplaceVector_Min;
    [SerializeField] Vector3 Roam_DisplaceVector_Max;
    [SerializeField] float Roam_TargetDistance;
    [SerializeField] int Roam_ExecuteCount_Min;
    [SerializeField][Range(0, 100)] float Roam_ExecuteRate_Roam;
    [SerializeField][Range(0, 100)] float Roam_ExecuteRate_Attack;
    [SerializeField] int Roam_ExecuteCount;
    private void StartRoam()
    {
        Roam_ExecutedCountReset();
    }

    private void UpdateRoam()
    {

        if (!fsm.IsInState(1)) { return; }

        if (actionControl.GetCurrentActionConfig().ActionCommandType == CombatManager.ActionCommand.GetHitHeavy) { return; }

        float _totalExecuteValue = Roam_ExecuteRate_Roam + Roam_ExecuteRate_Attack;

        float _executeValue = UnityEngine.Random.Range(0, _totalExecuteValue);

        if (!Roam_Timer.Running)
        {

            if (Roam_ExecuteCount < Roam_ExecuteCount_Min
                || _executeValue < Roam_ExecuteRate_Roam)
            {
                Roam_RunningInit();
                Roam_ExecuteCount++;
            }
            else if (_executeValue < Roam_ExecuteRate_Roam + Roam_ExecuteRate_Attack
            && character.GetLockTarget() != null)
            {
                Roam_ExecutedCountReset();
                fsm.Transition(2);
                return;
            }
        }

        if (character.GetLockTarget() != null) { return; }
        ((Enemy)character).TryLock();
    }

    private void Roam_ExecutedCountReset()
    {
        Roam_ExecuteCount = 0;
    }

    private void Roam_RunningInit()
    {
        roamTime = UnityEngine.Random.Range(Roam_Time_Min, Roam_Time_Max);
        float _x = UnityEngine.Random.Range(Roam_DisplaceVector_Min.x, Roam_DisplaceVector_Max.x);
        float _z;

        if (!character.IsLocked())
        {
            _z = UnityEngine.Random.Range(Roam_DisplaceVector_Min.z, Roam_DisplaceVector_Max.z);
        }
        else
        {
            if (Vector3.Distance(character.GetTransform().position, character.GetLockTarget().GetTransform().position)
             > Roam_TargetDistance)
            {
                _z = UnityEngine.Random.Range(0, Roam_DisplaceVector_Max.z);
            }
            else
            {
                _z = UnityEngine.Random.Range(Roam_DisplaceVector_Min.z, 0);
            }
        }

        roamVector = new Vector3(_x, 0, _z);
        Roam_Timer.Begin(roamTime, Timer.TimerMode.InstantStop);
        ((Enemy)character).MoveDistanceLocal(roamVector, Enemy.MovingType.Walk);
    }

    #endregion

    #region Attack

    [Header("Attack")]
    CombatActionConfig _cAC;

    int AttackTime;
    private void UpdateAttack()
    {
        if (!fsm.IsInState(2)) { return; }

        if (AttackTime == 0)
        {
            if (actionControl.GetCurrentActionConfig().ActionCommandType == CombatManager.ActionCommand.GetHitHeavy) { return; }
            _cAC = GetAttackActionByPriority();
            Vector3 _disVector = character.GetTransform().position - character.GetLockTarget().GetTransform().position;
            float _minTD = _cAC.AttackDistance;
            float _maxTD = GetMaxTriggerDistance(_cAC) + _cAC.AttackDistance;
            float _targetDistance = UnityEngine.Random.Range(_minTD, _maxTD);

            Enemy.MovingType _movingType = _disVector.magnitude > _targetDistance
            ? Enemy.MovingType.Run
            : _movingType = Enemy.MovingType.Walk;

            ((Enemy)character).MoveToPosition(character.GetLockTarget().GetTransform().position
                       + _disVector.normalized * _targetDistance, _movingType);

            ((Enemy)character).StopMovingEvents += PlayWhenMoved;
            AttackTime++;
        }
        else
        {
            if (actionControl.GetCurrentActionConfig().ACtype == ActionConfig.ACType.Combat
            && actionControl.GetCurrentActionConfig().DefaultNextAction.actionConfig.ACtype == ActionConfig.ACType.Combat)
            {
                CombatManager.Instance.SendCommand(character, CombatManager.ActionCommand.LightAttack);
            }
            if (actionControl.GetCurrentActionConfig().ActionCommandType == CombatManager.ActionCommand.None)
            {
                AttackTime = 0;
                fsm.Transition(1);
            }
        }
    }

    private void PlayWhenMoved()
    {
        actionControl.PlayAction(_cAC);
        ((Enemy)character).StopMovingEvents -= PlayWhenMoved;
    }

    #endregion

}
