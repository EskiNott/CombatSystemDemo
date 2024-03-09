using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollider : CharacterCollider
{
    public BlockCollider()
    {
        type = ColliderType.Block;
    }
}
