using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hero : Character
{

    [field: SerializeField] public float HitCount { get; private set; }
    [field: SerializeField] public float HitCountExpiredTime { get; private set; }
    [field: SerializeField] public float HitCountDecreaseSpeed { get; private set; }
    [field: SerializeField] public Timer HitCountTimer { get; private set; }
    [field: SerializeField] private bool hitCountDecreasing;
    public Hero()
    {
        CharType = CharacterType.Hero;
    }

    protected override void Awake()
    {
        base.Awake();
        HitCountTimer = new();
    }

    protected override void Start()
    {
        base.Start();
        hitCountDecreasing = false;
        HitCountTimer.TimerEnded += StartHitCountDecrease;
    }

    protected override void Update()
    {
        base.Update();
        UpdateCharacterRotate();
        UpdateHitCountDecrease();
        HitCountTimer.Update();
    }

    public override void AddCourage(float courage)
    {
        base.AddCourage(courage);
        UIManager.Instance.UpdateUI_Player();
    }

    public override float AddHealth(float value)
    {
        float _health = base.AddHealth(value);
        UIManager.Instance.UpdateUI_Player();
        return _health;
    }

    private void UpdateCharacterRotate()
    {
        Quaternion _lookRotateByCamera = GetLookRotateQuaternion(PlayerController.Instance.GetCameraTrans());
        Vector3 _inputDirectionWithLook = GetInputDirectionWithLook(_lookRotateByCamera);
        Quaternion _inputRotation = InputRotationWithLook(_inputDirectionWithLook);
        CharacterRotate(_lookRotateByCamera, _inputRotation);
    }

    private void UpdateHitCountDecrease()
    {
        if (!hitCountDecreasing) { return; }
        HitCount -= HitCountDecreaseSpeed * Time.deltaTime;
        HitCount = HitCount < 0 ? 0 : HitCount;
    }

    public void StartHitCountDecrease()
    {
        hitCountDecreasing = true;
    }

    public void StopHitCountDecrease()
    {
        hitCountDecreasing = false;
        HitCountTimer.Begin(HitCountExpiredTime, Timer.TimerMode.InstantStop);
    }

    public void AddHitCount(float amount)
    {
        HitCount += amount;
    }

}
