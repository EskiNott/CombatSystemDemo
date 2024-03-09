using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Enemy : Character
{
    [field: SerializeField] public float MoveSpeedThreshold { get; private set; }
    private bool isMoving;
    private CombatManager.ActionCommand moveCommand;
    private Vector3 TargetPositionInWorld;
    private AI CharacterAI;

    public Enemy()
    {
        CharType = CharacterType.Enemy;
    }

    [Serializable]
    public enum MovingType
    {
        Walk,
        Run
    }

    override protected void Awake()
    {
        base.Awake();
        CharacterAI = GetComponent<AI>();
    }

    protected override void Start()
    {
        base.Start();
        isMoving = false;
    }

    protected override void Update()
    {
        base.Update();
        UpdateCharacterMove();
        UpdateCharacterRotate();
        UpdateDrawViewingArea();
    }

    public override void AddTough(float tough)
    {
        base.AddTough(tough);
        UIManager.Instance.UpdateUI_Enemy();
    }

    public override float AddHealth(float value)
    {
        float _health = base.AddHealth(value);
        UIManager.Instance.UpdateUI_Enemy();
        UIManager.Instance.UpdateUI_Hit();
        return _health;
    }

    private void UpdateCharacterRotate()
    {
        //Quaternion LookRotateByTarget = GetLookRotateQuaternion(GetLockTarget().GetTransform());
        Quaternion _lockRotation = Quaternion.identity;
        if (IsLocked() && GetLockTarget() != null)
        {
            Quaternion _tempLookQua = Quaternion.LookRotation(GetLockTarget().GetTransform().position - thisTransform.position);
            _lockRotation = Quaternion.Euler(0, _tempLookQua.eulerAngles.y, 0);
        }
        //Vector3 InputDirection = GetInputDirectionWithLook(LookRotateByTarget);
        Vector3 _lookVec = (TargetPositionInWorld - thisTransform.position).normalized;
        Quaternion _nonLockRotation = Quaternion.identity;
        if (_lookVec != Vector3.zero)
        {
            _nonLockRotation = Quaternion.LookRotation(_lookVec);
        }
        CharacterRotate(_lockRotation, _nonLockRotation);
    }

    private void StartMoving()
    {
        isMoving = true;
    }

    public Action StopMovingEvents;

    private void StopMoving()
    {
        SetMoveDirection(Vector3.zero);
        CombatManager.Instance.SendCommand(this, CombatManager.ActionCommand.None);
        isMoving = false;
        StopMovingEvents?.Invoke();
    }

    private void UpdateCharacterMove()
    {
        if (!isMoving) { return; }
        //For enemy rotate to move direction
        SetInputDirection((TargetPositionInWorld - thisTransform.position).normalized);

        //Move World Space Coordinate
        SetMoveDirection((TargetPositionInWorld - thisTransform.position).normalized);

        CombatManager.Instance.SendCommand(this, moveCommand);

        if (Mathf.Abs(TargetPositionInWorld.x - thisTransform.position.x) < 0.3f
        && Mathf.Abs(TargetPositionInWorld.z - thisTransform.position.z) < 0.3f)
        {
            StopMoving();
        }
    }

    public Vector3 GetRotateTargetVector()
    {
        return thisTransform.rotation * new Vector3(GetInputDirection().x, 0, GetInputDirection().y);
    }

    public void MoveToPosition(Vector3 targetPosition, MovingType moveType)
    {
        TargetPositionInWorld = targetPosition;
        moveCommand = moveType == MovingType.Walk ? CombatManager.ActionCommand.Walk : CombatManager.ActionCommand.Run;
        StartMoving();
    }

    public void MoveDistance(Vector3 distance, MovingType moveType)
    {
        Vector3 _dis = new Vector3(distance.x, 0, distance.z);
        Vector3 _targetPositionInWorld = thisTransform.position + _dis;
        MoveToPosition(_targetPositionInWorld, moveType);
    }

    public void MoveDistanceLocal(Vector3 LocalDistance, MovingType moveType)
    {
        MoveDistance(thisTransform.TransformVector(LocalDistance), moveType);
    }

    public bool TryLock()
    {
        Character _lockTarget = GetNearestVisibleCharacter();
        if (_lockTarget == null) { return false; }
        LockToTarget(_lockTarget);
        return true;
    }

    override public void CharacterRotate(Quaternion LockDirection, Quaternion NonLockDirection)
    {
        CombatManager.ActionCommand _ac = actionControl.GetCurrentActionConfig().ActionCommandType;
        if (!(_ac == CombatManager.ActionCommand.Walk
        || _ac == CombatManager.ActionCommand.Dodge
        || _ac == CombatManager.ActionCommand.Run)) { return; }
        base.CharacterRotate(LockDirection, NonLockDirection);
    }

    /// <summary>
    /// 通过碰撞检测得到视野内对象
    /// </summary>
    /// <returns></returns>
    private Character GetNearestVisibleCharacter()
    {
        Character _result = null;
        List<Collider> _colliders = new(Physics.OverlapSphere(thisTransform.position, MaxLockDistance));
        float _minDistance = Mathf.Infinity;
        foreach (var item in _colliders)
        {
            if (!item.gameObject.CompareTag("Character")) { continue; }
            if (item.gameObject.name == "Enemy") { continue; }
            if (!IsWithinFanShapeInfiniteArea(item.gameObject)) { continue; }

            float _dis = Vector3.Distance(item.transform.position, thisTransform.position);
            if (_dis >= _minDistance) { continue; }
            _minDistance = _dis;
            _result = item.GetComponent<Character>();
        }
        return _result;
    }

    /// <summary>
    /// 取决于参数 MaxLockAngle 的无限扇形区域
    /// </summary>
    /// <param name="target"></param>
    /// <returns>是否在视野内</returns>
    private bool IsWithinFanShapeInfiniteArea(GameObject target)
    {
        // 扇形区域的半径已经在上级函数中得到，所以无需再算一次
        // float _distance = Vector3.Distance(target.transform.position, thisTransform.position);
        // if (!(_distance < MaxLockDistance)) { return false; }

        Vector3 _thisToTargetVec = target.transform.position - thisTransform.position;
        float _angle = Mathf.Abs(Vector3.Angle(thisTransform.forward, _thisToTargetVec));
        if (!(_angle < MaxLockAngle / 2)) { return false; }

        return true;
    }

    private void UpdateDrawViewingArea()
    {
        if (!debugViewing) { return; }
        int _totalNodeCount = (int)(MaxLockAngle / 15);
        float _anglePerNode = MaxLockAngle / _totalNodeCount;
        float _angleProgress = -MaxLockAngle / 2;

        DrawRadius(_angleProgress);

        for (int index = 0; index < _totalNodeCount; index++)
        {
            DrawArc(_angleProgress);
            _angleProgress += _anglePerNode;
        }

        DrawRadius(_angleProgress);

        void DrawRadius(float progress)
        {
            Debug.DrawLine(thisTransform.position, thisTransform.position + Quaternion.AngleAxis(progress, thisTransform.up) * thisTransform.forward * MaxLockDistance);
        }

        void DrawArc(float progress)
        {
            Debug.DrawLine(thisTransform.position + Quaternion.AngleAxis(progress, thisTransform.up) * thisTransform.forward * MaxLockDistance
            , thisTransform.position + Quaternion.AngleAxis(progress + _anglePerNode, thisTransform.up) * thisTransform.forward * MaxLockDistance);
        }
    }
}
