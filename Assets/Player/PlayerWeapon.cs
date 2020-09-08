using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    ONE_HAND, TWO_HAND, SHIELD, POLEARM, RANGED
};

[RequireComponent (typeof(PlayerWeaponAnimator))]
public class PlayerWeapon : MonoBehaviour
{
    private PlayerWeaponAnimator animator;

    // This could be derived from the current equipped weapon instead of assigning a weapon type, assign an actual weapon and get the type from that
    private WeaponType currentWeaponType;

    // Number key in dictionary is active frame for weapon attack (zero indexed)
    private Dictionary<int, WeaponAttackFrame> weaponAttackFrames;

    private readonly Dictionary<int, WeaponAttackFrame> ONE_HAND_ATK_FRAMES = new Dictionary<int, WeaponAttackFrame> {
        { 2, new WeaponAttackFrame(false, true) },
        { 3, new WeaponAttackFrame(false, true) },
    };

    private void Awake()
    {
        animator = this.GetComponent<PlayerWeaponAnimator>();
    }

    public void ActivateWeaponAttackFrame(AimDirection direction, int frame)
    {
        animator.SetSpriteByDirectionAndIndex(direction, frame);
        WeaponAttackFrame attackFrame;
        if (weaponAttackFrames.TryGetValue(frame, out attackFrame))
        {
            // Activate weapon hurt box
        }
    }

    public void EndAttack() {
        animator.ClearSprite();
    }

    public WeaponType CurrentWeaponType
    {
        get { return currentWeaponType; }
        set
        {
            currentWeaponType = value;
            // Assign weapon attack frames from the set current weapon type
            switch (currentWeaponType)
            {
                case WeaponType.ONE_HAND:
                    weaponAttackFrames = ONE_HAND_ATK_FRAMES;
                    break;
                default:
                    Debug.LogError("Could not find that weapon type, cannot assign weapon attack frames");
                    break;
            }
        }
    }
}

/**
 * Class meant to represent a single frame of attack within an attack animation frame
 */
public class WeaponAttackFrame
{
    // Does frame have armor to protect from interupt? Example: 2H weapons should be able to withstand some damage without attacks being interuptted
    public readonly bool IsFrameArmor;
    // Is the weapon hurt box active this frame?
    public readonly bool IsActiveHurtBox;

    public WeaponAttackFrame(bool isFrameArmor, bool isActiveHurtBox)
    {
        IsFrameArmor = isFrameArmor;
        IsActiveHurtBox = isActiveHurtBox;
    }
}
