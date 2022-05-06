using ResourceManager;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    THROW, CONSUME, WEAR, TRAP
}

public class PlayerItem
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
    // Item IDs
    public readonly static int FIREBOMB_ID = 1;
    public readonly static int MEDICINE_ID = 2;

    // Library of weapons
    public static Dictionary<int, PlayerItem> Items = new Dictionary<int, PlayerItem> {
        { FIREBOMB_ID, new PlayerItem(ItemType.THROW, PlayerItemMananger.Instance.FireBomb) },
        { MEDICINE_ID, new PlayerItem(ItemType.CONSUME, PlayerItemMananger.Instance.Medicine)}
    };

    public static int GetItemStackSize(ItemType type)
    {
        switch (type)
        {
            case ItemType.THROW:
                return 2;
            case ItemType.CONSUME:
                return 5;
            case ItemType.WEAR:
            case ItemType.TRAP:
                return 1;
            default:
                return 0;
        }
    }
}
