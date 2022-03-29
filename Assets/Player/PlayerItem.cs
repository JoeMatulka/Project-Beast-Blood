using ResourceManager;
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

    public ItemType Type { get { return this.type; } }
}

public static class PlayerItemLibrary
{
    private const float THROW_FORCE = 25f;

    public static PlayerItem FireBomb
    {
        get
        {
            return new PlayerItem(ItemType.THROW, (Player player) =>
            {
                // Spawn Fire Bomb
                GameObject fb = ProjectileMananger.Instance.FireBomb;
                // Apply force in the aim of the player
                fb?.GetComponent<Rigidbody2D>().AddForce(player.Aim.ToVector * THROW_FORCE);
            });
        }
    }
}
