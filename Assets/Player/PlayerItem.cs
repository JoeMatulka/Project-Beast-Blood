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
    private readonly GameObject prefab;

    public PlayerItem(ItemType type, GameObject prefab)
    {
        this.type = type;
        this.prefab = prefab;
    }

    public ItemType Type { get { return this.type; } }

    public GameObject Prefab { get { return this.prefab; } }
}

public static class PlayerItemLibrary
{
    public static PlayerItem FireBomb
    {
        get
        {
            return new PlayerItem(ItemType.THROW, PlayerItemMananger.Instance.FireBomb);
        }
    }

    public static PlayerItem Medicine
    {
        get
        {
            return new PlayerItem(ItemType.CONSUME, PlayerItemMananger.Instance.Medicine);
        }
    }
}
