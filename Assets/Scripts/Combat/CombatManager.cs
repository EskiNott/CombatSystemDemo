using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatManager : EskiNottToolKit.MonoSingleton<CombatManager>
{

    [Header("数据监测")]
    [SerializeField] List<Character> CombatCharacters;
    [SerializeField] Queue<AttackModel> attackModels;
    [SerializeField] public Action PlayerAttackEvent;
    [SerializeField] private float ShakeScreenPowerDamageRate_Player;
    [SerializeField] private float ShakeScreenPowerDamageRate_Enemy;
    [Serializable]
    public enum ActionCommand
    {
        Block,
        Dodge,
        Execute,
        LightAttack,
        HeavyAttack,
        Run,
        Walk,
        None,
        GetHitLight,
        GetHitHeavy
    }

    [Serializable]
    public struct AttackModel
    {
        public Character Host;
        public Character Target;
        public Vector3 HitDirection;
        public Vector3 HitPointWorld;
        public bool isCourage;
        public bool isBlock;
    }

    private void Start()
    {
        attackModels = new();
    }

    public void SendHitEvent(AttackModel attackModel)
    {
        attackModels.Enqueue(attackModel);
    }

    public void DealHitEvent()
    {
        if (attackModels.Count <= 0) { return; }
        AttackModel _am = attackModels.Dequeue();
        ActionConfig _hostAC = _am.Host.GetActionControl().GetCurrentActionConfig();
        ActionConfig _targetAC = _am.Target.GetActionControl().GetCurrentActionConfig();

        _am.Host.GetActionControl().AttackResourceReset();

        Debug.Log("Hit Event");

        if (_am.isCourage)
        {

        }
        else
        {
            if (_am.isBlock)
            {
                Debug.Log("Attack Block");
                if (_hostAC.ACtype == ActionConfig.ACType.Combat)
                {
                    CombatActionConfig _hostCAC = (CombatActionConfig)_hostAC;

                    float _stuckTime = _hostCAC.AutoStuckFrame
                    ? _hostCAC.AnimationFixedTime * 0.25f
                    : _hostCAC.StuckFrameTime;
                    _am.Host.GetActionControl().StuckFrame(_stuckTime);
                    _am.Target.GetActionControl().StuckFrame(_stuckTime);

                    SendCommand(_am.Host, ActionCommand.GetHitLight);
                }
            }
            else
            {
                Debug.Log("Attack NotBlock");
                if (_hostAC.ACtype == ActionConfig.ACType.Combat)
                {
                    Debug.Log("Attack Hit : " + _am.Host + " | " + _am.Target);
                    CombatActionConfig _hostCAC = (CombatActionConfig)_hostAC;

                    //DataUpdate
                    _am.Target.AddHealth(-_hostCAC.Damage);
                    _am.Target.AddTough(-_hostCAC.ToughCutting);
                    _am.Host.AddCourage(_hostCAC.NormalCourageGain);


                    //DealTough
                    if (_am.Target.Tough <= 0)
                    {
                        SendCommand(_am.Target, ActionCommand.GetHitHeavy);
                        _am.Target.ResetLightHitlagTime();
                        if (_am.Target.CharType != Character.CharacterType.Hero)
                        {
                            UIManager.Instance.ShowHardBreakText();
                        }
                    }
                    else if (_am.Target.Tough / _am.Target.MaxTough <= (1 - (_am.Target.LightHitlagTime + 1) * 0.2f))
                    {
                        _am.Target.SetHitVector(_am.HitDirection.x, _am.HitDirection.y);
                        SendCommand(_am.Target, ActionCommand.GetHitLight);
                        _am.Target.AddLightHitlagTime();
                        if (_am.Target.CharType != Character.CharacterType.Hero)
                        {
                            UIManager.Instance.ShowBreakText();
                        }
                    }

                    //StuckFrame
                    float _stuckTime = _hostCAC.AutoStuckFrame
                    ? _hostCAC.AnimationFixedTime * 0.25f
                    : _hostCAC.StuckFrameTime;
                    _am.Host.GetActionControl().StuckFrame(_stuckTime);
                    _am.Target.GetActionControl().StuckFrame(_stuckTime);

                    if (_am.Host.CharType == Character.CharacterType.Hero)
                    {
                        Hero _h = (Hero)_am.Host;
                        _h.AddHitCount(1);
                        _h.StopHitCountDecrease();
                        UIManager.Instance.ShowToughNumber(_am.HitPointWorld, (int)_hostCAC.ToughCutting);
                        UIManager.Instance.ShowDamageNumber((int)_hostCAC.Damage);
                    }
                    else if (_am.Target.CharType == Character.CharacterType.Hero)
                    {

                    }

                    //After Stuck

                    _am.Host.GetActionControl().StuckFrameTimer.TimerEnded += AfterStuck;

                    void AfterStuck()
                    {
                        if (_am.Host.CharType == Character.CharacterType.Hero)
                        {
                            PlayerController.Instance.ScreenShake(_am.HitDirection, _hostCAC.Damage * ShakeScreenPowerDamageRate_Player, 0.1f);
                        }
                        else if (_am.Target.CharType == Character.CharacterType.Hero)
                        {
                            PlayerController.Instance.ScreenShake(_am.HitDirection, _hostCAC.Damage * ShakeScreenPowerDamageRate_Enemy, 0.2f);
                        }

                        _am.Host.GetActionControl().StuckFrameTimer.TimerEnded -= AfterStuck;

                        //HitEffect
                        Quaternion _rot = Quaternion.LookRotation(_am.HitPointWorld - _am.Host.GetTransform().position, _am.Target.GetTransform().up);
                        _am.Target.PlayHitEffect(_am.HitPointWorld, _rot);
                    }
                }
            }
        }
    }

    private void Update()
    {
        DealHitEvent();
    }

    public void SendCommand(Character character, ActionCommand combatCommand)
    {
        character.GetActionControl().SendActionCommand(combatCommand);
    }
}
