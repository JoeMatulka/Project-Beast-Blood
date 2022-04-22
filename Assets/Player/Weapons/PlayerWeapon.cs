public enum WeaponType
{
    ONE_HAND, TWO_HAND, RANGED
};

public struct PlayerWeapon
{
    public readonly Damage Damage;

    public readonly WeaponType Type;

    public PlayerWeapon(Damage damage, WeaponType type)
    {
        this.Damage = damage;
        this.Type = type;
    }
}

public static class PlayerWeaponLibrary
{
    public static PlayerWeapon HUNTER_SWORD
    {
        get { return new PlayerWeapon(new Damage(10, DamageType.RAW), WeaponType.ONE_HAND); }
    }
}
