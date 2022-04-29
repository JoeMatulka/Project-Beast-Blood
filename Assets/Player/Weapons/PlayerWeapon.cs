using System.Collections.Generic;

public enum WeaponType
{
    ONE_HAND, TWO_HAND, RANGED
};

public enum WeaponSpriteType
{
    SWORD, MACE, AXE, GUN, CROSSBOW
}

public struct PlayerWeapon
{
    public readonly Damage Damage;

    public readonly WeaponType Type;

    public readonly WeaponSpriteType Sprite;

    public PlayerWeapon(Damage damage, WeaponType type, WeaponSpriteType sprite)
    {
        this.Damage = damage;
        this.Type = type;
        this.Sprite = sprite;
    }
}

public static class PlayerWeaponLibrary
{
    // IDs of weapons
    public static int IRON_SWORD_ID = 1;

    // Library of weapons
    public static Dictionary<int, PlayerWeapon> Weapons = new Dictionary<int, PlayerWeapon> {
        { IRON_SWORD_ID, new PlayerWeapon(new Damage(50, DamageType.RAW), WeaponType.ONE_HAND, WeaponSpriteType.SWORD) },
    };
}
