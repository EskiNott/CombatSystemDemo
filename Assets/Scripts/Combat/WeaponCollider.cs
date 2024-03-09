using UnityEngine;

public class WeaponCollider : CharacterCollider
{
    private bool AttackResource;

    public WeaponCollider()
    {
        type = ColliderType.Weapon;
    }

    protected override void Awake()
    {
        base.Awake();
        AttackResource = false;
    }

    public bool TakeResource()
    {
        if (AttackResource)
        {
            AttackResource = false;
            return true;
        }
        return false;
    }

    public void ReturnResource()
    {
        AttackResource = true;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        CharacterCollider cc = other.GetComponent<CharacterCollider>();
        if (cc != null && cc.type != ColliderType.Weapon && TakeResource())
        {
            CombatManager.AttackModel _am;

            Vector3 _hitDirectionWorld = other.transform.position - transform.position;
            Vector3 _hitDirectionLocal = other.transform.InverseTransformDirection(_hitDirectionWorld);

            _am.Host = character;
            _am.Target = cc.GetCharacter();
            _am.HitDirection = _hitDirectionLocal;
            _am.isCourage = false;
            _am.isBlock = cc.type == ColliderType.Block;
            _am.HitPointWorld = other.ClosestPoint(transform.position);

            if (character.GetActionControl().IsCouraging)
            {
                if (character.Courage >= 100)
                {
                    character.AddCourage(-100);
                    _am.isCourage = true;
                }
            }

            CombatManager.Instance.SendHitEvent(_am);
            Debug.Log("Hit");
        }
    }
}
