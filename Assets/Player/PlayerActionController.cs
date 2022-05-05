using CreatureSystems;
using HitboxSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerActionController : MonoBehaviour
{
    public Player Player;

    public GameCamera GameCamera;

    public PlayerWeaponController WeaponController;

    // Number key in dictionary is active frame for non-weapon attack (zero indexed)
    private Dictionary<int, ActionFrame> actionFrames;
    private int currentActionID = 0;
    // Allows for functions not to be called mutliple times during a frame
    private int lastCalledFrame = 0;

    // Variables for Fatal Attack
    public Creature fatalAttackCreature;
    // Different for first and second strikes in a fatal attack
    public readonly float FATAL_ATK_DMG_MOD = .25f;

    private LayerMask playerLayerMask;

    private void Awake()
    {
        playerLayerMask = ~LayerMask.GetMask("Player", "Ignore Raycast", "Creature");
        Player = this.GetComponent<Player>();

        GameCamera = Camera.main.GetComponent<GameCamera>();

        WeaponController = transform.Find("Weapon").GetComponent<PlayerWeaponController>();
    }

    public void ActivateMainWeaponAction()
    {
        int aim = (int)Player.Aim.ToEnum;
        Player.Animator.SetInteger("Aim", aim);
        Player.Animator.SetTrigger("WeaponAction");
        WeaponController.ActivateWeaponAttackAnimation(aim);
    }

    public void ActivateAttackCancelAnimation()
    {
        Player.Animator.SetTrigger("CancelAnimation");
        // Needed because cancelling doesn't work for some reason...
        WeaponController.Animator.Rebind();
        WeaponController.Animator.Update(0f);
    }

    public void ActivateWeaponAttackFrame(Vector2 direction, int frame)
    {
        WeaponAttackFrame attackFrame;
        if (WeaponController.WeaponAttackFrames.TryGetValue(frame, out attackFrame))
        {
            if (attackFrame.IsActiveHurtBox)
            {
                WeaponController.DrawWeaponRaycast(direction, playerLayerMask);
            }
            if (attackFrame.IsEndOfRecoveryFrame && lastCalledFrame != frame)
            {
                Player.CanCancelAnim = true;
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
                Player.CanCancelAnim = true;
            }
            lastCalledFrame = frame;
        }
    }

    public void GenerateAttackDamage()
    {
        // This is to ensure unique damages are being applied to anything with a hitbox
        Player.EquippedWeapon.Damage.GenerateNewGuid();
    }

    public void EndAttackOrAction()
    {
        lastCalledFrame = 0;
        WeaponController.EndWeaponAttack();
    }

    public int CurrentNonWeaponAttackID
    {
        get { return currentActionID; }
        set
        {
            currentActionID = value;
            // Assign frames based off of ID
            switch (currentActionID)
            {
                case ActionLibrary.FATAL_ATK_ID:
                    actionFrames = ActionLibrary.FATAL_ATK_FRAMES;
                    break;
                case ActionLibrary.THROW_ID:
                    actionFrames = ActionLibrary.THROW_FRAMES;
                    break;
                case ActionLibrary.CONSUME_ID:
                    actionFrames = ActionLibrary.CONSUME_FRAMES;
                    break;
                default:
                    Debug.LogError("Could not find that non weapon attack id, cannot assign attack frames");
                    break;
            }
        }
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

public static class ActionLibrary
{
    // Fatal attack, a cinematic attack that does large damage independent of the player weapon on a staggered monster
    public const int FATAL_ATK_ID = 1;
    public static Dictionary<int, ActionFrame> FATAL_ATK_FRAMES = new Dictionary<int, ActionFrame> {
        { 1, new ActionFrame(true, false, (PlayerActionController controller) => {
            controller.WeaponController.HideHolsteredWeapon();
            controller.GameCamera.Zoom(3);
        })},
        { 5, new ActionFrame(true, false, (PlayerActionController controller) => { 
            // Initial Damage to creature cause creature to flinch
            Damage dmg = new Damage(controller.fatalAttackCreature.Stats.BaseHealth * controller.FATAL_ATK_DMG_MOD, DamageElementType.RAW);
            controller.fatalAttackCreature.Damage(dmg);
            controller.fatalAttackCreature.Flinch();
            controller.fatalAttackCreature.SpawnEffectOnCreature(controller.Player.transform.position, CreatureOnEffect.BloodSpurt);
        })},
        { 13, new ActionFrame(true, false, (PlayerActionController controller) => { 
            // Secondary Damage to creature and force a trip on the creature
            Damage dmg = new Damage(controller.fatalAttackCreature.Stats.BaseHealth * controller.FATAL_ATK_DMG_MOD, DamageElementType.RAW);
            controller.fatalAttackCreature.Damage(dmg, CreatuePartSystems.CreaturePartDamageModifier.NONE, 1, false, true);
            controller.fatalAttackCreature.SpawnEffectOnCreature(controller.Player.transform.position, CreatureOnEffect.BloodSplash);
            controller.GameCamera.Shake();
        })},
        { 20, new ActionFrame(false, true, (PlayerActionController controller) => {
            // Release input freeze on player
            controller.WeaponController.SetHolsteredWeaponSprite();
            controller.Player.stopInput = false;
            controller.GameCamera.Zoom(GameCamera.DEFAULT_CAMERA_ZOOM);
        })},
    };
    // Throw item, a non-weapon attack involving throwing an item at the direction the player is aiming
    public const int THROW_ID = 2;
    public static Dictionary<int, ActionFrame> THROW_FRAMES = new Dictionary<int, ActionFrame> {
        { 5, new ActionFrame(false, false, (PlayerActionController controller) => {
            // Activate throwable equipped to player
            controller.Player.ThrowItem();
        })},
    };
    // Consume item, a non-weapon action that involves consuming the currently equipped item
    public const int CONSUME_ID = 3;
    public static Dictionary<int, ActionFrame> CONSUME_FRAMES = new Dictionary<int, ActionFrame> {
        { 10, new ActionFrame(false, false, (PlayerActionController controller) => {
            // Activate consumable equipped to player
            controller.Player.ConsumeItem();
        })},
    };
}
