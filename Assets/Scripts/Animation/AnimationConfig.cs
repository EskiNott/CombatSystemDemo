using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObject/AnimationConfig")]
[Serializable]
public class AnimationConfig : ScriptableObject
{
    public List<AnimationInfo> animations;

    [Serializable]
    public struct AnimationInfo
    {
        public string AnimationName;
        [Range(0f, 10f)] public float ReleaseTime;
        [Range(0f, 10f)] public float TransitionTime;
        [Range(0f, 10f)] public float TimeOffset;
        [Range(0f, 4f)] public float PlaySpeedMultiplier;
        [Range(0, 3)] public int Layer;
        public float Threshold;
    }
}
