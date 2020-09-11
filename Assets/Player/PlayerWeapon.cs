using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    ONE_HAND, TWO_HAND, SHIELD, POLEARM, RANGED
};

[RequireComponent(typeof(PlayerWeaponAnimator))]
public class PlayerWeapon : MonoBehaviour
{
    private PlayerWeaponAnimator animator;

    // This could be derived from the current equipped weapon instead of assigning a weapon type, assign an actual weapon and get the type from that
    private WeaponType currentWeaponType;

    // Number key in dictionary is active frame for weapon attack (zero indexed)
    private Dictionary<int, WeaponAttackFrame> weaponAttackFrames;
    // Length of ray cast when weapon attacks
    private float weaponAttackRayLength;

    private LayerMask playerLayerMask;

    private void Awake()
    {
        animator = this.GetComponent<PlayerWeaponAnimator>();
        playerLayerMask = ~LayerMask.GetMask("Player");
    }

    public void ActivateWeaponAttackFrame(AimDirection direction, int frame)
    {
        animator.SetSpriteByDirectionAndIndex(direction, frame);
        WeaponAttackFrame attackFrame;
        if (weaponAttackFrames.TryGetValue(frame, out attackFrame))
        {
            // Activate weapon hurt box
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, weaponAttackRayLength, playerLayerMask);
            Debug.DrawRay(transform.position, Vector2.right * weaponAttackRayLength, Color.green);
            if (hit.collider != null)
            {
                // Made contact with hit
            }
        }
    }

    public void EndAttack()
    {
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
                    weaponAttackFrames = WeaponClassLibrary.ONE_HAND_ATK_FRAMES;
                    weaponAttackRayLength = WeaponClassLibrary.ONE_HAND_ATK_WEAPON_LENGTH;
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

/**
 * Class meant to hold the weapon type data by archtype, not intended for specific weapons, but for specific weapon types
 */
public class WeaponClassLibrary
{
    private WeaponClassLibrary()
    {
        //Private constructor because class is not meant to be created
    }

    // One handed weapon
    public static Dictionary<int, WeaponAttackFrame> ONE_HAND_ATK_FRAMES = new Dictionary<int, WeaponAttackFrame> {
        { 2, new WeaponAttackFrame(false, true) },
        { 3, new WeaponAttackFrame(false, true) },
    };
    public static float ONE_HAND_ATK_WEAPON_LENGTH = 1;
}
