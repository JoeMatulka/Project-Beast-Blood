using CreatureSystems;
using HitboxSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    ONE_HAND, TWO_HAND, RANGED
};

[RequireComponent(typeof(PlayerWeaponAnimator))]
public class PlayerActionController : MonoBehaviour
{
    private PlayerWeaponAnimator animator;
    public Player Player;

    public GameCamera GameCamera;

    // This could be derived from the current equipped weapon instead of assigning a weapon type, assign an actual weapon and get the type from that
    private WeaponType currentWeaponType;
    private Damage currentAttackDamage;

    // Current equipped item
    public PlayerItem CurrentItem;

    // Number key in dictionary is active frame for weapon attack (zero indexed)
    private Dictionary<int, WeaponAttackFrame> weaponAttackFrames;
    // Number key in dictionary is active frame for non-weapon attack (zero indexed)
    private Dictionary<int, ActionFrame> actionFrames;
    private int currentNonWeaponAttackID = 0;
    // Allows for functions not to be called mutliple times during a frame
    private int lastCalledFrame = 0;
    // Length of ray cast when weapon attacks
    private float weaponAttackRayLength;
    private readonly float diagonalWeaponRayMod = .25f;
    private readonly float playerCenterOffset = .25f;
    private readonly float playerCrouchOffect = .15f;

    // Variables for Fatal Attack
    public Creature fatalAttackCreature;
    // Different for first and second strikes in a fatal attack
    public readonly float FATAL_ATK_DMG_MOD = .25f;

    private LayerMask playerLayerMask;

    private void Awake()
    {
        animator = this.GetComponent<PlayerWeaponAnimator>();
        playerLayerMask = ~LayerMask.GetMask("Player", "Ignore Raycast", "Creature");
        Player = this.GetComponentInParent<Player>();

        GameCamera = Camera.main.GetComponent<GameCamera>();
    }

    public void ActivateWeaponAttackFrame(AimDirection direction, int frame)
    {
        animator.SetSpriteByDirectionAndIndex(direction, frame);
        WeaponAttackFrame attackFrame;
        if (weaponAttackFrames.TryGetValue(frame, out attackFrame))
        {
            if (attackFrame.IsActiveHurtBox)
            {
                DrawWeaponRecast(direction);
            }
            if (attackFrame.IsEndOfRecoveryFrame && lastCalledFrame != frame)
            {
                Player.CanCancelAttackAnim = true;
            }
            lastCalledFrame = frame;
        }
    }

    public void ActiveActionFrame(int frame)
    {
        ActionFrame actionFrame;
        if (actionFrames.TryGetValue(frame, out actionFrame))
        {
            if (actionFrame.ActionOnFrame != null && lastCalledFrame != frame)
            {
                actionFrame.ActionOnFrame(this);
            }
            Player.IsInvulnerable = actionFrame.IsInvulnerable;
            if (actionFrame.IsEndOfRecoveryFrame && lastCalledFrame != frame)
            {
                Player.CanCancelAttackAnim = true;
            }
            lastCalledFrame = frame;
        }
    }

    private void DrawWeaponRecast(AimDirection direction)
    {
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
            default:
                break;
        }
        // Flip x axis of aim if player is not facing right
        if (!Player.Controller.FacingRight)
        {
            rayDirection = new Vector3(rayDirection.x * -1, rayDirection.y);
        }

        // Activate weapon hurt box from offset of center of player
        Vector3 center = transform.position + (rayDirection * playerCenterOffset);

        // Adjust center if player is crouching
        if (Player.isCrouching)
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
                hit.collider.GetComponent<Hitbox>()?.ReceiveDamage(currentAttackDamage, Player.transform.position);
                // If the hit is a creature that is staggered, perform a fatal attack
                Creature creature = hit.collider.transform.root.GetComponent<Creature>();
                if (creature != null && creature.IsStaggered)
                {
                    Player.stopInput = true;
                    Player.FatalAttack(creature);
                }
            }
        }
    }

    public void GenerateAttackDamage()
    {
        // TODO Grab damage from weapon
        currentAttackDamage = new Damage(10, DamageType.RAW);
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
                    weaponAttackFrames = WeaponAttackFrameLibrary.ONE_HAND_ATK_FRAMES;
                    weaponAttackRayLength = WeaponAttackFrameLibrary.ONE_HAND_ATK_WEAPON_LENGTH;
                    break;
                default:
                    Debug.LogError("Could not find that weapon type, cannot assign weapon attack frames");
                    break;
            }
        }
    }

    public int CurrentNonWeaponAttackID
    {
        get { return currentNonWeaponAttackID; }
        set
        {
            currentNonWeaponAttackID = value;
            // Assign frames based off of ID
            switch (currentNonWeaponAttackID)
            {
                case ActionLibrary.FATAL_ATK_ID:
                    actionFrames = ActionLibrary.FATAL_ATK_FRAMES;
                    break;
                default:
                    Debug.LogError("Could not find that non weapon attack id, cannot assign attack frames");
                    break;
            }
        }
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
 * Class meant to represent a single frame of an action a player can take, examples being unique attacks without weapons, using items
 */
public struct ActionFrame
{
    // Used for attacks that are cinematic in nature and should not take damage during the animation
    public readonly bool IsInvulnerable;
    // Is this the end of the recovery frame, allows for cancelling animations with movement, etc.
    public readonly bool IsEndOfRecoveryFrame;
    // Action called on frame, can be used for a lot of different use cases. Calls a method on the frame with same ref as this class
    public readonly Action<PlayerActionController> ActionOnFrame;
    public ActionFrame(bool isInvulnerable = false, bool isEndOfRecoveryFrame = false, Action<PlayerActionController> actionOnFrame = null)
    {
        IsInvulnerable = isInvulnerable;
        IsEndOfRecoveryFrame = isEndOfRecoveryFrame;
        ActionOnFrame = actionOnFrame;
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

public static class ActionLibrary
{
    // Fatal attack, a cinematic attack that does large damage independent of the player weapon on a staggered monster
    public const int FATAL_ATK_ID = 1;
    public static Dictionary<int, ActionFrame> FATAL_ATK_FRAMES = new Dictionary<int, ActionFrame> {
        { 1, new ActionFrame(true, false, (PlayerActionController controller) => {
            controller.GameCamera.Zoom(3);
        })},
        { 5, new ActionFrame(true, false, (PlayerActionController controller) => { 
            // Initial Damage to creature cause creature to flinch
            Damage dmg = new Damage(controller.fatalAttackCreature.Stats.BaseHealth * controller.FATAL_ATK_DMG_MOD, DamageType.RAW);
            controller.fatalAttackCreature.Damage(dmg);
            controller.fatalAttackCreature.Flinch();
            controller.fatalAttackCreature.SpawnEffectOnCreature(controller.Player.transform.position, CreatureOnEffect.BloodSpurt);
        })},
        { 13, new ActionFrame(true, false, (PlayerActionController controller) => { 
            // Secondary Damage to creature and force a trip on the creature
            Damage dmg = new Damage(controller.fatalAttackCreature.Stats.BaseHealth * controller.FATAL_ATK_DMG_MOD, DamageType.RAW);
            controller.fatalAttackCreature.Damage(dmg, CreatuePartSystems.CreaturePartDamageModifier.NONE, 1, false, true);
            controller.fatalAttackCreature.SpawnEffectOnCreature(controller.Player.transform.position, CreatureOnEffect.BloodSplash);
            controller.GameCamera.Shake();
        })},
        { 20, new ActionFrame(false, true, (PlayerActionController controller) => {
            // Release input freeze on player
            controller.Player.stopInput = false;
            controller.GameCamera.Zoom(GameCamera.DEFAULT_CAMERA_ZOOM);
        })},
    };
    // Throw item, a non-weapon attack involving throwing an item at the direction the player is aiming

    // Consume item, a non-weapon action that involves consuming the currently equipped item

}
