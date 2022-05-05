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
    public readonly string Name;

    public readonly Damage Damage;

    public readonly WeaponType Type;

    public readonly WeaponSpriteType Sprite;

    public PlayerWeapon(string name, Damage damage, WeaponType type, WeaponSpriteType sprite)
    {
        this.Name = name;
        this.Damage = damage;
        this.Type = type;
        this.Sprite = sprite;
    }
}

public static class PlayerWeaponLibrary
{
    // IDs of weapons
    public readonly static int IRON_SWORD_ID = 1;
    public readonly static int IRON_MACE_ID = 2;
    public readonly static int IRON_AXE_ID = 3;
    public readonly static int WARM_IRON_SWORD_ID = 4;
    public readonly static int RUSTY_IRON_SWORD_ID = 5;

    // Library of weapons
    public static Dictionary<int, PlayerWeapon> Weapons = new Dictionary<int, PlayerWeapon> {
        { IRON_SWORD_ID, new PlayerWeapon("Iron Sword", new Damage(50, DamageElementType.RAW, new Dictionary<DamageModType, float>() {
            { DamageModType.STRIKE, 1f}, { DamageModType.CHOP, 1f}, { DamageModType.SHARP, 3f},
        }), WeaponType.ONE_HAND, WeaponSpriteType.SWORD) },
        // TODO Change Sprite type to mace when sprites exist
        { IRON_MACE_ID, new PlayerWeapon("Iron Mace", new Damage(50, DamageElementType.RAW, new Dictionary<DamageModType, float>() {
            { DamageModType.STRIKE, 3f}, { DamageModType.CHOP, 1f}, { DamageModType.SHARP, 1f},
        }), WeaponType.ONE_HAND, WeaponSpriteType.SWORD) },
        // TODO Change Sprite type to axe when sprites exist
        { IRON_AXE_ID, new PlayerWeapon("Iron Axe", new Damage(50, DamageElementType.RAW, new Dictionary<DamageModType, float>() {
            { DamageModType.STRIKE, 1f}, { DamageModType.CHOP, 3f}, { DamageModType.SHARP, 1f},
        }), WeaponType.ONE_HAND, WeaponSpriteType.SWORD) },
        { WARM_IRON_SWORD_ID, new PlayerWeapon("Warm Iron Sword", new Damage(50, DamageElementType.FIRE, new Dictionary<DamageModType, float>() {
            { DamageModType.STRIKE, 1f}, { DamageModType.CHOP, 1f}, { DamageModType.SHARP, 1f},
        }), WeaponType.ONE_HAND, WeaponSpriteType.SWORD) },
        { RUSTY_IRON_SWORD_ID, new PlayerWeapon("Rusty Iron Sword", new Damage(50, DamageElementType.POISON, new Dictionary<DamageModType, float>() {
            { DamageModType.STRIKE, 1f}, { DamageModType.CHOP, 1f}, { DamageModType.SHARP, 1f},
        }), WeaponType.ONE_HAND, WeaponSpriteType.SWORD) }
    };
}
