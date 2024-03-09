using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class AnimationControl : MonoBehaviour
{
    private Animator animator;
    private List<AnimationConfig.AnimationInfo> animationInfo;
    [SerializeField] private List<int> animationStateHash;
    [SerializeField] private int currentStateHash;
    [SerializeField] private bool IsActionConfigUpdated;
    private ActionControl actionControl;
    [SerializeField] float finishPercent = 0.89f;
    private Character character;

    private void Start()
    {
        character = GetComponent<Character>();
        animator = GetComponent<Animator>();
        animationInfo = GetComponent<Character>().GetAnimationConfig().animations;
        animationStateHash = new();
        foreach (var item in animationInfo)
        {
            animationStateHash.Add(Animator.StringToHash(item.AnimationName));
        }
        actionControl = GetComponent<ActionControl>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    public void UpdateAnimation()
    {
        if (IsActionConfigUpdated)
        {
            CrossFade(actionControl.GetCurrentActionConfig());
            IsActionConfigUpdated = false;
        }
    }

    public void Pause(bool isPause)
    {
        if (isPause)
        {
            animator.speed = 0;
        }
        else
        {
            animator.speed = 1;
        }
    }

    private void CrossFade(ActionConfig info)
    {
        if (info == null) { return; }
        int _stateIndex = AnimationNameToIndex(info.AnimationName);
        int _stateHash = IndexToStateHash(_stateIndex);

        animator.CrossFadeInFixedTime
        (
            _stateHash,
            animationInfo[_stateIndex].ReleaseTime,
            info.AnimationLayer,
            animationInfo[_stateIndex].TimeOffset,
            animationInfo[_stateIndex].TransitionTime
        );

        currentStateHash = _stateHash;
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public List<AnimationConfig.AnimationInfo> GetActionAnimation()
    {
        return animationInfo;
    }

    public int IndexToStateHash(int stateIndex)
    {
        return animationStateHash[stateIndex];
    }

    public int GetCurrentState()
    {
        return currentStateHash;
    }

    public int AnimationNameToIndex(string animationName)
    {
        return animationInfo.FindIndex(x => x.AnimationName == animationName);
    }

    public void ActionConfigUpdated()
    {
        IsActionConfigUpdated = true;
    }

}
