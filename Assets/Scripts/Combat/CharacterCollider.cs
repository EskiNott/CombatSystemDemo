using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CharacterCollider : MonoBehaviour
{
    protected Character character;
    [HideInInspector] public ColliderType type;
    protected Collider thisCollider;

    public CharacterCollider()
    {
        type = ColliderType.Normal;
    }
    public enum ColliderType
    {
        Normal,
        Weapon,
        Block
    }

    virtual protected void Awake()
    {
        if (character == null)
        {
            GetParentCharacter();
        }
        thisCollider = GetComponent<Collider>();
    }
    protected void GetParentCharacter()
    {
        int times = 0;
        Transform _target = transform;
        while (_target.GetComponent<Character>() == null && times <= 50)
        {
            _target = _target.parent;
            times++;
        }
        character = _target.GetComponent<Character>();
    }

    public Character GetCharacter()
    {
        return character;
    }

    public void SetColliderEnable(bool enable)
    {
        thisCollider.enabled = enable;
    }

    virtual protected void OnTriggerEnter(Collider other)
    {

    }
}
