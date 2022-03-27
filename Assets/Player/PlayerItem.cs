using System;
using UnityEngine;

public enum ItemType
{
    THROW, CONSUME, WEAR, TRAP
}

public struct PlayerItem
{
    private readonly ItemType type;
    public readonly Action<Player> Activate;

    public PlayerItem(ItemType type, Action<Player> activateAction)
    {
        this.type = type;
        this.Activate = activateAction;
    }
}
