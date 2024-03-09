using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/CombatActionConfig")]
public class CombatActionConfig : ActionConfig
{
    [Header("伤害")] public float Damage;
    [Header("削韧")] public float ToughCutting;
    [Header("击中时获得勇气值")] public float NormalCourageGain;
    [Header("勇气击中时获得勇气值")] public float PowerCourageGain;
    [Header("招式派生超时")][Range(0f, 4f)] public float TimeoutNormalized;
    [Header("触发距离")] public float AttackDistance;

    public CombatActionConfig()
    {
        Damage = 0;
        ToughCutting = 0;
        NormalCourageGain = 0;
        TimeoutNormalized = 0.3f;
        CollideControl = CollideControlType.Attack;
        ACtype = ACType.Combat;
        AttackDistance = 0;
    }
}
