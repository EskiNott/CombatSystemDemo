using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.VFX;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using Unity.Mathematics;

[RequireComponent(typeof(Character))]
public class ActionControl : MonoBehaviour
{
    [Header("数据监测")]
    [SerializeField] ActionConfig currentAction;
    [SerializeField] List<CombatManager.ActionCommand> nextActionCommandTable;
    [SerializeField] bool invincible;
    [field: SerializeField] public Timer ActionTimer { get; private set; }
    [field: SerializeField] public bool IsCouraging { get; private set; }
    [SerializeField] ProgressTracker<ActionConfig.ActionMove> moveTracker;
    [SerializeField] ProgressTracker<ActionConfig.Particle> particleTracker;
    [field: SerializeField] public bool CanExecuteLockTarget { get; private set; }

    [Header("引用")]
    [SerializeField] ActionConfig IdleAction;
    [SerializeField] List<CharacterCollider> DodgeColliders;
    [SerializeField] List<WeaponCollider> AttackColliders;
    [SerializeField] List<BlockCollider> BlockColliders;
    [SerializeField] List<Transform> ParticleTransforms;

    [Header("修改项")]
    [SerializeField] float moveSpeedMultiplier = 3;
    [SerializeField] float sprintSpeedMultiplier = 6;
    [SerializeField] float dodgeForceMultiplier = 10;
    [SerializeField] float hitInvincibleTime;
    WaitForSeconds hitInvincibleWait;
    bool tableUpdated;
    Character character;
    bool isSprint;
    Rigidbody characterRigidbody;
    Vector3 getHitVector;
    AnimationControl animationControl;
    //[SerializeField] List<bool> actionTimer_MoveExecutedList;

    private void Awake()
    {
        character = GetComponent<Character>();
        characterRigidbody = GetComponent<Rigidbody>();
        animationControl = GetComponent<AnimationControl>();
        hitInvincibleWait = new WaitForSeconds(hitInvincibleTime);
        //actionTimer_MoveExecutedList = new();
        moveTracker = new();
        particleTracker = new();
    }

    private void Start()
    {
        ActionTimer = new();
        StuckFrameTimer = new();
        ActionTimer.Reset();
        ActionTimer.TimerStarted += ActionTimerRegister_Started;
        ActionTimer.TimerRunning += ActionTimerRegister_Running;
        StuckFrameTimer.TimerEnded += StuckFrameOnStop;
        if (currentAction == null)
        {
            PlayAction(IdleAction);
        }
    }

    virtual protected void Update()
    {
        UpdateDealActionCommand();
        UpdateCourage();
        UpdateCanExecuteTargetCheck();
        ActionTimer.Update();
        StuckFrameTimer.Update();
    }

    virtual protected void FixedUpdate()
    {
        UpdateCharacterMovement();
    }

    private void UpdateDealActionCommand()
    {
        if (nextActionCommandTable.Count <= 0 && !ActionTimer.IsEnd()) { return; }

        if (nextActionCommandTable.Count > 0)
        {
            if (tableUpdated)
            {
                foreach (var _targetItem in nextActionCommandTable)
                {
                    int _index = currentAction.NextActionSequence.FindIndex(x => x.Command == _targetItem);
                    if (_index == -1) { continue; }
                    if (!ActionTimer.IsEnd() && !currentAction.NextActionSequence[_index].IsBreakable) { continue; }
                    if (_targetItem == CombatManager.ActionCommand.Execute)
                    {
                        if (!CanExecuteLockTarget) { continue; }
                        Execute();
                    }
                    PlayAction(currentAction.NextActionSequence[_index].actionConfig);

                    ClearNextActionCommandTable();

                    break;
                }
            }

            if (ActionTimer.IsEnd())
            {
                ClearNextActionCommandTable();
                PlayAction(currentAction.DefaultNextAction.actionConfig);
            }
        }
        else if (ActionTimer.IsEnd())
        {
            PlayAction(currentAction.DefaultNextAction.actionConfig);
        }
    }

    private void Execute()
    {
        //Do something
        Transform _c = character.GetTransform();
        Transform _t = character.GetLockTarget().GetTransform();

        Vector3 _dir = _t.position - _c.position;

        Quaternion _look_thisToTarget = Quaternion.LookRotation(_dir, _c.up);
        Quaternion _look_targetToThis = Quaternion.LookRotation(-_dir, _t.up);

        _c.DORotateQuaternion(_look_thisToTarget, 0);
        _t.DORotateQuaternion(_look_targetToThis, 0);
        //_c.position = _t.position + _t.forward * 1f + _t.right * 0.5f;

        if (character.CharType != Character.CharacterType.Hero) { return; }
        //Camera Do Something
        CameraManager.Instance.DoExecuteCamera();
    }

    public void PlayAction(ActionConfig actionConfig)
    {
        if (actionConfig.CollideControl == ActionConfig.CollideControlType.Attack)
        {
            AttackResourceReturn();
        }
        SetCurrentActionConfig(actionConfig);
        animationControl.ActionConfigUpdated();
        ColliderReset();
        ActionTimer.Begin(actionConfig.AnimationFixedTime, Timer.TimerMode.Continuous);
        if (character.GetLockTarget() != null)
        {
            //Debug.Log(Vector3.Distance(character.GetTransform().position, character.GetLockTarget().GetTransform().position));
        }
        if (actionConfig.ActionCommandType == CombatManager.ActionCommand.Dodge)
        {
            Vector3 _moveDir = character.GetMoveDirection();
            Vector3 _dir = _moveDir.x == 0 && _moveDir.y == 0 ? -transform.forward : _moveDir.normalized;
            characterRigidbody.AddForce(_dir * dodgeForceMultiplier, ForceMode.Impulse);
        }
    }

    private void ColliderReset()
    {
        foreach (var item in DodgeColliders)
        {
            item.SetColliderEnable(true);
        }
        foreach (var item in AttackColliders)
        {
            item.SetColliderEnable(false);
        }
        foreach (var item in BlockColliders)
        {
            item.SetColliderEnable(false);
        }
    }

    public void AttackResourceReturn()
    {
        foreach (var item in AttackColliders)
        {
            item.ReturnResource();
        }
    }

    public void AttackResourceReset()
    {
        foreach (var item in AttackColliders)
        {
            item.TakeResource();
        }
    }

    public ActionConfig GetCurrentActionConfig()
    {
        return currentAction;
    }

    public void SetCurrentActionConfig(ActionConfig action)
    {
        currentAction = action;
    }

    #region Timer
    public bool CanReceiveInCommandWindow(CombatManager.ActionCommand nextCommand)
    {
        CombatManager.ActionCommand _curACType = GetCurrentActionConfig().ActionCommandType;

        bool _commandWindow =
        _curACType == CombatManager.ActionCommand.HeavyAttack
        && nextCommand == CombatManager.ActionCommand.HeavyAttack
        ? ActionTimer.Progress() >= 0.4f
        : ActionTimer.Progress() >= 0;

        return ActionTimer.Running && _commandWindow;
    }

    private void ActionTimer_ColliderControl()
    {
        if (!currentAction.IsNeededCollideTime) { return; }
        List<CharacterCollider> _interactedColliders = GetCurrentActionControlledCollides();
        if (_interactedColliders == null) { return; }

        bool _whitelistCondition = ActionTimer.Progress() > currentAction.CollideNormalizedTime1
                        && ActionTimer.Progress() < currentAction.CollideNormalizedTime2;

        bool _enable = currentAction.collideControlMethod == ActionConfig.CollideControlMethod.Whitelist ? _whitelistCondition : !_whitelistCondition;

        foreach (var item in _interactedColliders)
        {
            item.SetColliderEnable(_enable);

        }
    }

    private void ActionTimerRegister_Started()
    {
        moveTracker.Initialize(currentAction.actionMoves, MoveRequirement, MoveAction);
        particleTracker.Initialize(currentAction.particles, ParticleRequirement, ParticleAction);
    }

    private void ActionTimerRegister_Running()
    {
        ActionTimer_ColliderControl();
        moveTracker.Update();
        particleTracker.Update();
    }

    private bool MoveRequirement(ActionConfig.ActionMove actionMove)
    {
        return ActionTimer.ReachProgress(actionMove.FixedTime);
    }

    private void MoveAction(ActionConfig.ActionMove actionMove)
    {
        if (actionMove.coordType == ActionConfig.ActionMove.MoveCoordType.Local)
        {
            DisplaceLocal(actionMove.MoveVector, actionMove.Duration, actionMove.MoveEase);
        }
        else
        {
            Displace(actionMove.MoveVector, actionMove.Duration, actionMove.MoveEase);
        }
    }

    private bool ParticleRequirement(ActionConfig.Particle particle)
    {
        return ActionTimer.ReachProgress(particle.FixedTime);
    }

    private void ParticleAction(ActionConfig.Particle particle)
    {
        Transform _t = ParticleTransforms[particle.ParticleIndex];
        _t.SetLocalPositionAndRotation
        (particle.LocalPositionVector, Quaternion.Euler(particle.LocalRotationVector));
        _t.localScale = particle.LocalScaleVector;
        _t.gameObject.SetActive(true);

        if (particle.Type == ActionConfig.ParticleType.ParticleSystem)
        {
            ParticleSystem _ps = _t.GetComponent<ParticleSystem>();
            _ps.Play();
        }
        else if (particle.Type == ActionConfig.ParticleType.VisualEffect)
        {
            VisualEffect _ve = _t.GetComponent<VisualEffect>();
            _ve.Play();
        }

    }

    #endregion

    private List<CharacterCollider> GetCurrentActionControlledCollides()
    {
        if (currentAction.CollideControl == ActionConfig.CollideControlType.Attack)
        {
            return new List<CharacterCollider>(AttackColliders);
        }
        else if (currentAction.CollideControl == ActionConfig.CollideControlType.Dodge)
        {
            return DodgeColliders;
        }
        else if (currentAction.CollideControl == ActionConfig.CollideControlType.Block)
        {
            return new List<CharacterCollider>(BlockColliders);
        }
        else
        {
            return null;
        }
    }

    public List<CombatManager.ActionCommand> GetNextActionCommandTable()
    {
        return nextActionCommandTable;
    }

    public void SendActionCommand(CombatManager.ActionCommand newCommand)
    {
        //if (!CanReceiveInCommandWindow()) { return; }

        if (!CanReceiveInCommandWindow(newCommand)) { return; }

        int index = nextActionCommandTable.FindIndex(x => x == newCommand);

        if (index != -1 && index != 0)
        {
            if (!(newCommand == CombatManager.ActionCommand.Run ||
            newCommand == CombatManager.ActionCommand.Walk ||
            newCommand == CombatManager.ActionCommand.None))
            {
                nextActionCommandTable.RemoveAt(index);
                nextActionCommandTable.Insert(0, newCommand);
                tableUpdated = true;
            }
            //Debug.Log(newCommand.ToString() + "sent");
        }
        else if (index == -1)
        {
            nextActionCommandTable.Insert(0, newCommand);
            tableUpdated = true;
            //Debug.Log(newCommand.ToString() + "sent");
        }

    }

    public void ClearNextActionCommandTable()
    {
        nextActionCommandTable.Clear();
        tableUpdated = false;
    }

    bool isVelocityReset = false;

    public void UpdateCharacterMovement()
    {
        Vector3 _moveDir = character.GetMoveDirection().normalized;
        if (_moveDir == null) { return; }
        if (_moveDir.x != 0 || _moveDir.z != 0)
        {
            if (currentAction.ActionCommandType == CombatManager.ActionCommand.Run)
            {
                characterRigidbody.velocity = new Vector3
                (_moveDir.x * sprintSpeedMultiplier
                , characterRigidbody.velocity.y
                , _moveDir.z * sprintSpeedMultiplier);
            }
            else if (currentAction.ActionCommandType == CombatManager.ActionCommand.Walk)
            {
                characterRigidbody.velocity = new Vector3
                (_moveDir.x * moveSpeedMultiplier
                , characterRigidbody.velocity.y
                , _moveDir.z * moveSpeedMultiplier);
            }
            isVelocityReset = false;
        }
        else
        {
            if (!isVelocityReset)
            {
                characterRigidbody.velocity = new Vector3
                (0
                , characterRigidbody.velocity.y
                , 0);
                isVelocityReset = true;
            }

        }
    }

    public bool IsSprint()
    {
        return isSprint;
    }

    public Vector3 GetHitVector()
    {
        return getHitVector;
    }

    public void Displace(Vector3 targetPosition, float duration, Ease ease = Ease.Linear)
    {
        characterRigidbody.DOMove(targetPosition, duration).SetEase(ease);
    }

    public void DisplaceLocal(Vector3 localTargetPosition, float duration, Ease ease = Ease.Linear)
    {
        Displace(character.GetTransform().TransformPoint(localTargetPosition), duration, ease);
    }

    public Timer StuckFrameTimer;

    public void StuckFrame(float FixedTime)
    {
        animationControl.Pause(true);
        ActionTimer.Pause();
        StuckFrameTimer.Begin(FixedTime, Timer.TimerMode.InstantStop);
    }

    private void StuckFrameOnStop()
    {
        animationControl.Pause(false);
        ActionTimer.Play();
    }

    public void CourageOn()
    {
        if (character.Courage <= 1)
        {
            IsCouraging = false;
            return;
        }
        IsCouraging = true;
    }

    public void CourageOff()
    {
        IsCouraging = false;
    }

    private void UpdateCourage()
    {
        if (!IsCouraging) { return; }
        character.AddCourage(-character.CourageMinus * Time.deltaTime);
    }

    private void UpdateCanExecuteTargetCheck()
    {
        if (!character.IsLocked() || character.GetLockTarget() == null)
        {
            CanExecuteLockTarget = false;
            return;
        }

        Character _target = character.GetLockTarget();
        ActionControl _targetAC = _target.GetActionControl();

        if (_targetAC.currentAction.ActionCommandType != CombatManager.ActionCommand.GetHitLight)
        {
            CanExecuteLockTarget = false;
            return;
        }

        if (Vector3.Distance(_target.GetTransform().position, character.GetTransform().position)
        > (character.ExecuteDistance + _target.ExecuteDistance))
        {
            CanExecuteLockTarget = false;
            return;
        }

        CanExecuteLockTarget = true;
        Debug.Log("Can Execute");
    }
}
