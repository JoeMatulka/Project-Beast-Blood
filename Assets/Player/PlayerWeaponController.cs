using HitboxSystem;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    ONE_HAND, TWO_HAND, SHIELD, POLEARM, RANGED
};

[RequireComponent(typeof(PlayerWeaponAnimator))]
public class PlayerWeaponController : MonoBehaviour
{
    private PlayerWeaponAnimator animator;
    private Player player;

    // This could be derived from the current equipped weapon instead of assigning a weapon type, assign an actual weapon and get the type from that
    private WeaponType currentWeaponType;
    private Damage currentAttackDamage;

    // Number key in dictionary is active frame for weapon attack (zero indexed)
    private Dictionary<int, WeaponAttackFrame> weaponAttackFrames;
    // Allows for functions not to be called mutliple times during a frame
    private int lastCalledFrame = 0;
    // Length of ray cast when weapon attacks
    private float weaponAttackRayLength;
    private readonly float diagonalWeaponRayMod = .25f;
    private readonly float playerCenterOffset = .25f;
    private readonly float playerCrouchOffect = .15f;

    private LayerMask playerLayerMask;

    private void Awake()
    {
        animator = this.GetComponent<PlayerWeaponAnimator>();
        playerLayerMask = ~LayerMask.GetMask("Player", "Ignore Raycast", "Creature");
        player = this.GetComponentInParent<Player>();
    }

    public void ActivateWeaponAttackFrame(AimDirection direction, int frame)
    {
        animator.SetSpriteByDirectionAndIndex(direction, frame);
        WeaponAttackFrame attackFrame;
        if (weaponAttackFrames.TryGetValue(frame, out attackFrame))
        {
            if (attackFrame.IsActiveHurtBox) {
                DrawWeaponRecast(direction);
            }
            if (attackFrame.IsEndOfRecoveryFrame && lastCalledFrame != frame) {
                player.CanCancelAttackAnim = true;
            }
            lastCalledFrame = frame;
        }
    }

    private void DrawWeaponRecast(AimDirection direction) {
        //Determine Vector Direction based off of Aim Direction (It's not part of the enum since the AimDirection is used by the animator)
        Vector3 rayDirection = Vector3.zero;
        float attackLength = weaponAttackRayLength;
        switch (direction)
        {
            case AimDirection.UP:
                rayDirection = Vector3.up;
                break;
            case AimDirection.UP_DIAG:
                rayDirection = new Vector3(1, 1);
                // This is because rays are drawn longer at a diagonal angle from origin
                attackLength -= diagonalWeaponRayMod;
                break;
            case AimDirection.STRAIGHT:
                rayDirection = Vector3.right;
                break;
            case AimDirection.DOWN_DIAG:
                rayDirection = new Vector3(1, -1);
                // This is because rays are drawn longer at a diagonal angle from origin
                attackLength -= diagonalWeaponRayMod;
                break;
            case AimDirection.DOWN:
                rayDirection = Vector3.down;
                break;
        }
        // Flip x axis of aim if player is not facing right
        if (!player.Controller.FacingRight)
        {
            rayDirection = new Vector3(rayDirection.x * -1, rayDirection.y);
        }

        // Activate weapon hurt box from offset of center of player
        Vector3 center = transform.position + (rayDirection * playerCenterOffset);

        // Adjust center if player is crouching
        if (player.isCrouching)
        {
            center = new Vector3(center.x, center.y - playerCrouchOffect);
        }


        RaycastHit2D[] hits = Physics2D.RaycastAll(center, rayDirection, attackLength, playerLayerMask);
        Debug.DrawRay(center, rayDirection * attackLength, Color.green);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                hit.collider.GetComponent<Hitbox>()?.ReceiveDamage(currentAttackDamage, player.transform.position);
            }
        }
    }

    public void GenerateAttackDamage()
    {
        // TODO Grab damage from weapon
        currentAttackDamage = new Damage(10, DamageType.POISON);
    }

    public void EndAttack()
    {
        animator.ClearSprite();
        lastCalledFrame = 0;
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
public struct WeaponAttackFrame
{
    // Is this the end of the recovery frame, allows for cancelling animations with movement, etc.
    public readonly bool IsEndOfRecoveryFrame;
    // Is the weapon hurt box active this frame?
    public readonly bool IsActiveHurtBox;

    public WeaponAttackFrame(bool isActiveHurtBox, bool IsEndOfRecoveryFrame)
    {
        this.IsEndOfRecoveryFrame = IsEndOfRecoveryFrame;
        this.IsActiveHurtBox = isActiveHurtBox;
    }
}

/**
 * Class meant to hold the weapon type data by archtype, not intended for specific weapons, but for specific weapon types
 */
public static class WeaponClassLibrary
{
    // One handed weapon
    public static Dictionary<int, WeaponAttackFrame> ONE_HAND_ATK_FRAMES = new Dictionary<int, WeaponAttackFrame> {
        { 5, new WeaponAttackFrame(true, false) },
        { 12, new WeaponAttackFrame(false, true) },
    };
    public static float ONE_HAND_ATK_WEAPON_LENGTH = .7f;
}
