using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EskiNottToolKit.MonoSingleton<PlayerController>
{
    [Header("对象引用")]
    [SerializeField] Hero hero;
    [SerializeField] Transform playerTrans;
    [SerializeField] Transform characterCameraTrans;
    [SerializeField] CinemachineVirtualCamera characterCamera;
    [SerializeField] CinemachineVirtualCamera characterExecuteCamera;
    [SerializeField] Camera mainCamera;
    ActionControl heroActionControl;

    [Header("数据监测")]
    [SerializeField] Vector2 lookInputVector;
    [SerializeField] float xRotation = 0f;
    [SerializeField] float yRotation = 0f;
    [SerializeField] InputDeviceType deviceType = InputDeviceType.None;
    [SerializeField] bool isSprint;
    [SerializeField] bool isLightAttack;
    [SerializeField] bool isHeavyAttack;
    [SerializeField] bool isBlock;
    [SerializeField] bool isDodge;
    [SerializeField] bool isExecute;
    [SerializeField] bool isCourage;

    [Header("调整项")]
    [SerializeField] float mouseSensitivity;
    [SerializeField] float padSensitivity;

    [SerializeField] float yMaxAngle;
    [SerializeField] float yMinAngle;
    [SerializeField] float yLockMaxAngle;
    [SerializeField] float yLockMinAngle;
    [SerializeField] bool CursorLock = true;
    [SerializeField] bool CursorHide = true;

    public PlayerInputControl PlayerInput;

    [SerializeField]
    public enum InputDeviceType
    {
        Keyboard,
        Gamepad,
        None
    }

    protected override void Awake()
    {
        base.Awake();
        PlayerInput = new PlayerInputControl();
        heroActionControl = hero.GetComponent<ActionControl>();
        InputEventRegister_Awake();
    }

    private void OnEnable()
    {
        PlayerInput.Enable();
    }

    private void OnDisable()
    {
        PlayerInput.Disable();
    }

    private void Update()
    {
        InputDetect();
        CameraControl();
        MouseControl();
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void InputDetect()
    {
        hero.SetInputDirection(PlayerInput.Player.Move.ReadValue<Vector2>());
        lookInputVector = PlayerInput.Player.Look.ReadValue<Vector2>();

    }

    private void PlayerMovement()
    {
        PlayMoveAnimation();

        hero.SetMoveDirection
        (
            hero.GetInputDirectionWithLook(hero.GetLookRotateQuaternion(characterCameraTrans))
            );

        void PlayMoveAnimation()
        {
            if (!(hero.GetMoveDirection().x == 0 && hero.GetMoveDirection().y == 0))
            {
                if (isSprint)
                {
                    //hero.GetActionConfig().PlayAction(CombatManager.ActionCommand.Run);
                    CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.Run);
                }
                else
                {
                    //hero.GetActionConfig().PlayAction(CombatManager.ActionCommand.Walk);
                    CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.Walk);
                }
            }
            else
            {
                CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.None);
            }
        }

    }

    private void CameraControl()
    {
        if (hero.IsLocked())
        {
            Vector3 _camDir = hero.GetLockTarget().transform.position - characterCameraTrans.position;

            Quaternion _camRot = Quaternion.LookRotation(_camDir);

            // float _ang = Quaternion.Angle(_camRot, hero.GetTransform().rotation);

            // if (-_ang > yMaxAngle || -_ang < yMinAngle)
            // {
            //     hero.Unlock();
            //     UIManager.Instance.LockPointEnable(false);
            // }

            float _xR = CheckAngle(_camRot.eulerAngles.x);

            _xR = Mathf.Clamp(_xR, yLockMinAngle, yLockMaxAngle);

            //float _xR = _camRot.eulerAngles.x;

            Quaternion _newCamRot = Quaternion.Euler(_xR, _camRot.eulerAngles.y, _camRot.eulerAngles.z);

            characterCameraTrans.DORotateQuaternion(_newCamRot, 0.1f);

            xRotation = -characterCameraTrans.eulerAngles.y;
            yRotation = -characterCameraTrans.eulerAngles.x;
        }
        else
        {
            xRotation -= lookInputVector.x * Time.deltaTime * mouseSensitivity;
            yRotation += lookInputVector.y * Time.deltaTime * mouseSensitivity;

            xRotation = xRotation > 360 ? xRotation - 360 : xRotation;
            xRotation = xRotation < -360 ? xRotation + 360 : xRotation;

            yRotation = Mathf.Clamp(yRotation, yMinAngle, yMaxAngle);
            Quaternion _rot = Quaternion.Euler(-yRotation, -xRotation, 0);
            characterCameraTrans.DORotateQuaternion(_rot, 0.1f);
        }
    }

    public float CheckAngle(float value)
    {
        value -= 180;

        if (value < 0)
        {
            return value + 180;
        }
        else
        {
            return value - 180;
        }
    }

    private void MouseControl()
    {
        Cursor.lockState = CursorLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !CursorHide;
    }

    #region Input Events
    private void InputEventRegister_Awake()
    {
        PlayerInput.Player.Lock.started += OnLockPress;
        PlayerInput.Player.Sprint.started += OnSprintStart;
        PlayerInput.Player.Sprint.canceled += OnSprintCancel;
        PlayerInput.Player.LightAttack.started += OnLightAttackStart;
        PlayerInput.Player.LightAttack.canceled += OnLightAttackCancel;
        PlayerInput.Player.HeavyAttack.started += OnHeavyAttackStart;
        PlayerInput.Player.HeavyAttack.canceled += OnHeavyAttackCancel;
        PlayerInput.Player.Block.started += OnBlockStart;
        PlayerInput.Player.Block.canceled += OnBlockCancel;
        PlayerInput.Player.Dodge.started += OnDodgeStart;
        PlayerInput.Player.Dodge.canceled += OnDodgeCancel;
        PlayerInput.Player.Execute.started += OnExecuteStart;
        PlayerInput.Player.Execute.canceled += OnExecuteCancel;
        PlayerInput.Player.Courage.started += OnCourageStart;
        PlayerInput.Player.Courage.canceled += OnCourageCancel;

        PlayerInput.UI.ConsoleControl.started += OnConsoleStart;
    }
    private void OnSprintStart(InputAction.CallbackContext obj)
    {
        isSprint = true;
    }
    private void OnSprintCancel(InputAction.CallbackContext obj)
    {
        isSprint = false;
    }

    private void OnLightAttackStart(InputAction.CallbackContext obj)
    {
        CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.LightAttack);
        isLightAttack = true;
        CombatManager.Instance.PlayerAttackEvent?.Invoke();
    }
    private void OnLightAttackCancel(InputAction.CallbackContext obj)
    {
        isLightAttack = false;
    }

    private void OnHeavyAttackStart(InputAction.CallbackContext obj)
    {
        CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.HeavyAttack);
        isHeavyAttack = true;
        CombatManager.Instance.PlayerAttackEvent?.Invoke();
    }
    private void OnHeavyAttackCancel(InputAction.CallbackContext obj)
    {
        isHeavyAttack = false;
    }

    private void OnBlockStart(InputAction.CallbackContext obj)
    {
        CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.Block);
        isBlock = true;
    }
    private void OnBlockCancel(InputAction.CallbackContext obj)
    {
        isBlock = false;
    }

    private void OnDodgeStart(InputAction.CallbackContext obj)
    {
        CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.Dodge);
        isDodge = true;
    }
    private void OnDodgeCancel(InputAction.CallbackContext obj)
    {
        isDodge = false;
    }

    private void OnExecuteStart(InputAction.CallbackContext obj)
    {
        CombatManager.Instance.SendCommand(hero, CombatManager.ActionCommand.Execute);
        isExecute = true;
    }
    private void OnExecuteCancel(InputAction.CallbackContext obj)
    {
        isExecute = false;
    }

    private void OnCourageStart(InputAction.CallbackContext obj)
    {
        heroActionControl.CourageOn();
        isCourage = true;
    }
    private void OnCourageCancel(InputAction.CallbackContext obj)
    {
        heroActionControl.CourageOff();
        isCourage = false;
    }

    private void OnLockPress(InputAction.CallbackContext obj)
    {
        if (hero.IsLocked())
        {
            hero.Unlock();

            UIManager.Instance.LockPointEnable(false);
            UIManager.Instance.EnemySituationEnable(false);
        }
        else
        {
            if (!hero.LockToTarget(GetNearestVisibleCharacter()))
            {
                hero.Unlock();
                UIManager.Instance.LockPointEnable(false);
                UIManager.Instance.EnemySituationEnable(false);
            }
            else
            {
                UIManager.Instance.LockPointEnable(true);
                UIManager.Instance.EnemySituationEnable(true);
            }
        }
    }

    private void OnConsoleStart(InputAction.CallbackContext obj)
    {
        DebugConsole.Instance.SwitchConsoleShow();
    }
    #endregion

    public Transform GetCameraTrans()
    {
        return characterCameraTrans;
    }

    private Character GetNearestVisibleCharacter()
    {
        Character _result = null;
        List<Collider> _colliders = new(Physics.OverlapSphere(playerTrans.position, hero.MaxLockDistance));
        float _minDistance = Mathf.Infinity;
        foreach (var item in _colliders)
        {
            if (!item.gameObject.CompareTag("Character")) { continue; }
            if (!UIManager.IsCameraVisible(mainCamera, item.transform)) { continue; }
            float _dis = Vector3.Distance(item.transform.position, playerTrans.position);
            if (_dis >= _minDistance) { continue; }
            if (!item.TryGetComponent<CharacterCollider>(out var _cc)) { continue; }
            if (_cc.GetCharacter().CharType == Character.CharacterType.Hero) { continue; }
            _result = _cc.GetCharacter();
            _minDistance = _dis;
        }
        return _result;
    }

}
