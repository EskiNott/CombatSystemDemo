using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    [Header("对象引用")]
    [SerializeField] protected Transform LockPointTransform;
    [SerializeField] protected AnimationConfig animationConfig;

    [Header("数据监测")]
    [SerializeField] bool isLock;
    [SerializeField] protected Vector2 hitVector;
    [SerializeField] protected float MoveVelocity;
    [SerializeField] Character lockTarget;

    /// <summary>
    /// Local space input direction
    /// </summary>
    [SerializeField] Vector2 InputDirection;

    /// <summary>
    /// In world space move direction
    /// </summary>
    [SerializeField] Vector3 MoveDirection;

    [Header("旋转时间")]
    [SerializeField] protected float LockedRotateFixedTime = 0.1f;
    [SerializeField] protected float NonLockedRotateFixedTime = 0.2f;
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public float Health { get; private set; }
    [field: SerializeField] public float Courage { get; private set; }
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float MaxCourage { get; private set; }
    [field: SerializeField] public float Tough { get; private set; }
    [field: SerializeField] public float MaxTough { get; private set; }
    [field: SerializeField] public float LightHitlagTime { get; private set; }
    [field: SerializeField] public float MaxLockDistance { get; private set; }
    [field: SerializeField] public float MaxLockAngle { get; private set; }
    [field: SerializeField] public float CourageMinus { get; private set; }
    [field: SerializeField] public CharacterType CharType { get; protected set; }
    [field: SerializeField] protected bool debugViewing;
    [field: SerializeField] public Transform HitEffect { get; private set; }

    protected ActionControl actionControl;
    protected Rigidbody characterRigidbody;
    protected Transform thisTransform;
    protected Animator animator;
    protected AnimationControl animationControl;

    [Serializable]
    public enum CharacterType
    {
        Normal,
        Hero,
        Enemy
    }

    virtual protected void Awake()
    {
        characterRigidbody = GetComponent<Rigidbody>();
        thisTransform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        actionControl = GetComponent<ActionControl>();
        animationControl = GetComponent<AnimationControl>();
    }

    virtual protected void Start()
    {
        Health = MaxHealth;
        Tough = MaxTough;
        LightHitlagTime = 0;
    }

    virtual protected void Update()
    {
        UpdateVelocity();
        UpdateValueCheck();
        UpdateTargetLock();

        UpdateHitVector_Animator();
        UpdateMoveVector_Animator();
    }

    protected void UpdateValueCheck()
    {
        Health = Health > MaxHealth ? MaxHealth : Health;
        Courage = Courage > MaxCourage ? MaxCourage : Courage;
        Courage = Courage < 0 ? 0 : Courage;
        Tough = Tough > MaxTough ? MaxTough : Tough;
    }

    protected void UpdateVelocity()
    {
        MoveVelocity = new Vector3(characterRigidbody.velocity.x, 0, characterRigidbody.velocity.z).magnitude;
    }

    public float GetMoveSpeed()
    {
        return MoveVelocity;
    }

    public Vector3 GetLocalVelocity()
    {
        return thisTransform.InverseTransformVector(characterRigidbody.velocity);
    }

    public Vector3 GetVelocity()
    {
        return characterRigidbody.velocity;
    }

    public AnimationConfig GetAnimationConfig()
    {
        return animationConfig;
    }

    public ActionControl GetActionControl()
    {
        return actionControl;
    }

    public Transform GetLockPointTransform()
    {
        return LockPointTransform;
    }

    public Transform GetTransform()
    {
        return thisTransform;
    }

    public AnimationControl GetAnimationControl()
    {
        return animationControl;
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    virtual public void AddTough(float tough)
    {
        Tough += tough;
    }

    virtual public void AddCourage(float courage)
    {
        Courage += courage;
    }

    virtual public float AddHealth(float value)
    {
        float _lastHealth = Health;
        Health += value;
        return MaxHealth - _lastHealth;
    }

    public void AddLightHitlagTime()
    {
        LightHitlagTime++;
    }

    public void ResetLightHitlagTime()
    {
        LightHitlagTime = 0;
    }

    public void SetHitVector(float x, float y)
    {
        hitVector = new Vector2(x, y);
    }

    private void UpdateHitVector_Animator()
    {
        if (hitVector == null || (hitVector.x == 0 && hitVector.y == 0)) { return; }
        animator.SetFloat("GetHit_x", hitVector.x);
        animator.SetFloat("GetHit_y", hitVector.x);
    }

    private void UpdateMoveVector_Animator()
    {
        animator.SetFloat("xInput", GetLocalVelocity().z);
        animator.SetFloat("yInput", GetLocalVelocity().x);
    }

    public bool LockToTarget(Character target)
    {
        if (target == null) { return false; }
        lockTarget = target;
        isLock = true;
        return true;
    }

    public void Unlock()
    {
        lockTarget = null;
        isLock = false;
    }

    public Character GetLockTarget()
    {
        if (lockTarget == null) { return null; }
        return lockTarget;
    }

    public bool IsLocked()
    {
        return isLock;
    }

    private void UpdateTargetLock()
    {
        if (!IsLocked()) { return; }
    }

    public Quaternion InputRotationWithLook(Vector3 InputVector)
    {
        return InputVector != Vector3.zero ? Quaternion.LookRotation(InputVector) : Quaternion.identity;
    }

    virtual public void CharacterRotate(Quaternion LockDirection, Quaternion NonLockDirection)
    {
        if (IsLocked() && !(actionControl.GetCurrentActionConfig().ActionCommandType == CombatManager.ActionCommand.Run))
        {
            thisTransform.DORotateQuaternion(LockDirection, LockedRotateFixedTime);
        }
        else
        {
            if (GetInputDirection().x == 0 && GetInputDirection().y == 0) { return; }
            thisTransform.DORotateQuaternion(NonLockDirection, NonLockedRotateFixedTime);
        }
    }

    public Quaternion GetLookRotateQuaternion(Transform RotationInfoProviderTransfrom)
    {
        float _yRot = RotationInfoProviderTransfrom.rotation.eulerAngles.y;
        return Quaternion.Euler(0, _yRot, 0);
    }

    public Vector3 GetInputDirectionWithLook(Quaternion lookQuaternion)
    {
        return lookQuaternion * new Vector3(GetInputDirection().x, 0, GetInputDirection().y);
    }

    /// <summary>
    /// 被锁定目标相对 originPosition 的方向向量
    /// </summary>
    /// <param name="originPosition">相对目标坐标</param>
    /// <returns></returns>
    public Vector3 LockToPosDirection(Vector3 originPosition)
    {
        return (GetLockTarget().transform.position - originPosition).normalized;
    }

    public void SetInputDirection(Vector2 inputDirection)
    {
        InputDirection = inputDirection;
    }

    public void SetMoveDirection(Vector3 moveDirection)
    {
        MoveDirection = moveDirection;
    }

    public Vector2 GetInputDirection()
    {
        return InputDirection;
    }

    public Vector3 GetMoveDirection()
    {
        return MoveDirection;
    }

    public void PlayHitEffect(Vector3 Position, Quaternion Rotation)
    {
        HitEffect.gameObject.SetActive(true);
        HitEffect.position = Position;
        HitEffect.rotation = Rotation;
        HitEffect.GetComponent<ParticleSystem>().Play();
    }
}
