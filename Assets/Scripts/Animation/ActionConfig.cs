using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ActionConfig")]
public class ActionConfig : ScriptableObject
{
    [Header("动作名")] public string ActionName;
    [Header("动画名")] public string AnimationName;
    [Header("动画层级")] public int AnimationLayer;
    [Header("动画持续时间")] public float AnimationFixedTime;
    [Header("默认衔接动作")] public NextAction DefaultNextAction;
    [Header("动作关系")] public List<NextAction> NextActionSequence;
    [Header("动作类型")] public CollideControlType CollideControl;
    [Header("动作细分类型")] public CombatManager.ActionCommand ActionCommandType;
    [Header("是否需要控制碰撞时间")] public bool IsNeededCollideTime;
    [Header("控制类型")] public CollideControlMethod collideControlMethod;
    [Header("碰撞归一化时间1")][Range(0, 1f)] public float CollideNormalizedTime1;
    [Header("碰撞归一化时间2")][Range(0, 1f)] public float CollideNormalizedTime2;
    [Header("自动碰撞顿帧")] public bool AutoStuckFrame;
    [Header("碰撞顿帧时间")][Range(0, 1f)] public float StuckFrameTime;
    [Header("AC类型")] public ACType ACtype;
    [Header("动作中移动")] public List<ActionMove> actionMoves;
    [Header("处理粒子")] public bool IsControlParticle;
    [Header("粒子特效列表")] public List<Particle> particles;

    [Serializable]
    public enum ACType
    {
        Normal,
        Combat
    }

    [Serializable]
    public struct ActionMove
    {
        public float FixedTime;
        public MoveCoordType coordType;
        public Vector3 MoveVector;
        public float Duration;
        public Ease MoveEase;

        [Serializable]
        public enum MoveCoordType
        {
            Local,
            World
        }
    }

    [Serializable]
    public struct NextAction
    {
        public CombatManager.ActionCommand Command;
        public bool IsBreakable;
        public ActionConfig actionConfig;
    }

    [Serializable]
    public struct Particle
    {
        public int ParticleIndex;
        public ParticleType Type;
        public float Speed;
        public float FixedTime;
        public Vector3 LocalPositionVector;
        public Vector3 LocalRotationVector;
        public Vector3 LocalScaleVector;
    }
    [Serializable]
    public enum ParticleType
    {
        VisualEffect,
        ParticleSystem
    }

    public enum CollideControlMethod
    {
        [InspectorName("白名单模式(之间)")] Whitelist,
        [InspectorName("黑名单模式(之外)")] Blacklist
    }

    public enum CollideControlType
    {
        Attack,
        Dodge,
        None,
        Block
    }

    public ActionConfig()
    {
        AnimationName = "animation";
        AnimationLayer = 0;
        CollideControl = CollideControlType.Attack;
        IsNeededCollideTime = false;
        collideControlMethod = CollideControlMethod.Whitelist;
        CollideNormalizedTime1 = 0;
        CollideNormalizedTime2 = 1;
        ACtype = ACType.Normal;
        AutoStuckFrame = true;
        StuckFrameTime = 0;
        IsControlParticle = false;

    }

    public NextAction Next(CombatManager.ActionCommand Command)
    {
        return NextActionSequence.Find(x => x.Command == Command);
    }
}
