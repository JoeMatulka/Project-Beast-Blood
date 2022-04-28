using CreatureSystems;
using HitboxSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerWeaponController : MonoBehaviour
{
    private Player player;

    public Animator Animator;

    // Number key in dictionary is active frame for weapon attack (zero indexed)
    public Dictionary<int, WeaponAttackFrame> WeaponAttackFrames;
    // Length of ray cast when weapon attacks
    private float weaponAttackRayLength;
    private readonly float diagonalWeaponRayMod = .25f;
    private readonly float playerCenterOffset = .25f;
    private readonly float playerCrouchOffect = .15f;

    void Awake()
    {
        // Grab player and equipped weapon
        player = this.GetComponentInParent<Player>();
        AssignAttackFrames();

        Animator = this.GetComponent<Animator>();
    }

    public void DrawWeaponRaycast(Vector2 direction, LayerMask playerLayerMask)
    {
        //Determine Vector Direction based off of Aim Direction (It's not part of the enum since the AimDirection is used by the animator)
        Vector3 rayDirection = direction;
        float attackLength = weaponAttackRayLength;
        if (rayDirection.Equals(new Vector2(1, 1)) || rayDirection.Equals(new Vector2(1, -1)))
        {
            // This is because rays are drawn longer at a diagonal angle from origin
            attackLength -= diagonalWeaponRayMod;
        }
        // Activate weapon raycast from offset of center of player
        Vector2 center = transform.position + (rayDirection * playerCenterOffset);

        // Adjust center if player is crouching
        if (player.IsCrouching)
        {
            center = new Vector3(center.x, center.y - playerCrouchOffect);
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(center, rayDirection, attackLength, playerLayerMask);
        Debug.DrawRay(center, rayDirection * attackLength, Color.green);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // If the hit has a hitbox to receive damage, then damage it
                hit.collider.GetComponent<Hitbox>()?.ReceiveDamage(player.EquippedWeapon.Damage, player.transform.position);
                // If the hit is a creature that is staggered, perform a fatal attack
                Creature creature = hit.collider.transform.root.GetComponent<Creature>();
                if (creature != null && creature.IsStaggered)
                {
                    player.stopInput = true;
                    player.FatalAttack(creature);
                }
            }
        }
    }


    public void AssignAttackFrames()
    {
        // Assign weapon attack frames from the set current weapon type
        switch (player.EquippedWeapon.Type)
        {
            case WeaponType.ONE_HAND:
                WeaponAttackFrames = WeaponAttackFrameLibrary.ONE_HAND_ATK_FRAMES;
                weaponAttackRayLength = WeaponAttackFrameLibrary.ONE_HAND_ATK_WEAPON_LENGTH;
                break;
            default:
                Debug.LogError("Could not find that weapon type, cannot assign weapon attack frames");
                break;
        }

    }

    public void EndWeaponAttack()
    {

    }
}

/**
 * Class meant to represent a single frame of attack within an weapon attack animation frame
 */
public struct WeaponAttackFrame
{
    // Is this the end of the recovery frame, allows for cancelling animations with movement, etc.
    public readonly bool IsEndOfRecoveryFrame;
    // Is the weapon hurt box active this frame?
    public readonly bool IsActiveHurtBox;

    public WeaponAttackFrame(bool isActiveHurtBox, bool isEndOfRecoveryFrame)
    {
        IsEndOfRecoveryFrame = isEndOfRecoveryFrame;
        IsActiveHurtBox = isActiveHurtBox;
    }
}

/**
 * Class meant to hold the weapon type data by archtype, not intended for specific weapons, but for specific weapon types
 */
public static class WeaponAttackFrameLibrary
{
    // One handed weapon
    public static Dictionary<int, WeaponAttackFrame> ONE_HAND_ATK_FRAMES = new Dictionary<int, WeaponAttackFrame> {
        { 5, new WeaponAttackFrame(true, false) },
        { 12, new WeaponAttackFrame(false, true) },
    };
    public static float ONE_HAND_ATK_WEAPON_LENGTH = .8f;
}
